Shader "Custom/Pixelation"
{
    Properties
    {
        _PixelSize ("Pixel Size", Range(1, 32)) = 1
        _MainTex ("MainTex", 2D) = "white" {} // For compatibility
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "Pixelation"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // URP/Blit compatibility
            TEXTURE2D_X(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _PixelSize;

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings FullscreenVert(uint id : SV_VertexID)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(id);
                o.uv = GetFullScreenTriangleTexCoord(id);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy;
                float2 pixelSizeUV = texelSize * _PixelSize;
                float2 snappedUV = (floor(i.uv / pixelSizeUV) + 0.5) * pixelSizeUV;
                snappedUV = clamp(snappedUV, 0.0, 1.0);
                float3 col = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, snappedUV).rgb;
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/BlitCopy"
}