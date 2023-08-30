Shader "UI/CircularGraduation"
{
    Properties
    {
        _ScaleTex ("Scale Texture", 2D) = "white" {}
        _RadiusMinMax ("Radius x:Min|y:Max|-|-", Vector) = (0.6, 0.7, 0, 0)
        _AngularRange ("Angular Range", Float) = 90
        _Rotation ("Rotation", Float) = 0
        _Orientation ("Orientation", Float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
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
            float2 _RadiusMinMax;
            float _AngularRange;
            float _Rotation;
            float _Orientation;
            float _Antialiasing;
            sampler2D _ScaleTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const float2 center = float2(0.5, 0.5);
                float2 distToCenter = 2 * (center - i.uv);
                float distToCenterLength = length(distToCenter);
                float antialias = fwidth(distToCenterLength) * _Antialiasing * 0.5;
                float withinRadius =
                    smoothstep(_RadiusMinMax.x - antialias, _RadiusMinMax.x + antialias, distToCenterLength) *
                    smoothstep(_RadiusMinMax.y + antialias, _RadiusMinMax.y - antialias, distToCenterLength);

                float normalizedAngle = (atan2(distToCenter.y, distToCenter.x) / K_PI) * 0.5 + 0.5;
                normalizedAngle = 1 - frac(normalizedAngle + _Rotation / 360.0f); // Clockwise, rotation is expressed in degrees.
                float scale = 360.0f / (2 * _AngularRange);
                float pos = normalizedAngle * scale;

                if (pos > 1 || pos < 0 || withinRadius < K_EPSILON)
                    discard;

                if (_Orientation < 0)
                    pos = 1 - pos;

                float alpha = tex2D(_ScaleTex, float2(pos, 0.5)).a;

                return lerp(float4(0, 0, 0, 0), _Color, alpha * withinRadius);
            }
            ENDCG
        }
    }
}
