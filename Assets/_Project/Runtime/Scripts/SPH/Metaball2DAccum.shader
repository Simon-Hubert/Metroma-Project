//radial gauss pour chaque particule, additif. la fusion se fait au seuil ensuite.
Shader "Instanced/Metaball2DAccum"
{
    Properties
    {
        _size("Blob Size", Float) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend One One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float _size;
                float _simZ; // Z du verre, sinon decale en perspective
                float4 _blobColor; // couleur
            CBUFFER_END

            struct Particle
            {
                float pressure;
                float density;
                float2 currentForce;
                float2 velocity;
                float2 position;
            };

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<Particle> _particlesBuffer;
            #endif

            void setup()
            {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float2 pos = _particlesBuffer[unity_InstanceID].position;
                float s = _size;

                // un quad plat centre sur la particule, au Z du verre
                unity_ObjectToWorld._11_21_31_41 = float4(s, 0, 0, 0);
                unity_ObjectToWorld._12_22_32_42 = float4(0, s, 0, 0);
                unity_ObjectToWorld._13_23_33_43 = float4(0, 0, s, 0);
                unity_ObjectToWorld._14_24_34_44 = float4(pos.xy, _simZ, 1);

                unity_WorldToObject = unity_ObjectToWorld;
                unity_WorldToObject._14_24_34 *= -1;
                unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
            #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = mul(unity_ObjectToWorld, input.positionOS).xyz;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 0 au centre -> 1 au bord
                float2 d = input.uv * 2.0 - 1.0;
                float r2 = dot(d, d);

                // degrade doux : max au centre, 0 au bord si j'ai bien compris
                float falloff = saturate(1.0 - r2);
                float contribution = falloff * falloff;
                // RGB = couleur * poids , A = poids (densite)
                return half4(_blobColor.rgb * contribution, contribution);
            }
            ENDHLSL
        }
    }
}
