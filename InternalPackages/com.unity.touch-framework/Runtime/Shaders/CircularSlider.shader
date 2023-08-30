Shader "UI/CircularSlider"
{
    Properties
    {
        _Radius ("Radius", float) = 0.8
        _HandleRadius ("Handle Radius", float) = 0.05
        _LineWidth ("Linewidth", float) = 0.1
        _Color ("Color", Color) = (1, 1, 1, 1)
        _BackgroundColor ("Background Color", Color) = (0.5, 0.5, 0.5, 1)
        _Position("Position", Range(0, 1)) = 0.5
        _Scale ("Scale", float) = 1
        _Rotation("Rotation (rad)", float) = 0
        _Antialiasing ("Antialiasing", Range(0, 2)) = 0.5

        // Just so that uGUI does not complain.
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define K_PI 3.14159265359
            #define K_EPSILON 0.0001

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float4 _BackgroundColor;
            float _Radius;
            float _HandleRadius;
            float _LineWidth;
            float _Position;
            float _Scale;
            float _Rotation;
            float _Antialiasing;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float withinRange(float2 range, float value, float antialias)
            {
                return (
                    smoothstep(range.x - antialias * 0.5, range.x + antialias * 0.5, value) *
                    smoothstep(range.y + antialias * 0.5, range.y - antialias * 0.5, value));
            }

            float deltaAngle(float current, float target)
            {
                return atan2(sin(target - current), cos(target - current));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const float2 center = float2(0.5, 0.5);
                float2 distToCenter = 2 * (center - i.uv);
                float distToCenterLength = length(distToCenter);

                float withinRadius = withinRange(
                    float2(_Radius - _LineWidth * 0.5, _Radius + _LineWidth * 0.5),
                    distToCenterLength, fwidth(distToCenterLength) * _Antialiasing);

                float angle = atan2(-distToCenter.y, -distToCenter.x);
                angle = deltaAngle(angle, _Rotation);
                float normalizedAngle = frac(angle / (2 * K_PI) + 1);
                float pos = normalizedAngle / _Scale;

                float posAntialias = fwidth(pos) * _Antialiasing;
                float isBackground = withinRadius * withinRange(float2(0, 1), pos, posAntialias);
                float isSlider = withinRadius * withinRange(float2(0, _Position), pos, posAntialias);

                float handleAngle = _Position * -_Scale * K_PI * 2 + _Rotation;
                float2 handlePosition = float2(cos(handleAngle), sin(handleAngle)) * _Radius * 0.5 + center;
                float distToHandle = length(handlePosition - i.uv);
                float handleAntialias = fwidth(distToHandle) * _Antialiasing;
                float isHandle = smoothstep(_HandleRadius + handleAntialias * 0.5, _HandleRadius - handleAntialias * 0.5, distToHandle);

                float4 color = float4(0, 0, 0, 0);
                color = lerp(color, _BackgroundColor, isBackground);
                color = lerp(color, _Color, isSlider);
                color = lerp(color, _Color, isHandle);
                return color;
            }
            ENDCG
        }
    }
}
