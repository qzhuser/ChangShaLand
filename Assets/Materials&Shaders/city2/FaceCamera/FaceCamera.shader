Shader "City/FaceCamera"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_MainColor("MainColor",Color)=(1,1,1,1)
        _VerticalBillboarding("VerticalBillboarding",float)=0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                float4 _MainTex_ST;
                SAMPLER(sampler_MainTex);

                float4 _MainColor;
                float _VerticalBillboarding;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                float3 center=float3(0,0,0);
                float3 viewDir= mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz-v.vertex.xyz;
                float3 normalDir=viewDir;
                normalDir.y*=_VerticalBillboarding;//如果_VerticalBillboarding为0，固定法线方向的y轴，只能横向不能纵向转动
                normalDir=normalize(normalDir);

                float3 upDir=abs(normalDir.y)>0.999?float3(0,0,1):float3(0,1,0);
                float3 rightDir=normalize(cross(upDir,normalDir));
                upDir=normalize(cross(normalDir,rightDir));
                
                //算出原始状态 每个顶点与中心的偏移
                float3 centerOffset=v.vertex.xyz-center;
                float3 localPos=center+rightDir*centerOffset.x+upDir*centerOffset.y+normalDir*centerOffset.z;

                o.vertex = TransformObjectToHClip(float4(localPos,1));
                //if(v.vertex.x<0.5&&v.vertex.y<0.5)
                //    v.uv=float2(0,0);
                //else if(v.vertex.x>0.5&&v.vertex.y<0.5)
                //    v.uv=float2(1,0);
                //else if(v.vertex.x<0.5&&v.vertex.y>0.5)
                //    v.uv=float2(0,1);
                //else
                //    v.uv=float2(1,1);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_MainColor;
                return col;
            }
            ENDHLSL
        }
    }
}
