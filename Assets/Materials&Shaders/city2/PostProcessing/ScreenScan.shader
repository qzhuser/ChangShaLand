Shader "City/ScreenScan"
{
    Properties
    {
        _MainTex ("_ScreenTex", 2D) = "white" {}
        _DepthWidth("LineWidth",float)=1.0
        _Speed("Speed",float)=1.0
        [HDR]_Color("Color",Color)=(1.0,1.0,1.0,1.0)
        _RangeMin("RangeMin",float)=1.0
        _RangeMax("RangeMax",float)=1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

            
            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_TexelSize;
            CBUFFER_END

            float4x4 _FrustumCornersRay;
            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            //sampler2D _CameraOpaqueTexture;

            float _DepthWidth;
            float _Speed;
            float4 _Color;
            float _RangeMin;
            float _RangeMax;

            struct Attributes
            {
                float4 positionOS: POSITION;
                float2 uv:TEXCOORD0;
                
            };
            
            struct Varyings
            {
                float4 vertex: SV_POSITION;
                float4 srcPos:TEXCOORD0;
                float4 interpolatedRay:TEXCOORD1;
                float2 uv_depth:TEXCOORD2;
                float2 uv[9]:TEXCOORD3;

            };

            //计算灰度值
            half luminance(half4 color){
                return 0.2125*color.r+0.7154*color.g+0.0721*color.b;
            }

            //Sobel算子检测边缘
            half Sobel(Varyings i){
                const half Gx[9]={-1,-2,-1,
                                  0,0,0,
                                  1,2,1};
                const half Gy[9]={-1,0,1,
                                  -2,0,2,
                                  -1,0,1};

                half texColor;
                half edgeX=0;
                half edgeY=0;
                for(int it=0;it<9;it++){
                    //texColor=luminance(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv[it]));
                    texColor=SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv[it]);
                    edgeX+=texColor*Gx[it];
                    edgeY+=texColor*Gy[it];
                }

                return 1-abs(edgeX)-abs(edgeY);
            }

            
            Varyings vert (Attributes v)
            {
                Varyings o=(Varyings)0;
                o.vertex=TransformObjectToHClip(v.positionOS);
                o.srcPos=ComputeScreenPos(o.vertex);
                o.uv_depth=v.uv;

                #if UNITY_UV_STARTS_AT_TOP
                    if(_MainTex_TexelSize.y<0)
                        o.uv_depth.y=1-o.uv_depth;
                #endif
                //判断位置 赋值四条射线
                int index=0;
                if(v.uv.x<0.5&&v.uv.y<0.5){
                    index=0;
                }else if(v.uv.x>0.5&&v.uv.y<0.5){
                    index=1;
                }else if(v.uv.x>0.5&&v.uv.y>0.5){
                    index=2;
                }else {
                    index=3;
                }
                //判断不同平台下的的射线
                #if UNITY_UV_STARTS_AT_TOP
                    if(_MainTex_TexelSize.y<0)
                        index=3-index;
                #endif
                o.interpolatedRay=_FrustumCornersRay[index];

                half2 uv=v.uv;

                o.uv[0]=uv+_MainTex_TexelSize.xy*half2(-1,-1);
                o.uv[1]=uv+_MainTex_TexelSize.xy*half2(0,-1);
                o.uv[2]=uv+_MainTex_TexelSize.xy*half2(1,-1);
                o.uv[3]=uv+_MainTex_TexelSize.xy*half2(-1,0);
                o.uv[4]=uv+_MainTex_TexelSize.xy*half2(0,0);
                o.uv[5]=uv+_MainTex_TexelSize.xy*half2(1,0);
                o.uv[6]=uv+_MainTex_TexelSize.xy*half2(-1,1);
                o.uv[7]=uv+_MainTex_TexelSize.xy*half2(0,-1);
                o.uv[8]=uv+_MainTex_TexelSize.xy*half2(1,1);

                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                half edge=1-Sobel(i);

                half4 col=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv[4]);
                //half gray=luminance(col);
                //return lerp(_Color,col,edge);
                //return half4(edge,edge,edge,1);
                float2 srcPos=i.srcPos.xy/i.srcPos.w;
                float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv_depth).r;
                //获取视角空间下的0~1的线性深度值
                float depthValue = Linear01Depth(depth, _ZBufferParams);

                float3 center=float3(0,0,0);

                //float curVal = frac(_Time.y*_Speed);
                //float showCol = saturate(abs(curVal - depthValue) / _DepthWidth/2);
                //float3 finalColor = lerp(_Color.rgb, col.rgb, showCol);
                
                float4 fcol;
                half percent=1;
                float3 worldPos=_WorldSpaceCameraPos+depthValue* i.interpolatedRay.xyz;
                float dis=distance(worldPos,center);
                float curVal = frac(dis*0.03-_Time.y*_Speed);
                float _Range=_RangeMin+_RangeMax*curVal;
                if(_Range-dis>0&&depthValue<1&&depthValue>0){
                    percent=saturate((_Range-dis)/_DepthWidth);//越靠近0 越在圆的最边缘
                }
                col+=_Color*edge*(1-percent);
                //fcol=lerp(_Color*edge,col,percent);

                //return float4(edge,edge,edge,1);
                //return float4(finalColor, 1);
                return col;//float4(worldPos,1);
            }
            ENDHLSL
        }
    }
}
