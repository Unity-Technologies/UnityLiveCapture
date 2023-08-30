#include <CoreVideo/CoreVideo.h>
#include <VideoToolbox/VideoToolbox.h>
#include <Metal/Metal.h>

namespace MacOsEncodingPlugin
{
    extern "C" struct UnityTextureHandles
    {
        void* texture;
        void* textureCbCr;
    };

    struct MetalFrameTextures
    {
        CVMetalTextureRef texture;
        CVMetalTextureRef textureCbCr;

        MetalFrameTextures()
        : texture(0)
        , textureCbCr(0)
        {
        }

        void Release()
        {
            if (texture != nil)
                CFRelease(texture);
            if (textureCbCr != nil)
                CFRelease(textureCbCr);
        }
    };
}
