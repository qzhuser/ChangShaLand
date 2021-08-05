Shader "QZH/ParticleLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_MainCol("MainColor",COLOR)=(1,1,1,1)
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            Tags { "LightMode"="UniversalForward"}
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
                float4 color:COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
                float3 worldPos:TEXCOORD1;
            };
            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                float4 _MainTex_ST;
                SAMPLER(sampler_MainTex);

                float4 _MainCol;
                float _SpaceX;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldPos=TransformObjectToWorld(v.vertex.xyz);
                o.uv = float2(o.worldPos.x/_SpaceX+0.5,o.worldPos.z/80);
                o.color=v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                //float2 uv=float2(i.worldPos.x/_Space.x+0.5f,i.worldPos.z/80);
                half4 texCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                half4 col=half4(_MainCol.rgb*texCol.rgb,i.color.a);

                return col;//half4(i.worldPos.x/50+0.5f,i.worldPos.y,i.worldPos.z/80,1);
            }
            ENDHLSL
        }
    }
}
