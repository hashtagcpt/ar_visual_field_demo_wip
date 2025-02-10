Shader "Custom/DoGShader"
{
    Properties
    {
        // DoG parameters (with UVs remapped from [0,1] to [-1,1])
        _Sigma("Center Sigma", Range(0.01, 1.0)) = 0.3
        _Ratio("Surround Ratio", Range(1.0, 5.0)) = 2.56
        _CenterWeight("Center Weight", Range(0, 2)) = 1.0
        _SurroundWeight("Surround Weight", Range(0, 2)) = 0.6
        _Contrast("Contrast", Range(0, 1)) = 1.0
        // 1 for on–center (bright center), 0 for off–center.
        _OnCenter("On Center (1=on, 0=off)", Range(0, 1)) = 1

        // Envelope to confine the patch to a single contiguous region.
        _Envelope("Envelope Size", Range(0.1, 2.0)) = 1.0
        // If the envelope falls below this value, the fragment is discarded.
        _Cutoff("Cutoff", Range(0, 0.1)) = 0.01

        // Base color tint for the patch.
        _Color("Color", Color) = (1,1,1,1)

        // Face-selection parameters:
        // Only fragments whose (world) normal is near this direction will display the DoG.
        //_FaceDirection("Face Direction", Vector) = (0,0,1,0)
        // The dot product threshold (0 to 1) for accepting a face.
        //_FaceThreshold("Face Threshold", Range(0,1)) = 0.9
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            // Specify an unlit shader.
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Shader properties (automatically populated from the material).
            float _Sigma;
            float _Ratio;
            float _CenterWeight;
            float _SurroundWeight;
            float _Contrast;
            float _OnCenter;
            float _Envelope;
            float _Cutoff;
            fixed4 _Color;
            float4 _FaceDirection;
            float _FaceThreshold;

            // Include vertex normals.
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv         : TEXCOORD0;
                float3 worldNormal: TEXCOORD1;
                float4 vertex     : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // --- Face Selection ---
                // Only show the DoG on fragments whose world-space normal is nearly aligned with _FaceDirection.
                float3 desiredDir = normalize(_FaceDirection.xyz);
                if (dot(normalize(i.worldNormal), desiredDir) < _FaceThreshold)
                {
                    discard;
                }

                // --- DoG Computation ---
                // Remap UV coordinates from [0,1] to [-1,1] so that (0,0) is at the patch center.
                float2 uv = i.uv * 2.0 - 1.0;
                float r2 = dot(uv, uv);

                // Compute center and surround Gaussians.
                float centerGauss = exp(-r2 / (2.0 * _Sigma * _Sigma));
                float sigmaSurround = _Sigma * _Ratio;
                float surroundGauss = exp(-r2 / (2.0 * sigmaSurround * sigmaSurround));

                // Compute the Difference-of-Gaussians (DoG) based on the _OnCenter flag.
                float dog = (_OnCenter >= 0.5)
                            ? (_CenterWeight * centerGauss - _SurroundWeight * surroundGauss)
                            : (_SurroundWeight * surroundGauss - _CenterWeight * centerGauss);
                dog *= _Contrast;

                // --- Envelope & Transparency ---
                // Use an envelope (a Gaussian) to confine the patch.
                float envelope = exp(-r2 / (2.0 * _Envelope * _Envelope));
                // Discard fragments that are far from the center (i.e. where envelope is very low).
                if(envelope < _Cutoff)
                {
                    discard;
                }

                // Use the absolute value of the DoG for intensity.
                float intensity = saturate(abs(dog));

                // Output the tinted color multiplied by the computed intensity.
                // The alpha is set to the envelope so that outside the patch (where envelope→0) the fragment is fully transparent.
                return fixed4(_Color.rgb * intensity, envelope);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
