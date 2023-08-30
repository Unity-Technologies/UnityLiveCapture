#include "UnityCG.cginc"

struct appdata_t
{
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};

sampler2D _InputTexture;
float4 _InputTexture_ST;

v2f Vert(appdata_t v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.texcoord = TRANSFORM_TEX(v.texcoord, _InputTexture);
    return o;
}

fixed4 Frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_InputTexture, i.texcoord);
    return col;
}
