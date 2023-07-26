#pragma once

#include <iostream>
#include "d3d11.h"
#include "IGraphicsEncoderDevice.h"
#include "RGBToNV12ConverterD3D11.h"
#include "D3D11Texture2D.h"

namespace NvencPlugin
{
    class D3D11EncoderDevice : public IGraphicsEncoderDevice
    {
    public:
        D3D11EncoderDevice(ID3D11Device* device);
        virtual ~D3D11EncoderDevice();

        virtual bool Initialize() override;
        virtual void InitializeConverter(const int width, const int height) override;
        virtual bool InitializeMultithreadingSecurity() override;
        virtual void Cleanup() override;

        virtual GraphicsDeviceType GetDeviceType() override;
        virtual ITexture2D* CreateDefaultTexture(uint32_t width, uint32_t height, bool forceNV12) override;

        virtual bool ConvertRGBToNV12(IUnknown* nativeSrc, void* nativeDest) override;
        virtual bool CopyResource(IUnknown* nativeDest, void* nativeSrc) override;

        inline IUnknown* GetDevice() { return m_D3d11Device; }

#ifdef DEBUG_MODE
        inline ID3D11DeviceContext* GetD3d11Context() { return m_D3d11Context; }
#endif

    private:
        ID3D11Device* m_D3d11Device;
        ID3D11DeviceContext* m_D3d11Context;
        std::unique_ptr<RGBToNV12ConverterD3D11> m_Converter;
    };
}
