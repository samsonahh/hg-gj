// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

Shader "Custom/ComicBubbleOutline"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 0.03
        _WaveAmplitude ("Wave Amplitude", Float) = 0.03
        _WaveFrequency ("Wave Frequency", Float) = 6.0
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _Alpha ("Alpha", Range(0,1)) = 1.0

        _Billboard ("Always Face Camera (1=On,0=Off)", Float) = 1
        _BillboardYOnly ("Billboard Y Only (Cylindrical)", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;
            float _Alpha;

            // Billboard controls
            float _Billboard;
            float _BillboardYOnly;

            // Use built-in _WorldSpaceCameraPos (don’t redeclare)

            // Helper: extract world scale from unity_ObjectToWorld
            float3 GetWorldScale()
            {
                float3 col0 = float3(unity_ObjectToWorld[0][0], unity_ObjectToWorld[1][0], unity_ObjectToWorld[2][0]);
                float3 col1 = float3(unity_ObjectToWorld[0][1], unity_ObjectToWorld[1][1], unity_ObjectToWorld[2][1]);
                float3 col2 = float3(unity_ObjectToWorld[0][2], unity_ObjectToWorld[1][2], unity_ObjectToWorld[2][2]);
                return float3(length(col0), length(col1), length(col2));
            }

            v2f vert (appdata_t v)
            {
                v2f o;

                // UV wave (unchanged)
                float wave = sin(v.uv.x * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveAmplitude;
                float2 wavedUV = v.uv + float2(0, wave);
                o.uv = TRANSFORM_TEX(wavedUV, _MainTex);

                float3 worldPos;

                if (_Billboard > 0.5)
                {
                    // Object center in world space
                    float3 center = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;

                    // Camera-facing forward
                    float3 toCam = _WorldSpaceCameraPos - center;
                    float3 forward;
                    if (_BillboardYOnly > 0.5)
                    {
                        // Cylindrical billboard: only rotate around Y
                        forward = normalize(float3(toCam.x, 0, toCam.z));
                        // Handle degenerate case straight up/down
                        if (all(abs(forward) < 1e-5)) forward = float3(0,0,1);
                    }
                    else
                    {
                        // Spherical billboard
                        forward = normalize(toCam);
                    }

                    float3 worldUp = float3(0,1,0);
                    float3 right = normalize(cross(worldUp, forward));
                    if (any(isnan(right)) || length(right) < 1e-5) right = float3(1,0,0);

                    float3 upVec = normalize(cross(forward, right));

                    // Preserve object Transform scale
                    float3 wsScale = GetWorldScale();
                    float3 local = v.vertex.xyz * wsScale;

                    // Rebuild world position using billboard basis (size preserved)
                    worldPos = center + right * local.x + upVec * local.y + forward * local.z;
                }
                else
                {
                    // Regular object transform (keeps scale/rotation)
                    worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                }

                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float alpha = col.a;

                // Outline sampling
                float outline = 0.0;
                float2 offset = float2(_OutlineThickness, 0);
                outline += tex2D(_MainTex, i.uv + offset).a;
                outline += tex2D(_MainTex, i.uv - offset).a;
                outline += tex2D(_MainTex, i.uv + float2(0, _OutlineThickness)).a;
                outline += tex2D(_MainTex, i.uv - float2(0, _OutlineThickness)).a;

                if (alpha < 0.1 && outline > 0.1)
                    return float4(_OutlineColor.rgb, _OutlineColor.a * _Alpha);
                else
                    return float4(col.rgb, col.a * _Alpha);
            }
            ENDCG
        }
    }
}