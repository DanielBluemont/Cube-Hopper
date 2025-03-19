Shader "Custom Shaders/Gradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise ("Texture", 2D) = "white" {}
        _UVScale("UV Scale" ,float) = 1
        _HorizontalSpeed("Horizontal Speed", float) = 1
        _TopColor ("Top Color", color) = (1,1,1,1) 
        _BottomColor ("Bottom Color", color) = (1,1,1,1) 
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 uv           : TEXCOORD0;

            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            half4 _BottomColor;
            half4 _TopColor;
            float _UVScale;
            float _HorizontalSpeed;

            inline float hash(float2 p) {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            inline half2 randomVector(float2 p) {
                float a = 45.0;
                float b = 315.0;
                float randomValue = hash(p);
                float angle = lerp(a, b, randomValue); // Map random value to range [a, b]
                return half2(-cos(angle), -sin(angle));
            }

            inline half2 quintic(float2 p) {
                return p * p * p * (10.0 + p * (-15.0 + p * 6.0));
            }
             
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = lerp(_BottomColor, _TopColor, IN.uv.y);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half aspect = _ScreenParams.x / _ScreenParams.y;
                half2 scaledUv = IN.uv * half2(_UVScale * aspect, _UVScale);
                scaledUv.x += _Time * _HorizontalSpeed;

                half2 gridId = floor(scaledUv);
                half2 gridUv = frac(scaledUv);

                half2 bl = gridId;
                half2 br = gridId + half2(1, 0);
                half2 tl = gridId + half2(0, 1);
                half2 tr = gridId + half2(1, 1);

                half2 gradBl = randomVector(bl);
                half2 gradBr = randomVector(br);
                half2 gradTl = randomVector(tl);
                half2 gradTr = randomVector(tr);

                half2 distToBl = gridUv;
                half2 distToBr = gridUv - half2(1, 0);
                half2 distToTl = gridUv - half2(0, 1);
                half2 distToTr = gridUv - half2(1, 1);

                half dotBl = dot(gradBl, distToBl);
                half dotBr = dot(gradBr, distToBr);
                half dotTl = dot(gradTl, distToTl);
                half dotTr = dot(gradTr, distToTr);

                gridUv = smoothstep(0,1,(smoothstep(0, 1, gridUv)));

                half b = lerp(dotBl, dotBr, gridUv.x);
                half t = lerp(dotTl, dotTr, gridUv.x);
                half perlin = saturate(lerp(b, t, gridUv.y) +0.7);
          
                return lerp(IN.color, half4(perlin, perlin, perlin, 1), step(0.9, perlin));
            }
            ENDHLSL
        }
    }
}
