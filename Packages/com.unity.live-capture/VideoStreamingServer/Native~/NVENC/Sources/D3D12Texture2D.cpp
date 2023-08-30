#include "D3D12Texture2D.h"

namespace NvencPlugin
{
    D3D12Texture2D::D3D12Texture2D(uint32_t w,
                                   uint32_t h,
                                   ID3D12Resource* nativeTex,
                                   HANDLE handle,
                                   ID3D11Texture2D* sharedTex,
                                   ID3D11Texture2D* nv12Tex)
        : ITexture2D(w, h),
        m_nativeTexture(nativeTex),
        m_sharedHandle(handle),
        m_sharedTexture(sharedTex),
        m_nv12Texture(nv12Tex),
        m_readbackResource(nullptr),
        m_nativeTextureFootprint(nullptr)
    {
    }

    D3D12Texture2D::~D3D12Texture2D()
    {
        if (m_nativeTextureFootprint)
        {
            delete(m_nativeTextureFootprint);
        }

        if (m_readbackResource)
        {
            m_readbackResource->Release();
            m_readbackResource = nullptr;
        }

        if (m_sharedTexture)
        {
            m_sharedTexture->Release();
            m_sharedTexture = nullptr;
        }

        CloseHandle(m_sharedHandle);

        if (m_nativeTexture)
        {
            m_nativeTexture->Release();
            m_nativeTexture = nullptr;
        }
    }

    HRESULT D3D12Texture2D::CreateReadbackResource(ID3D12Device* const device)
    {
        if (m_nativeTextureFootprint)
        {
            delete(m_nativeTextureFootprint);
        }

        if (m_readbackResource)
        {
            m_readbackResource->Release();
            m_readbackResource = nullptr;
        }

        m_nativeTextureFootprint = new D3D12ResourceFootprint();
        D3D12_RESOURCE_DESC origDesc = m_nativeTexture->GetDesc();
        device->GetCopyableFootprints(&origDesc, 0, 1, 0,
            &m_nativeTextureFootprint->Footprint,
            &m_nativeTextureFootprint->NumRows,
            &m_nativeTextureFootprint->RowSize,
            &m_nativeTextureFootprint->ResourceSize
        );

        //Create the readback buffer for the texture.
        D3D12_RESOURCE_DESC desc{};
        desc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
        desc.Alignment = 0;
        desc.Width = m_nativeTextureFootprint->ResourceSize;
        desc.Height = 1;
        desc.DepthOrArraySize = 1;
        desc.MipLevels = 1;
        desc.Format = DXGI_FORMAT_UNKNOWN;
        desc.SampleDesc.Count = 1;
        desc.SampleDesc.Quality = 0;
        desc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
        desc.Flags = D3D12_RESOURCE_FLAG_NONE;

        const D3D12_HEAP_PROPERTIES D3D12_READBACK_HEAP_PROPS =
        {
            D3D12_HEAP_TYPE_READBACK,
            D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
            D3D12_MEMORY_POOL_UNKNOWN,
            0,
            0
        };

        const auto hr =
            device->CreateCommittedResource(&D3D12_READBACK_HEAP_PROPS,
                                            D3D12_HEAP_FLAG_NONE,
                                            &desc,
                                            D3D12_RESOURCE_STATE_COPY_DEST,
                                            nullptr,
                                            IID_PPV_ARGS(&m_readbackResource));
        return hr;
    }

    void* D3D12Texture2D::GetNativeTexturePtrV()
    {
        return m_nativeTexture;
    }

    const void* D3D12Texture2D::GetNativeTexturePtrV() const
    {
        return m_nativeTexture;
    }

    void* D3D12Texture2D::GetEncodeTexturePtrV()
    {
        return m_sharedTexture;
    }

    const void* D3D12Texture2D::GetEncodeTexturePtrV() const
    {
        return m_sharedTexture;
    }

    ID3D12Resource* D3D12Texture2D::GetReadbackResource() const
    {
        return m_readbackResource;
    }

    const D3D12ResourceFootprint* D3D12Texture2D::GetNativeTextureFootprint() const
    {
        return m_nativeTextureFootprint;
    }

    void* D3D12Texture2D::GetNV12Texture()
    {
        return m_nv12Texture;
    }

    const void* D3D12Texture2D::GetNV12Texture() const
    {
        return m_nv12Texture;
    }
}

