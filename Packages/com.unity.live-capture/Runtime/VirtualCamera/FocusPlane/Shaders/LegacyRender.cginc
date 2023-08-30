#include "UnityCG.cginc"
#include "FocusPlane.cginc"

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

sampler2D _CameraDepthTexture;

v2f Vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 Frag(v2f i) : SV_Target
{
    float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
    float eyeSpaceDepth = LinearEyeDepth(depth);
    float screenAspect = _ScreenParams.x / _ScreenParams.y;
    return FocusPlane(eyeSpaceDepth, i.uv, screenAspect);
}
