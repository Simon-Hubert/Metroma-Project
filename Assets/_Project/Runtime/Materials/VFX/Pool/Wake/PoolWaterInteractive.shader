Shader "Metroma/PoolWaterInteractive"
{
    Properties
    {
        [HideInInspector] _MainTex ("Sprite", 2D) = "white" {}

        [Header(Depth color)]
        _ShallowColor ("Shallow Color", Color) = (0.45, 0.85, 1.0, 1)
        _DeepColor ("Deep Color", Color) = (0.05, 0.42, 0.85, 1)
        _DepthDistance ("Depth Distance", Range(0.05, 1.5)) = 0.7
        _OpacityShallow ("Opacity Shallow", Range(0,1)) = 0.6
        _OpacityDeep ("Opacity Deep", Range(0,1)) = 0.92

        [Header(Contour ripples)]
        _FoamColor ("Foam / Ripple Color", Color) = (1,1,1,1)
        _NoiseScale ("Noise Scale", Float) = 0.35
        _RippleStretch ("Ripple Stretch", Range(0, 3)) = 0.5
        _RippleBands ("Ripple Bands", Float) = 5
        _RippleWidth ("Ripple Line Width", Range(0.01, 0.6)) = 0.12
        _RippleScroll ("Ripple Line Speed", Float) = 0.4
        _SurfaceDistortion ("Distortion", Range(0, 1)) = 0.25
        _SurfaceScroll ("Noise Scroll (xy)", Vector) = (0.03, 0.02, 0, 0)
        _SurfaceAA ("Foam Edge Softness", Range(0.005, 0.2)) = 0.06
        _IsoYScale ("Iso Y Scale", Range(0.3, 1.2)) = 0.85
        _IsoShear ("Iso Shear", Range(-0.5, 0.5)) = 0.0

        [Header(Foam band)]
        _EdgeFoamWidth ("Pool Edge Foam Width", Range(0.0, 0.4)) = 0.1
        _ObjectFoamDistance ("Object Foam Distance", Range(0.05, 1.0)) = 0.5
        _WakeFoam ("Bow Wave Edge Foam", Range(0, 12)) = 5.0
        _WakeDistort ("Bow Wave Distortion", Range(0, 4)) = 1.5
        _MaskBlur ("Mask Blur (texels)", Float) = 1.5

        [Header(Wake interior)]
        _WakeColor ("Wake Tint Color", Color) = (0.72, 0.9, 1.0, 1)
        _WakeTint ("Wake Tint Amount", Range(0, 1)) = 0.25
        _WakeChurn ("Wake Churn (alters ripples)", Range(0, 6)) = 2.0

        [Header(Object ripple rings)]
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _ObjRingCount ("Ring Count", Float) = 3
        _ObjRingSpeed ("Ring Speed", Float) = 0.8
        _ObjRingWidth ("Ring Width", Range(0.01, 0.6)) = 0.15

        [Header(Refraction)]
        _Refraction ("Refraction Amount", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

        TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
        TEXTURE2D(_WaterMask); SAMPLER(sampler_WaterMask);
        TEXTURE2D(_WakeTex);   SAMPLER(sampler_WakeTex);
        float4 _WaterMaskRect;
        float4 _WaterTexel;    // xy = 1/res, zw = res

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _ShallowColor, _DeepColor, _FoamColor, _WakeColor, _RingColor;
            float _DepthDistance, _OpacityShallow, _OpacityDeep;
            float _NoiseScale, _RippleStretch, _RippleBands, _RippleWidth, _RippleScroll;
            float _SurfaceDistortion, _SurfaceAA, _IsoYScale, _IsoShear;
            float4 _SurfaceScroll;
            float _EdgeFoamWidth, _ObjectFoamDistance, _WakeFoam, _WakeDistort, _MaskBlur;
            float _WakeTint, _WakeChurn;
            float _ObjRingCount, _ObjRingSpeed, _ObjRingWidth;
            float _Refraction;
        CBUFFER_END

        struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float2 wpos : TEXCOORD1;
        };

        Varyings vert (Attributes IN)
        {
            Varyings o;
            float3 wp = TransformObjectToWorld(IN.positionOS.xyz);
            o.positionHCS = TransformWorldToHClip(wp);
            o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
            o.wpos = wp.xy;
            return o;
        }

        float hash21 (float2 p)
        {
            p = frac(p * float2(123.34, 345.45));
            p += dot(p, p + 34.345);
            return frac(p.x * p.y);
        }

        float valueNoise (float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            float2 u = f * f * (3.0 - 2.0 * f);
            float a = hash21(i);
            float b = hash21(i + float2(1, 0));
            float c = hash21(i + float2(0, 1));
            float d = hash21(i + float2(1, 1));
            return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
        }

        float fbm (float2 p)
        {
            float v = 0.0;
            float a = 0.5;
            [unroll] for (int i = 0; i < 3; i++)
            {
                v += a * valueNoise(p);
                p *= 2.0;
                a *= 0.5;
            }
            return v / 0.875;
        }

        float SampleR (TEXTURE2D_PARAM(tex, smp), float2 uv)
        {
            if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
            return SAMPLE_TEXTURE2D(tex, smp, uv).r;
        }

        float SampleMaskBlur (float2 uv)
        {
            float ex = _WaterTexel.x * _MaskBlur;
            float ey = _WaterTexel.y * _MaskBlur;
            float a  = SampleR(TEXTURE2D_ARGS(_WaterMask, sampler_WaterMask), uv);
            a += SampleR(TEXTURE2D_ARGS(_WaterMask, sampler_WaterMask), uv + float2(ex, 0));
            a += SampleR(TEXTURE2D_ARGS(_WaterMask, sampler_WaterMask), uv - float2(ex, 0));
            a += SampleR(TEXTURE2D_ARGS(_WaterMask, sampler_WaterMask), uv + float2(0, ey));
            a += SampleR(TEXTURE2D_ARGS(_WaterMask, sampler_WaterMask), uv - float2(0, ey));
            return a / 5.0;
        }

        half4 frag (Varyings IN) : SV_Target
        {
            float2 sUV = IN.positionHCS.xy / _ScreenParams.xy;          // screen (refraction)
            float2 muv = (IN.wpos - _WaterMaskRect.xy) / _WaterMaskRect.zw;
            float t = _Time.y;
            
            float distC = length((IN.uv - 0.5) * 2.0);
            float depth01 = saturate((1.0 - distC) / max(_DepthDistance, 1e-3));
            float4 waterRGBA = lerp(_ShallowColor, _DeepColor, depth01);
            float opacity = lerp(_OpacityShallow, _OpacityDeep, depth01);

            float3 waterCol = waterRGBA.rgb;
            float waterAlpha = saturate(waterRGBA.a * opacity);
            
            float we = _WaterTexel.x * 2.0;
            float wL = SampleR(TEXTURE2D_ARGS(_WakeTex, sampler_WakeTex), muv - float2(we, 0));
            float wR = SampleR(TEXTURE2D_ARGS(_WakeTex, sampler_WakeTex), muv + float2(we, 0));
            float wDn = SampleR(TEXTURE2D_ARGS(_WakeTex, sampler_WakeTex), muv - float2(0, we));
            float wUp = SampleR(TEXTURE2D_ARGS(_WakeTex, sampler_WakeTex), muv + float2(0, we));
            float2 wakeGrad = float2(wR - wL, wUp - wDn);
            float wakeEdge = saturate(length(wakeGrad) * _WakeFoam);   // V borders only (flat-top cone)
            float wc = SampleR(TEXTURE2D_ARGS(_WakeTex, sampler_WakeTex), muv); // cone interior value

            float2 plane = float2(IN.wpos.x + IN.wpos.y * _IsoShear, IN.wpos.y / max(_IsoYScale, 1e-3));
            float2 nuv = float2(plane.x, plane.y * (1.0 + _RippleStretch)) * _NoiseScale + _SurfaceScroll.xy * t;
            float2 duv = nuv * 1.9 - _SurfaceScroll.xy * (t * 0.5);
            float2 distortion = (float2(fbm(duv), fbm(duv + 7.3)) - 0.5) * _SurfaceDistortion;
            distortion += wakeGrad * _WakeDistort;
            float nval = saturate(fbm(nuv + distortion));

            float bands = nval * _RippleBands + t * _RippleScroll + wc * _WakeChurn;
            float saw = abs(frac(bands) - 0.5) * 2.0;
            float ripple = smoothstep(_RippleWidth, 0.0, saw);

            float m = SampleMaskBlur(muv);
            float edgeProx = 0.0;
            if (_EdgeFoamWidth > 1e-5)
            {
                edgeProx = saturate((distC - (1.0 - _EdgeFoamWidth)) / _EdgeFoamWidth);
            }

            float objProx  = smoothstep(1.0 - _ObjectFoamDistance, 1.0, m);
            float foamFactor = saturate(max(max(edgeProx, objProx), wakeEdge));

            // Ragged foam band where nearness beats the noise field.
            float foam = smoothstep(nval - _SurfaceAA, nval + _SurfaceAA, foamFactor);
            ripple *= (1.0 - foam);
            float white = max(foam, ripple);

          //ring
            float ringDist = 1.0 - saturate(m);                          // 0 at object -> 1 far
            float ringPhase = ringDist * _ObjRingCount - t * _ObjRingSpeed;
            float ringTri = abs(frac(ringPhase) - 0.5) * 2.0;
            float ringW = _ObjRingWidth * saturate(m);                   // thinner far, full opacity
            float objRing = (ringW > 1e-4) ? smoothstep(1.0 - ringW, 1.0, ringTri) : 0.0;

           
            float2 wob = distortion * _Refraction;
            float3 sceneCol = SampleSceneColor(sUV + wob);

            float wakeAmount = saturate(wc * _WakeTint * _WakeColor.a);
            float foamAlpha = saturate(white * _FoamColor.a);
            float ringAlpha = saturate(objRing * _RingColor.a);

            float3 col = waterCol;
            col = lerp(col, _WakeColor.rgb, wakeAmount);  // wake visible seulement si _WakeColor.a > 0
            col = lerp(col, _FoamColor.rgb, white);       // foam
            col = lerp(col, _RingColor.rgb, objRing);     // rings

            float alpha = saturate(waterAlpha + wakeAmount + foamAlpha + ringAlpha);
            return half4(col, alpha);
        }
        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
    Fallback Off
}