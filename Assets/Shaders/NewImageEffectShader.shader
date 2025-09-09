Shader "Cammie/FullScreenCameraEffect"
{
    Properties
    {
        _Tint            ("Tint Intensity Color", Color) = (1,1,1,1)
        _Intensity       ("Tint Intensity", Range(0,1)) = 1
        _PosterizeSteps  ("Posterize Steps", Range(2,32)) = 8

        _VignetteRadius  ("Vignette Radius", Range(0,1)) = 0.65
        _VignetteSoftness("Vignette Softness", Range(0.001,0.5)) = 0.2

        _OutlineColor    ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness("Outline Thickness", Range(0.5,4)) = 1
        _DepthThreshold  ("Depth Threshold", Range(0.0005,0.02)) = 0.003
        _OutlineSoftness ("Outline Softness", Range(0.0001,0.02)) = 0.002
        _OutlineStrength ("Outline Strength", Range(0,3)) = 1

        _SilhouetteBackgroundThreshold ("Silhouette BG Threshold", Range(0.90,1)) = 0.995
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "FullScreenCameraEffect"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Frag

            // Feature toggles
            #pragma multi_compile __ OUTLINE_ENABLED
            #pragma multi_compile __ VIGNETTE_ENABLED
            #pragma multi_compile __ OUTLINE_SILHOUETTE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraColorTexture);   SAMPLER(sampler_CameraColorTexture);
            TEXTURE2D_X(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            float4 _CameraColorTexture_TexelSize;

            float4 _Tint;
            float  _Intensity;
            int    _PosterizeSteps;

            float  _VignetteRadius;
            float  _VignetteSoftness;

            float4 _OutlineColor;
            float  _OutlineThickness;
            float  _DepthThreshold;
            float  _OutlineSoftness;
            float  _OutlineStrength;
            float  _SilhouetteBackgroundThreshold;

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

            float3 PosterizeColor(float3 c, int steps)
            {
                steps = max(steps, 2);
                float s = steps - 1;
                return floor(c * s + 0.5) / s;
            }

            float VignetteMask(float2 uv, float radius, float softness)
            {
                float2 p = uv * 2 - 1;
                float d = length(p);
                return smoothstep(radius, radius - softness, d);
            }

            // Raw depth (non-linear) sample
            float RawDepth(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
            }

            float LinearEyeDepthAt(float2 uv)
            {
                float raw = RawDepth(uv);
                return LinearEyeDepth(raw, _ZBufferParams);
            }

            // Depth edge mode (original, with softness)
            float DepthEdge(float2 uv)
            {
                float center = LinearEyeDepthAt(uv);
                if (center <= 0) return 0;

                float2 px = _CameraColorTexture_TexelSize.xy * _OutlineThickness;
                float maxDiff = 0;
                float2 offs[4] = {
                    float2(px.x,0), float2(-px.x,0),
                    float2(0,px.y), float2(0,-px.y)
                };

                [unroll] for (int i=0;i<4;i++)
                {
                    float d = LinearEyeDepthAt(uv + offs[i]);
                    if (d > 0)
                        maxDiff = max(maxDiff, abs(center - d));
                }

                float edge = smoothstep(_DepthThreshold, _DepthThreshold + _OutlineSoftness, maxDiff);
                return edge;
            }

            // Silhouette-only mode: only outline where one of neighbors is "background" (raw depth close to 1)
            float SilhouetteEdge(float2 uv)
            {
                float centerRaw = RawDepth(uv);
                // If center is already background we skip (only outline foreground objects)
                if (centerRaw >= _SilhouetteBackgroundThreshold)
                    return 0;

                float2 px = _CameraColorTexture_TexelSize.xy * _OutlineThickness;
                float2 offs[4] = {
                    float2(px.x,0), float2(-px.x,0),
                    float2(0,px.y), float2(0,-px.y)
                };

                float silhouette = 0;
                [unroll] for (int i=0;i<4;i++)
                {
                    float nr = RawDepth(uv + offs[i]);
                    // neighbor background while center not => silhouette boundary
                    if (nr >= _SilhouetteBackgroundThreshold)
                    {
                        silhouette = 1;
                        break;
                    }
                }
                return silhouette;
            }

            float SampleOutline(float2 uv)
            {
            #ifndef OUTLINE_ENABLED
                return 0;
            #else
                #ifdef OUTLINE_SILHOUETTE
                    return SilhouetteEdge(uv) * _OutlineStrength;
                #else
                    return DepthEdge(uv) * _OutlineStrength;
                #endif
            #endif
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float3 col = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, i.uv).rgb;

                col = PosterizeColor(col, _PosterizeSteps);
                col = lerp(col, col * _Tint.rgb, _Intensity);

            #ifdef VIGNETTE_ENABLED
                float vig = VignetteMask(i.uv, _VignetteRadius, _VignetteSoftness);
                col *= vig;
            #endif

            #ifdef OUTLINE_ENABLED
                float outline = saturate(SampleOutline(i.uv));
                // Lerp to outline color (can switch to multiply if you want subtler lines)
                col = lerp(col, _OutlineColor.rgb, outline);
            #endif

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}