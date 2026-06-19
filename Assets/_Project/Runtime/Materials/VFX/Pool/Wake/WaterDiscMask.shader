//pas ouf et marche pas bien pour l'instant deso
Shader "Hidden/Metroma/WaterDiscMask"
{
    Properties { _MainTex ("", 2D) = "black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_DISCS 24
            float4 _Discs[MAX_DISCS];   // xy = center (area uv), z = world radius, w = stillness
            int _DiscCount;
            float2 _AreaSize;
            float _EdgeSoftness;
            float _IsoYScale;           // <1 squashes the rings vertically (iso look)
            float _IsoShear;

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
                float v = 0.0;
                [loop] for (int k = 0; k < MAX_DISCS; k++)
                {
                    if (k >= _DiscCount) break;
                    float2 c = _Discs[k].xy;
                    float r = max(_Discs[k].z, 1e-4);
                    float2 dd = (i.uv - c) * _AreaSize;      // world-space offset
                    dd.x += dd.y * _IsoShear;
                    dd.y /= max(_IsoYScale, 1e-3);
                    float dist = length(dd);
                    float cov = saturate(1.0 - dist / r) * _Discs[k].w;
                    v = max(v, cov);
                }
                return v;
            }
            ENDCG
        }
    }
    Fallback Off
}
