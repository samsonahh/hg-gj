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

            v2f vert (appdata_t v)
            {
                v2f o;
                float wave = sin(v.uv.x * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveAmplitude;
                float2 wavedUV = v.uv + float2(0, wave);
                o.uv = TRANSFORM_TEX(wavedUV, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
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