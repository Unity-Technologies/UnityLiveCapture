#pragma once

#include "d3d11.h"
#include "ITexture2D.h"

namespace NvencPlugin
{
    struct D3D11Texture2D : public ITexture2D
    {
    public:
        D3D11Texture2D(uint32_t w, uint32_t h, ID3D11Texture2D* tex);
        virtual ~D3D11Texture2D();

        virtual void* GetNativeTexturePtrV() override;
        virtual const void* GetNativeTexturePtrV() const override;
        virtual void* GetEncodeTexturePtrV() override;
        virtual const void* GetEncodeTexturePtrV() const override;

        virtual void* GetNV12Texture() override;
        virtual const void* GetNV12Texture() const override;

    private:
        ID3D11Texture2D* m_texture;
    };
}
