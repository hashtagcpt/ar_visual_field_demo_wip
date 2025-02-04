Shader "Custom/GaborShader"
{
    Properties
    {
        _Frequency("Frequency", Range(0, 100)) = 10.0          // Frequency of the sinusoidal grating
        _Phase("Phase", Range(0, 6.283)) = 0.0                // Phase (0 to 2π)
        _Envelope("Envelope Size", Range(0.1, 1.0)) = 0.5     // Gaussian envelope size
        _Contrast("Contrast", Range(0.0, 1.0)) = 0.5          // Contrast of the sinusoidal grating
        _Background("Background Intensity", Range(0.0, 1.0)) = 0.5 // Background gray level
        _ColorMode("Color Mode (0=Grayscale, 1=Blue-Yellow, 2=Red-Green)", Range(0, 2)) = 0 // Color mode selection
        _Orientation("Orientation (Degrees)", Range(0, 360)) = 0.0 // Orientation of the sinusoid
        _Alpha("Alpha Transparency", Range(0.0, 1.0)) = 1.0   // Alpha transparency
        _Resolution("Resolution", Float) = 512.0             // Resolution of the rendering surface
        _GammaR("Gamma (Red)", Range(1.0, 2.2)) = 2.2        // Gamma correction for Red
        _GammaG("Gamma (Green)", Range(1.0, 2.2)) = 2.2      // Gamma correction for Green
        _GammaB("Gamma (Blue)", Range(1.0, 2.2)) = 2.2       // Gamma correction for Blue
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha  // Enable alpha blending
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Shader properties
            float _Frequency;      // Frequency of the sinusoid
            float _Phase;          // Phase offset
            float _Envelope;       // Gaussian envelope size
            float _Contrast;       // Contrast adjustment
            float _Background;     // Background gray level
            float _ColorMode;      // Color mode selector (0=Grayscale, 1=Blue-Yellow, 2=Red-Green)
            float _Orientation;    // Orientation angle in degrees
            float _Alpha;          // Alpha transparency
            float _Resolution;     // Resolution of the rendering surface
            float _GammaR;         // Gamma correction for Red
            float _GammaG;         // Gamma correction for Green
            float _GammaB;         // Gamma correction for Blue

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float gaussian(float2 uv, float size)
            {
                float dist = dot(uv, uv);  // Squared distance from the center
                return exp(-dist / (2.0 * size * size));
            }

            float bandlimitedSin(float2 uv, float frequency, float phase, float resolution)
            {
                // Adjust frequency based on pixel spacing to avoid aliasing
                float adjustedFrequency = min(frequency, resolution / 2.0);

                // Rotate UVs by the orientation angle
                float angle = radians(_Orientation);
                float2 rotatedUV = float2(
                    uv.x * cos(angle) - uv.y * sin(angle),
                    uv.x * sin(angle) + uv.y * cos(angle)
                );

                // Compute the sinusoid
                return sin(2.0 * UNITY_PI * adjustedFrequency * rotatedUV.x + phase);
            }

            float3 applyColorMode(float modulation, float mode)
            {
                if (mode < 0.5) // Grayscale
                {
                    return float3(modulation, modulation, modulation);
                }
                else if (mode < 1.5) // Blue-Yellow
                {
                    return float3(0.0, modulation, 1.0 - modulation); // Blue-Yellow axis
                }
                else // Red-Green
                {
                    return float3(modulation, 1.0 - modulation, 0.0); // Red-Green axis
                }
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Transform UVs to center the Gabor patch
                float2 uv = i.uv * 2.0 - 1.0;  // Scale UVs to range [-1, 1]

                // Gaussian envelope
                float envelope = gaussian(uv, _Envelope);

                // Sinusoidal grating with anti-aliasing
                float sinValue = bandlimitedSin(uv, _Frequency, _Phase, _Resolution);

                // Symmetric modulation centered around 0
                float modulation = _Contrast * sinValue * envelope;

                // Grayscale background
                float3 backgroundColor = float3(_Background, _Background, _Background);

                // Apply the color mode to the sinusoidal modulation
                float3 gratingColor = applyColorMode(modulation, _ColorMode);

                // Combine the grating and background
                float3 finalColor = backgroundColor + gratingColor * envelope;

                // Ensure the final color stays within [0, 1]
                finalColor = saturate(finalColor);

                // Apply gamma correction for each channel
                finalColor.r = pow(finalColor.r, 1.0 / _GammaR);
                finalColor.g = pow(finalColor.g, 1.0 / _GammaG);
                finalColor.b = pow(finalColor.b, 1.0 / _GammaB);

                return fixed4(finalColor, _Alpha);  // Use alpha for transparency
            }
            ENDCG
        }
    }
}
