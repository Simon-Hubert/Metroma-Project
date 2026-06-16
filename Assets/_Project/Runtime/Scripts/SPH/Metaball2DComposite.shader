//liquide
Shader "Hidden/Metaball2DComposite"
{
    Properties
    {
        _MainTex("Field", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha //Par dessus la scene

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _LiquidColor;
            float4 _RimColor;
            float _Threshold;    //fusion
            float _EdgeSoftness; // lissage
            float _RimWidth;     // bordure
            int _DebugMode;      // laisse c parce que j'en chiais

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float4 f = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float density = f.a; // le poids

                //bullshit de debug
                if (_DebugMode == 2)
                    return half4(1, 0, 1, 0.5);
                if (_DebugMode == 1)
                {
                    float f = saturate(density);
                    return half4(f, f, f, 1);
                }

                // lissage
                float alpha = smoothstep(_Threshold - _EdgeSoftness,
                                         _Threshold + _EdgeSoftness, density);
                if (alpha <= 0.0)
                    return half4(0, 0, 0, 0);

                // couleur moyenne du verre (RGB / poids) + contour plus clair
                float3 baseCol = f.rgb / max(density, 1e-4);
                float rim = smoothstep(_Threshold, _Threshold + _RimWidth, density);
                float3 color = lerp(_RimColor.rgb, baseCol, rim);

                return half4(color, alpha * _LiquidColor.a);
            }
            ENDHLSL
        }
    }
}
