Shader "City/NormalBuild2"
{
    Properties
    {
        [HDR]_MainColor1 ("MainColor1", Color) = (1,1,1,1)
        [HDR]_MainColor2 ("MainColor2", Color) = (1,1,1,1)
        _YScope("YSocpe",Vector)=(0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        Pass
        {
            Tags { "LightMode"="UniversalForward"}
            //ZWrite Off
            Cull Back
            //Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal:NORMAL;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos:TEXCOORD1;
                float3 normal:NORMAL;
            };
            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor1;
                float4 _MainColor2;
                float4 _YScope;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldPos=TransformObjectToWorld(v.vertex.xyz);
                o.normal=TransformObjectToWorldNormal(v.normal);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                
                half4 col=lerp(_MainColor1,_MainColor2,smoothstep(_YScope.x,_YScope.y,i.worldPos.y));
                half3 normal=normalize(i.normal);
                Light light=GetMainLight();

                half3 diffuse= col.rgb*light.color*(saturate(dot(normal,light.direction))*0.5+0.5);

                //Second Effect
                //if(i.worldPos.y<_YScope.x&&i.worldPos.y>_YScope.y)
                //    return _MainColor1;
                //else
                //    return _MainColor2;


                // apply fog
                return half4(diffuse,1.0);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
