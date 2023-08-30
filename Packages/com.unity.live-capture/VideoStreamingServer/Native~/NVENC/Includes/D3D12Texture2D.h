#pragma once

#include <iostream>
#include "d3d11_4.h"
#include "d3d12.h"
#include "ITexture2D.h"

namespace NvencPlugin
{
    struct D3D12ResourceFootprint
    {
        D3D12_PLACED_SUBRESOURCE_FOOTPRINT Footprint;
        UINT NumRows;
        UINT64 RowSize;
        UINT64 ResourceSize;
    };

    class D3D12Texture2D : public ITexture2D
    {
    public:
        D3D12Texture2D(uint32_t w,
                       uint32_t h,
                       ID3D12Resource* nativeTex,
                       HANDLE handle,
                       ID3D11Texture2D* sharedTex,
                       ID3D11Texture2D* nv12Tex);

        virtual ~D3D12Texture2D();

        virtual void* GetNativeTexturePtrV() override;
        virtual const void* GetNativeTexturePtrV() const override;

        virtual void* GetEncodeTexturePtrV() override;
        virtual const void* GetEncodeTexturePtrV() const override;

        virtual void* GetNV12Texture() override;
        virtual const void* GetNV12Texture() const override;

        HRESULT CreateReadbackResource(ID3D12Device* device);
        ID3D12Resource* GetReadbackResource() const;
        const D3D12ResourceFootprint* GetNativeTextureFootprint() const;



    private:
        ID3D12Resource* m_nativeTexture;
        HANDLE m_sharedHandle;

        //Shared between DX11 and DX12
        ID3D11Texture2D* m_sharedTexture;

        //
        ID3D11Texture2D* m_nv12Texture;

        //For CPU Read
        ID3D12Resource* m_readbackResource;
        D3D12ResourceFootprint* m_nativeTextureFootprint;
    };
}
