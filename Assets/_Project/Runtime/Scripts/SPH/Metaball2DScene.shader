//Le shader qui affiche les particules dans une texture comme ça on peut gérer l'orde de redu
Shader "Custom/Metaball2DScene"
{
    Properties
    {
        _LiquidColor("Liquid Color", Color) = (0.2, 0.6, 1, 1)
        _RimColor("Rim Color", Color) = (0.7, 0.9, 1, 1)
        _Threshold("Threshold", Range(0, 4)) = 0.6
        _EdgeSoftness("Edge Softness", Range(0.001, 1)) = 0.05
        _RimWidth("Rim Width", Range(0.001, 1)) = 0.12
        [IntRange] _DebugMode("Debug Mode (0/1/2)", Range(0, 2)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MetaballField);
            SAMPLER(sampler_MetaballField);

            CBUFFER_START(UnityPerMaterial)
                float4 _LiquidColor;
                float4 _RimColor;
                float _Threshold;
                float _EdgeSoftness;
                float _RimWidth;
                float _DebugMode;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(IN.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.screenPos = p.positionNDC; // = ComputeScreenPos, pour lire le champ en espace ecran
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.screenPos.xy / IN.screenPos.w;
                float field = SAMPLE_TEXTURE2D(_MetaballField, sampler_MetaballField, uv).r;

                if (_DebugMode == 2) return half4(1, 0, 1, 0.5);
                if (_DebugMode == 1) { float f = saturate(field); return half4(f, f, f, 1); }

                float alpha = smoothstep(_Threshold - _EdgeSoftness, _Threshold + _EdgeSoftness, field);
                if (alpha <= 0.0) return half4(0, 0, 0, 0);

                float rim = smoothstep(_Threshold, _Threshold + _RimWidth, field);
                float3 color = lerp(_RimColor.rgb, _LiquidColor.rgb, rim);
                return half4(color, alpha * _LiquidColor.a);
            }
            ENDHLSL
        }
    }
}
