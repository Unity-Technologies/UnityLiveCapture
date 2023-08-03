Shader "Unlit/Wireframe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WireColor ("Wire Color", Color) = (1, 0, 0, 0)
        _FillColor ("Fill Color", Color) = (0, 0, 1, 0.1)
        _WireSmoothing ("Wire Smoothing", Range(0, 10)) = 1
        _WireThickness ("Wire Thickness", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _WireColor;
            fixed4 _FillColor;
            float _WireSmoothing;
            float _WireThickness;

            struct appdata
            {
                float4 position : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                half2 barycentricCoords : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(float4(v.position.xyz, 1));

                // The fourth component is used to encode vertex index within triangle.
                uint index = v.position.w * 10;
                half2 barycentricCoords = half2(
                    index % 3 == 0 ? 1 : 0,
                    index % 3 == 1 ? 1 : 0);
                o.barycentricCoords = barycentricCoords;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Barycentric coordinates sum up to one, so we only pass 2 components and infer the third.
                float3 barycentricCoords = float3(i.barycentricCoords, 1 - dot(i.barycentricCoords, float2(1, 1)));

                // Use screen-space derivatives to control wire thickness.
                float3 dBarycentricCoords = fwidth(barycentricCoords);

                float3 smoothing = dBarycentricCoords * _WireSmoothing;
                float3 thickness = dBarycentricCoords * _WireThickness;

                barycentricCoords = smoothstep(thickness, thickness + smoothing, barycentricCoords);

                float minBary = min(barycentricCoords.x, min(barycentricCoords.y, barycentricCoords.z));

                return lerp(_WireColor, _FillColor, minBary);
            }
            ENDCG
        }
    }
}
