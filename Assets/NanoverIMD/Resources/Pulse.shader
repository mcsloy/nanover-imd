Shader "Unlit/Pulse"
{
    Properties
    {
        _PulseColor("Pulse Color", Color) = (0,1,0,1)
        _PulseWidth("Pulse Width", Range(0, 1)) = 0.01
        _Duration("Duration (sec)", Float) = -0.5
        _PulseCount("Pulse Count", Int) = 3
        _SmoothingIntensity("Smoothing Intensity", Float) = 1.0
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _PulseColor;
            float _PulseWidth;
            float _Duration;
            int _PulseCount;
            float _SmoothingIntensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float normalizedTime = _Time.y / _Duration;

                float pulseIntensity = 0.0;
                for (int j = 0; j < _PulseCount; ++j)
                {
                    float pulsePos = frac(normalizedTime + (float(j) / _PulseCount));

                    // Calculate the distance, accounting for wrapping
                    float directDist = abs(i.uv.x - pulsePos);
                    float wrappedDist = 1.0 - directDist;
                    float dist = min(directDist, wrappedDist);

                    // Apply Gaussian smoothing with adjustable intensity
                    float normalizedPulseWidth = _PulseWidth * 0.1 * _SmoothingIntensity;
                    float gaussianFactor = exp(-((dist * dist) / (2.0 * normalizedPulseWidth * normalizedPulseWidth)));
                    pulseIntensity = max(pulseIntensity, gaussianFactor);
                }

                float4 baseColor = float4(0.3, 0.3, 0.3, 1); // Base color
                float4 color = lerp(baseColor, _PulseColor, pulseIntensity); // Blend base color with pulse color

                return color;
            }
            ENDCG
        }
    }
}
