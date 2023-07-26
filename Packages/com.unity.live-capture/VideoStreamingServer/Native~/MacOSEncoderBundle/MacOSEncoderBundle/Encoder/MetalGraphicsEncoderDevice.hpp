#pragma once

#include <stdint.h>
#include "../Unity/IUnityGraphicsMetal.h"
#include "../Unity/IUnityRenderingExtensions.h"

#include <Metal/Metal.h>

namespace MacOsEncodingPlugin
{

    class MetalGraphicsEncoderDevice
    {
    public:
        MetalGraphicsEncoderDevice(id<MTLDevice> device, IUnityGraphicsMetalV1* unityGraphicsMetal);
        virtual ~MetalGraphicsEncoderDevice();

        void* GetEncodeDevicePtr();
        bool CopyResourceFromNative(id<MTLTexture> dest, void* nativeTexturePtr);
        
    private:
        id<MTLDevice>         m_Device;
        IUnityGraphicsMetalV1* m_UnityGraphicsMetal;
        
        bool CopyTexture(id<MTLTexture> dest, id<MTLTexture> src);
    };
} // MacOsEncodingPlugin
