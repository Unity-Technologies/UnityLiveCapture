#include "D3D11EncoderDevice.h"

// Disable the 'unscoped enum' Nvenc warnings
#pragma warning(disable : 26812)

namespace NvencPlugin
{
    D3D11EncoderDevice::D3D11EncoderDevice(ID3D11Device* const device) :
        m_D3d11Device(device),
        m_D3d11Context(nullptr)
    {
    }

    D3D11EncoderDevice::~D3D11EncoderDevice()
    {
    }

    bool D3D11EncoderDevice::Initialize()
    {
        if (m_D3d11Device == nullptr)
            return false;

        m_D3d11Device->GetImmediateContext(&m_D3d11Context);

        return m_D3d11Device != nullptr && m_D3d11Context != nullptr;
    }

    void D3D11EncoderDevice::InitializeConverter(const int width, const int height)
    {
        m_Converter.reset(new RGBToNV12ConverterD3D11(m_D3d11Device,
            m_D3d11Context,
            width,
            height));
    }

    bool D3D11EncoderDevice::InitializeMultithreadingSecurity()
    {
        ID3D10Multithread* spMultithread;
        if (SUCCEEDED(m_D3d11Device->QueryInterface(__uuidof(ID3D10Multithread), (void**)&spMultithread)))
        {
            spMultithread->SetMultithreadProtected(true);
            return true;
        }
        return false;
    }

    void D3D11EncoderDevice::Cleanup()
    {
        if (m_D3d11Context != nullptr)
        {
            m_D3d11Context->Release();
            m_D3d11Context = nullptr;
        }
    }

    GraphicsDeviceType D3D11EncoderDevice::GetDeviceType()
    {
        return GraphicsDeviceType::GRAPHICS_DEVICE_D3D11;
    }

    ITexture2D* D3D11EncoderDevice::CreateDefaultTexture(uint32_t width, uint32_t height, bool forceNV12)
    {
        ID3D11Texture2D* texture = nullptr;
        D3D11_TEXTURE2D_DESC desc = { 0 };
        desc.Width = width;
        desc.Height = height;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = (forceNV12) ? DXGI_FORMAT_NV12 : DXGI_FORMAT_B8G8R8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_DEFAULT;
        desc.BindFlags = D3D11_BIND_RENDER_TARGET;
        desc.CPUAccessFlags = 0;
        HRESULT r = m_D3d11Device->CreateTexture2D(&desc, NULL, &texture);
        return new D3D11Texture2D(width, height, texture);
    }

    bool D3D11EncoderDevice::ConvertRGBToNV12(IUnknown* nativeSrc, void* tex2DDest)
    {
        auto text2D = static_cast<D3D11Texture2D*>(tex2DDest);
        auto nativeDest = text2D->GetNativeTexturePtrV();

        return m_Converter->ConvertRGBToNV12(static_cast<ID3D11Texture2D*>(nativeSrc),
                                             static_cast<ID3D11Texture2D*>(nativeDest));
    }

    bool D3D11EncoderDevice::CopyResource(IUnknown* nativeSrc, void* tex2DDest)
    {
        auto text2D = static_cast<D3D11Texture2D*>(tex2DDest);
        auto nativeDest = text2D->GetNativeTexturePtrV();

        m_D3d11Context->CopyResource(static_cast<ID3D11Resource*>(nativeDest),
                                     static_cast<ID3D11Resource*>(nativeSrc));
        return true;
    }
}
