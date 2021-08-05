Shader "City/RoadHDR"
{
    Properties
    {
        [HDR]_MainColor ("MainColor", Color) = (1,1,1,1)
        [PerRendererData]_MainTex("MainTex",2D)="white"{}
        //[Toggle]_IsUI("IsUI",float)=1.0
        //_Frequency("Frequency",float)=2
    }
    SubShader
    {
        Tags { "LightMode"="UniversalForward" "Queue"="Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
                float4 color:Color;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color:Color;
                float3 worldPos:TEXCOORD1;
            };
            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;

                TEXTURE2D(_MainTex);
                float4 _MainTex_ST;
                SAMPLER(sampler_MainTex);
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldPos=TransformObjectToWorld(v.vertex.xyz);
                o.color=v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 tex=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                //clip(i.color.a-0.5);
                // sample the texture
                half3 col = _MainColor.rgb*i.color.rgb*tex.rgb;
                //if()
                // apply fog
                return half4(col,i.color.a);
            }
            ENDHLSL
        }
    }
}
