Shader "UI/CircularBackground"
{
    Properties
    {
        _Radius ("Radius", Float) = 0.5
        _Color ("Background Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Float) = 0.1
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
            float4 _OutlineColor;
            float _Radius;
            float _OutlineWidth;
            float _Antialiasing;

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
                const float halfLineWidth = _OutlineWidth * 0.5;
                float2 delta = 2 * (center - i.uv);
                float lenDelta = length(delta);
                float lineAntialias = fwidth(lenDelta) * _Antialiasing;

                // Are we within the disc?
                float inCircle =  smoothstep(_Radius, _Radius - lineAntialias, lenDelta);
                fixed4 color = fixed4(_Color.rgb, _Color.a * inCircle);

                // Are we on the outline? (outline grows inside the disc so that outline does not change gloabl radius)
                float onCircle = smoothstep(halfLineWidth, halfLineWidth - lineAntialias, abs(_Radius - lenDelta - halfLineWidth));

                return lerp(color, _OutlineColor, onCircle);
            }
            ENDCG
        }
    }
}
