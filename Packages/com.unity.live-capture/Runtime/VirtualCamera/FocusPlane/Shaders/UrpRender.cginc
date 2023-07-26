#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "FocusPlane.cginc"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    half4 positionCS : SV_POSITION;
    half2 uv : TEXCOORD0;
};

TEXTURE2D (_CameraDepthTexture);
SAMPLER (sampler_CameraDepthTexture);

Varyings Vert(Attributes input)
{
    Varyings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = input.uv;
    return output;
}

float4 Frag(Varyings input) : SV_Target
{
    float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv);
    float eyeSpaceDepth = LinearEyeDepth(depth, _ZBufferParams);
    float screenAspect = _ScreenParams.x / _ScreenParams.y;
    return FocusPlane(eyeSpaceDepth, input.uv, screenAspect);
}
