Shader "UI/RoundedCornersWithBorder"
{
    Properties
    {
        // Default UI Shader Properties.
        // Main texture is not used but we keep the property since Image expects it.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        _BorderColor ("Border Color", Color) = (0, 0, 0, 1)
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        _RectTransformSize ("RectTransform Size", Vector) = (0, 0, 0, 0)
        _BorderParams ("Border Params", Vector) = (0, 0, 0, 0)
        _ShadowParams ("Shadow Params", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "RoundedCornersWithBorder"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile __ SHADOW_ON
            #include "UnityCG.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 positionPx  : TEXCOORD0;
                half4  mask : TEXCOORD2;
            };

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            fixed4 _Color;

            fixed4 _BorderColor;
            fixed4 _ShadowColor;
            float2 _RectTransformSize;
            float2 _BorderParams;
            float3 _ShadowParams;

            v2f vert(appdata_t v)
            {
                v2f OUT;

                // Infer vertex nudge from UVs.
                float2 nudgeDirection = (v.texcoord.xy - float2(0.5, 0.5)) * 2;
#if SHADOW_ON
                float2 nudge = abs(_ShadowParams.xy) + _ShadowParams.z; // Shadow offset+radius.
                float4 position = v.vertex + float4(nudgeDirection * nudge, 0, 0);
#else
                float2 nudge = float2(0, 0);
                float4 position = v.vertex;
#endif
                float4 vPosition = UnityObjectToClipPos(position);
                OUT.vertex = vPosition;

                // Vertex color is used by the CanvasGroup (among others).
                OUT.color = v.color;

                // Position in "UI pixels" in the (expanded) box.
                OUT.positionPx = nudgeDirection * (_RectTransformSize * 0.5 + nudge);

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                OUT.mask = half4(position.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                return OUT;
            }

            float2 DistToRoundedBox(float2 position, float2 halfSize, float radius)
            {
                float2 q = abs(position) - halfSize + radius;
                return min(max(q.x,q.y),0.0) + length(max(q, 0.0)) - radius;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float border = _BorderParams.x;
                float borderRadius = _BorderParams.y;

                float2 halfSize = _RectTransformSize * 0.5;

                // Distance to the rounded corner box.
                float distToRoundedBox = DistToRoundedBox(IN.positionPx, halfSize, borderRadius);

                const float smoothFactor = 0.2;

                float withinRoundedBox = smoothstep(-smoothFactor , 0, -distToRoundedBox);

                // Interpolation factor between the border and fill colors.
                float borderColorLerp = smoothstep(-smoothFactor , smoothFactor, -distToRoundedBox - border);

                // If the _Border is ~zero, we don't interpolate color smoothly, _BorderColor is not used.
                borderColorLerp = lerp(1, borderColorLerp, step(0.01, border));

                fixed4 color = lerp(_BorderColor, _Color, borderColorLerp);
                color.a *= withinRoundedBox;

#if SHADOW_ON
                float2 shadowOffset = _ShadowParams.xy;
                float shadowRadius = _ShadowParams.z;

                float distToRoundedBoxShadow = DistToRoundedBox(IN.positionPx + shadowOffset, halfSize, borderRadius);

                // The 0.2 is arbitrary, influences shadow scale.
                float shadowOpacity = 1 - smoothstep(0, 1, distToRoundedBoxShadow / shadowRadius);
                float4 shadowColor = _ShadowColor;
                shadowColor.a *= shadowOpacity * shadowOpacity; // Squared for falloff.

                color = color * color.a + shadowColor * min(shadowColor.a, (1 - color.a));
#endif

                color *= IN.color;

#ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
#endif

#ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
#endif

                return color;
            }
        ENDCG
        }
    }
}
