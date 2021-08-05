Shader "City/NormalBuild"
{
    Properties
    {
        [HDR]_MainColor ("MainColor", Color) = (1,1,1,1)
        _Frequency("Frequency",float)=2
    }
    SubShader
    {
        Tags { "LightMode"="UniversalForward" }
        LOD 100
        ZWrite Off
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos:TEXCOORD1;
            };
            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float _Frequency;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldPos=TransformObjectToWorld(v.vertex.xyz);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 col = _MainColor;
                col*=sin(i.worldPos.x*_Frequency);
                col*=sin(i.worldPos.z*_Frequency);
                col*=sin(i.worldPos.y*_Frequency);
                // apply fog
                return col;
            }
            ENDHLSL
        }
    }
}
