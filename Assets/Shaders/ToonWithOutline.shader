Shader "Cammie/ToonWithOutline"
{
    Properties
    {
        // Base Toon
        _BaseMap        ("Base Map", 2D) = "white" {}
        _BaseColor      ("Base Color", Color) = (1,1,1,1)

        _RampSteps      ("Ramp Steps", Range(2,12)) = 5
        _ShadowBlend    ("Shadow Blend (0=Ignore Shadows,1=Use)", Range(0,1)) = 0.6

        // NEW lighting helpers to prevent fully black areas
        _MinLightFloor  ("Min Light Floor", Range(0,1)) = 0.12
        _AmbientColor   ("Ambient Color", Color) = (0.6,0.6,0.7,1)
        _AmbientStrength("Ambient Strength", Range(0,2)) = 0.5
        _Wrap           ("Wrap Lighting (-1..1)", Range(-0.5,0.8)) = 0.2

        // Outline
        _OutlineColor     ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness (Obj Space)", Range(0.0005,0.1)) = 0.02
        _OutlineDepthBias ("Outline Depth Bias", Range(0,0.2)) = 0.01
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 150

        // -------- Pass 1: Toon Lit --------
        Pass
        {
            Name "ToonLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 posWS      : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _RampSteps;
                float  _ShadowBlend;
                float  _MinLightFloor;
                float4 _AmbientColor;
                float  _AmbientStrength;
                float  _Wrap;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;

            Varyings Vert(Attributes IN)
            {
                Varyings o;
                o.posWS      = TransformObjectToWorld(IN.positionOS.xyz);
                o.normalWS   = normalize(TransformObjectToWorldNormal(IN.normalOS));
                o.positionCS = TransformWorldToHClip(o.posWS);
                o.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
                return o;
            }

            float ToonStep(float v, float steps)
            {
                steps = max(2.0, steps);
                return floor(saturate(v) * steps) / (steps - 1.0);
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float3 n = normalize(IN.normalWS);

                // Albedo
                float3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;

                // Main light & optional shadow
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                    float4 shadowCoord = TransformWorldToShadowCoord(IN.posWS);
                    Light mainLight = GetMainLight(shadowCoord);
                #else
                    Light mainLight = GetMainLight();
                #endif

                // Wrapped diffuse (reduces harsh falloff and avoids pure black)
                // Wrap shifts the cosine curve: wrap > 0 brightens terminator.
                float ndl = dot(n, mainLight.direction);
                float wrapped = (ndl + _Wrap) / (1.0 + _Wrap);
                wrapped = saturate(wrapped);

                // Shadow blend (0 ignores shadow attenuation)
                float lit = lerp(wrapped, wrapped * mainLight.shadowAttenuation, _ShadowBlend);

                // Quantize
                float ramp = ToonStep(lit, _RampSteps);

                // Enforce a minimum light floor to stop full black patches
                ramp = max(ramp, _MinLightFloor);

                // Ambient fill
                float3 ambient = _AmbientColor.rgb * _AmbientStrength;

                float3 color = albedo * (ambient + ramp * mainLight.color);

                return half4(color, 1);
            }
            ENDHLSL
        }

        // -------- Pass 2: Outline --------
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            ZTest LEqual
            Offset 0,0

            HLSLPROGRAM
            #pragma vertex VertOutline
            #pragma fragment FragOutline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _OutlineThickness;
                float  _OutlineDepthBias;
            CBUFFER_END

            Varyings VertOutline(Attributes IN)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 nWS   = normalize(TransformObjectToWorldNormal(IN.normalOS));

                posWS += nWS * _OutlineThickness;

                float4 clip = TransformWorldToHClip(posWS);
                clip.z -= _OutlineDepthBias * 0.001 * clip.w;
                o.positionCS = clip;
                return o;
            }

            half4 FragOutline(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}