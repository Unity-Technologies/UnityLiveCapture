#include "D3D11Texture2D.h"

namespace NvencPlugin
{
    D3D11Texture2D::D3D11Texture2D(uint32_t w, uint32_t h, ID3D11Texture2D* tex)
        : ITexture2D(w, h),
        m_texture(tex)
    {
    }

    D3D11Texture2D::~D3D11Texture2D()
    {
        if (m_texture)
        {
            m_texture->Release();
            m_texture = nullptr;
        }
    }

    void* D3D11Texture2D::GetNativeTexturePtrV()
    {
        return m_texture;
    }

    const void* D3D11Texture2D::GetNativeTexturePtrV() const
    {
        return m_texture;
    }

    void* D3D11Texture2D::GetEncodeTexturePtrV()
    {
        return m_texture;
    }

    const void* D3D11Texture2D::GetEncodeTexturePtrV() const
    {
        return m_texture;
    }

    void* D3D11Texture2D::GetNV12Texture()
    {
        return m_texture;
    }

    const void* D3D11Texture2D::GetNV12Texture() const
    {
        return m_texture;
    }
}
