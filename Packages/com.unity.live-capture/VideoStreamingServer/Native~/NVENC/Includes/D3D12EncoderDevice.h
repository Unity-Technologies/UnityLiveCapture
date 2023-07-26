#pragma once

#include <comdef.h>
#include <iostream>
#include "d3d11_4.h"
#include "d3d12.h"
#include "IGraphicsEncoderDevice.h"
#include "RGBToNV12ConverterD3D11.h"
#include "Unity/IUnityGraphicsD3D12.h"

namespace NvencPlugin
{
#define DefPtr(_a) _COM_SMARTPTR_TYPEDEF(_a, __uuidof(_a))
    DefPtr(ID3D12CommandAllocator);
    DefPtr(ID3D12GraphicsCommandList4);

    // Solution from Unity Japan team, WebRTC.
    // https://github.com/Unity-Technologies/com.unity.webrtc
    class D3D12EncoderDevice : public IGraphicsEncoderDevice
    {
    public:
        D3D12EncoderDevice(ID3D12Device* nativeDevice, IUnityGraphicsD3D12v5* unityInterface);
        virtual ~D3D12EncoderDevice();

        virtual bool Initialize() override;
        virtual void InitializeConverter(const int width, const int height) override;
        virtual bool InitializeMultithreadingSecurity() override;
        virtual void Cleanup() override;

        virtual GraphicsDeviceType GetDeviceType() override;
        virtual ITexture2D* CreateDefaultTexture(uint32_t w, uint32_t h, bool forceNV12) override;

        virtual bool ConvertRGBToNV12(IUnknown* nativeSrc, void* nativeDest) override;
        virtual bool CopyResource(IUnknown* nativeDest, void* nativeSrc) override;

        // Since NVENC does not support D3D12, we use new a D3D12 resource to create a ID3D11Texture2D
        // that can be shared with the D3D12 device and passed to the NvEncoder instance.
        inline IUnknown* GetDevice() { return m_d3d11Device; }

    private:
        void WaitForFence(ID3D12Fence* fence, HANDLE handle, uint64_t* fenceValue);

        ID3D12Device* m_d3d12Device;
        ID3D12CommandQueue* m_d3d12CommandQueue;

        ID3D11Device5* m_d3d11Device;
        ID3D11DeviceContext4* m_d3d11Context;
        std::unique_ptr<RGBToNV12ConverterD3D11> m_Converter;

        ID3D12CommandAllocatorPtr m_commandAllocator;
        ID3D12GraphicsCommandList4Ptr m_commandList;

        ID3D12Fence* m_copyResourceFence;
        HANDLE       m_copyResourceEventHandle;
        uint64_t     m_copyResourceFenceValue;

        // Create a D3D11 NV12 texture.
        ID3D11Texture2D* CreateNV12Texture(uint32_t width, uint32_t height);

        // Create a D3D12 committed resource.
        ID3D12Resource* CreateD3D12Resource(uint32_t width, uint32_t height);

        // Create a D3D11 texture, shared with the previous D3D12 commited resource.
        ID3D11Texture2D* CreateSharedD3D11Resource(ID3D12Resource* nativeTex, HANDLE& handle);
    };
}
