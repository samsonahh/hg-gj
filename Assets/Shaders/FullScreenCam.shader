Shader "Cammie/FullScreenCameraEffect"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _Intensity ("Tint Intensity", Range(0,1)) = 1
        _PosterizeSteps ("Posterize Steps", Range(2,32)) = 8

        // Vignette (advanced)
        _VignetteInnerRadius ("Vignette Inner Radius", Range(0,1)) = 0.35
        _VignetteOuterRadius ("Vignette Outer Radius", Range(0,1)) = 0.85
        _VignetteSoftness ("Vignette Softness", Range(0,1)) = 0.25
        _VignetteIntensity ("Vignette Intensity", Range(0,2)) = 1
        _VignetteColor ("Vignette Color", Color) = (0,0,0,1)
        _VignetteCenter ("Vignette Center", Vector) = (0.5,0.5,0,0)
        _VignetteAxisScale ("Vignette Axis Scale", Vector) = (1,1,0,0)
        _VignetteRoundness ("Vignette Roundness", Range(0,1)) = 1

        // Outline
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 1
        _DepthThreshold ("Depth Threshold", Float) = 0.003
        _OutlineSoftness ("Outline Softness", Float) = 0.002
        _OutlineStrength ("Outline Strength", Float) = 1
        _SilhouetteBackgroundThreshold ("Silhouette BG Threshold", Range(0.9,1)) = 0.995

        // Film Grain
        _GrainIntensity ("Grain Intensity", Range(0,1)) = 0.35
        _GrainScale ("Grain Scale", Float) = 320
        _GrainSpeed ("Grain Speed", Float) = 1.5
        _GrainLumaResponse ("Grain Luma Response", Float) = 1
        _GrainSeedJitter ("Grain Seed Jitter", Vector) = (17.3,91.7,0,0)

        // Pixelation
        _PixelateEnabled ("Enable Pixelation", Float) = 0
        _PixelSize ("Pixel Size", Range(1, 32)) = 1
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

            #pragma multi_compile __ VIGNETTE_ENABLED
            #pragma multi_compile __ OUTLINE_ENABLED
            #pragma multi_compile __ OUTLINE_SILHOUETTE
            #pragma multi_compile __ FILMGRAIN_ENABLED
            #pragma multi_compile __ PIXELATE_ENABLED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
            TEXTURE2D_X(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);

            float4 _CameraColorTexture_TexelSize;

            float4 _Tint;
            float  _Intensity;
            int    _PosterizeSteps;

            // Vignette
            float  _VignetteInnerRadius;
            float  _VignetteOuterRadius;
            float  _VignetteSoftness;
            float  _VignetteIntensity;
            float4 _VignetteColor;
            float4 _VignetteCenter;
            float4 _VignetteAxisScale;
            float  _VignetteRoundness;

            // Outline
            float4 _OutlineColor;
            float  _OutlineThickness;
            float  _DepthThreshold;
            float  _OutlineSoftness;
            float  _OutlineStrength;
            float  _SilhouetteBackgroundThreshold;

            // Film grain
            float  _GrainIntensity;
            float  _GrainScale;
            float  _GrainSpeed;
            float  _GrainLumaResponse;
            float4 _GrainSeedJitter;

            // Pixelation
            float  _PixelateEnabled;
            float  _PixelSize;

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

            float3 Posterize(float3 c, int steps)
            {
                steps = max(steps, 2);
                float s = steps - 1;
                return floor(c * s + 0.5) / s;
            }

            float LinearEyeDepthSample(float2 uv)
            {
                float raw = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                return LinearEyeDepth(raw, _ZBufferParams);
            }

            // Depth-only silhouette edge (existing simple impl)
            float SampleOutlineDepth(float2 uv)
            {
            #ifndef OUTLINE_ENABLED
                return 0;
            #else
                float center = LinearEyeDepthSample(uv);
                if (center <= 0) return 0;
                float2 px = _CameraColorTexture_TexelSize.xy * _OutlineThickness;
                float maxDiff = 0;
                float2 offs[4] = { float2(px.x,0), float2(-px.x,0), float2(0,px.y), float2(0,-px.y) };
                [unroll] for (int i=0;i<4;i++)
                {
                    float d = LinearEyeDepthSample(uv + offs[i]);
                    if (d > 0)
                        maxDiff = max(maxDiff, abs(center - d));
                }
                float edge = smoothstep(_DepthThreshold, _DepthThreshold + _OutlineSoftness, maxDiff);
                return edge * _OutlineStrength;
            #endif
            }

            float SampleSilhouetteOnly(float2 uv)
            {
            #ifndef OUTLINE_ENABLED
                return 0;
            #else
                float centerRaw = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                if (centerRaw >= _SilhouetteBackgroundThreshold) return 0;
                float2 px = _CameraColorTexture_TexelSize.xy * _OutlineThickness;
                float2 offs[4] = { float2(px.x,0), float2(-px.x,0), float2(0,px.y), float2(0,-px.y) };
                [unroll] for (int i=0;i<4;i++)
                {
                    float nr = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + offs[i]).r;
                    if (nr >= _SilhouetteBackgroundThreshold)
                        return _OutlineStrength;
                }
                return 0;
            #endif
            }

            float2 Hash22(float2 p)
            {
                // Simple hash for grain UV jitter
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(float2(p.x * p.y + p.x, p.x * p.y + p.y));
            }

            float FilmGrain(float2 uv, float time)
            {
            #ifndef FILMGRAIN_ENABLED
                return 0;
            #else
                // Scale UV
                float2 gUV = uv * (_GrainScale / 100.0);
                // Animate with time + seed jitter
                gUV += _GrainSeedJitter.xy * 0.01 + time * _GrainSpeed;
                float2 h = Hash22(floor(gUV)) + Hash22(frac(gUV));
                float n = frac(sin(dot(h, float2(41.32, 289.11))) * 93758.5453);
                return n * 2.0 - 1.0; // -1..1
            #endif
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float3 col;

            #ifdef PIXELATE_ENABLED
                if (_PixelateEnabled > 0.5 && _PixelSize > 1.0)
                {
                    float2 pixelCount = _ScreenParams.xy / _PixelSize;
                    float2 pixelUV = (floor(i.uv * pixelCount) + 0.5) / pixelCount;
                    col = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, pixelUV).rgb;
                }
                else
                {
                    col = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, i.uv).rgb;
                }
            #else
                col = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, i.uv).rgb;
            #endif

                // Posterize
                col = Posterize(col, _PosterizeSteps);
                col = lerp(col, col * _Tint.rgb, _Intensity);

            #ifdef OUTLINE_ENABLED
                float outlineFactor =
                #ifdef OUTLINE_SILHOUETTE
                    SampleSilhouetteOnly(i.uv);
                #else
                    SampleOutlineDepth(i.uv);
                #endif
                col = lerp(col, _OutlineColor.rgb, saturate(outlineFactor));
            #endif

            #ifdef VIGNETTE_ENABLED
                {
                    float2 p = (i.uv - _VignetteCenter.xy);
                    p *= _VignetteAxisScale.xy;
                    // Elliptical distance
                    float dist = length(p);
                    // Roundness shaping (optional power curve)
                    float shaped = pow(dist, lerp(1.0, 0.75, _VignetteRoundness));
                    float inner = _VignetteInnerRadius;
                    float outer = _VignetteOuterRadius;
                    // Mask: 0 inside inner, 1 outside outer
                    float mask = smoothstep(inner, outer, shaped);
                    // Softness adjustment
                    mask = pow(mask, max(0.0001, _VignetteSoftness));
                    col = lerp(col, col * _VignetteColor.rgb, mask * _VignetteIntensity);
                }
            #endif

            #ifdef FILMGRAIN_ENABLED
                {
                    float time = _Time.y;
                    float g = FilmGrain(i.uv, time);
                    float luma = dot(col, float3(0.299,0.587,0.114));
                    float fade = lerp(1.0, saturate(1.0 - luma), _GrainLumaResponse);
                    col += g * (_GrainIntensity * 0.5) * fade; // subtle
                }
            #endif

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}