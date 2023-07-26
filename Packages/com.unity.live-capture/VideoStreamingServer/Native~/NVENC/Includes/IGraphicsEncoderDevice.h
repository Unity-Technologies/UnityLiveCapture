#pragma once

namespace NvencPlugin
{
    enum class GraphicsDeviceType
    {
        GRAPHICS_DEVICE_D3D11 = 0,
        GRAPHICS_DEVICE_D3D12,
        GRAPHICS_DEVICE_OPENGL,
        GRAPHICS_DEVICE_METAL,
        GRAPHICS_DEVICE_VULKAN,
    };

    class ITexture2D;
    class IGraphicsEncoderDevice
    {
    public:
        IGraphicsEncoderDevice() {}
        virtual ~IGraphicsEncoderDevice() {}

        virtual bool Initialize() = 0;
        virtual void InitializeConverter(const int width, const int height) = 0;
        virtual bool InitializeMultithreadingSecurity() = 0;
        virtual void Cleanup() = 0;

        virtual bool ConvertRGBToNV12(IUnknown* nativeSrc, void* nativeDest) = 0;
        virtual bool CopyResource(IUnknown* nativeSrc, void* nativeDest) = 0;

        virtual ITexture2D* CreateDefaultTexture(uint32_t width, uint32_t height, bool forceNV12) = 0;

        virtual GraphicsDeviceType GetDeviceType() = 0;
        virtual IUnknown* GetDevice() = 0;
    };
}
