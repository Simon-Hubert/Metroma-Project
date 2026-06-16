Shader "Instanced/GridTestParticleShader"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Color("Color", Color) = (0.25, 0.5, 0.5, 1)
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _DensityRange("Density Range", Range(0,500000)) = 1.0
        // URP requires this property for some setup functions
        _size("Size", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _size;
                float _DensityRange;
                half _Glossiness;
                half _Metallic;
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
                float size = _size;

                unity_ObjectToWorld._11_21_31_41 = float4(size, 0, 0, 0);
                unity_ObjectToWorld._12_22_32_42 = float4(0, size, 0, 0);
                unity_ObjectToWorld._13_23_33_43 = float4(0, 0, size, 0);
                unity_ObjectToWorld._14_24_34_44 = float4(pos.xy, 0, 1);
                
                unity_WorldToObject = unity_ObjectToWorld;
                unity_WorldToObject._14_24_34 *= -1;
                unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
            #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = mul(unity_ObjectToWorld, input.positionOS).xyz;
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                
                // Normal calculation
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Very simple lighting model for URP
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalize(input.normalWS), mainLight.direction));
                half3 diffuse = mainLight.color * NdotL;
                
                // Adding some ambient light so shadows aren't pitch black
                half3 ambient = half3(0.15, 0.15, 0.15); 
                
                half3 finalColor = _Color.rgb * (diffuse + ambient);
                
                return half4(finalColor, _Color.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}