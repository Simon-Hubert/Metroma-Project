
Shader "Hidden/Metroma/WaterWakeFade"
{
    Properties
    {
        _MainTex ("Previous Wake", 2D) = "black" {}
        _MaskTex ("Current Mask", 2D) = "black" {}
        _FadeAmount ("Fade", Float) = 0.94
        _DiffuseAmount ("Diffuse", Float) = 0.15
        _Texel ("Texel Size", Float) = 0.0039
        _MaskStrength ("Mask Deposit", Float) = 0.0

        _BrushUV ("Brush UV", Vector) = (0.5, 0.5, 0, 0)
        _BrushDir ("Brush Dir (aspect uv)", Vector) = (1, 0, 0, 0)
        _BrushStrength ("Brush Strength", Float) = 0.0
        _BrushLength ("Brush Length", Float) = 0.12
        _BrushWidth ("Brush Width", Float) = 0.012
        _BrushSpread ("Brush Spread", Float) = 0.15
        _BrushEdge ("Brush Edge Softness", Float) = 0.3
        _Aspect ("Aspect (w/h)", Float) = 1.0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _FadeAmount, _DiffuseAmount, _Texel, _MaskStrength;
            float4 _BrushUV, _BrushDir;
            float _BrushStrength, _BrushLength, _BrushWidth, _BrushSpread, _BrushEdge, _Aspect;

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float frag (v2f i) : SV_Target
            {
                float prev = tex2D(_MainTex, i.uv).r;

                // decay + small diffusion
                float n = tex2D(_MainTex, i.uv + float2(0, _Texel)).r;
                float s = tex2D(_MainTex, i.uv - float2(0, _Texel)).r;
                float e = tex2D(_MainTex, i.uv + float2(_Texel, 0)).r;
                float w = tex2D(_MainTex, i.uv - float2(_Texel, 0)).r;
                prev = lerp(prev, (n + s + e + w) * 0.25, _DiffuseAmount) * _FadeAmount;

                // silhouette deposit (ptet de la merde)
                float mask = tex2D(_MaskTex, i.uv).a * _MaskStrength;

                // directional cone behind the emitter
                float2 d = i.uv - _BrushUV.xy;
                d.x *= _Aspect;
                float2 dir = _BrushDir.xy;
                float along = dot(d, -dir);
                float across = dot(d, float2(-dir.y, dir.x));

                //bullshit de maths de merde
                float halfW = _BrushWidth + max(along, 0.0) * _BrushSpread;
                float coneAlong = (along > 0.0) ? saturate(1.0 - along / max(_BrushLength, 1e-4)) : 0.0;

                float aa = abs(across) / max(halfW, 1e-4);
                float coneAcross = smoothstep(1.0, 1.0 - _BrushEdge, aa);
                float cone = _BrushStrength * coneAlong * coneAcross;

                return saturate(prev + mask + cone);
            }
            ENDCG
        }
    }
    Fallback Off
}
