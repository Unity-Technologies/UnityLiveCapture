#include "D3D12EncoderDevice.h"
#include "D3D12Texture2D.h"

// Disable the 'unscoped enum' Nvenc warnings
#pragma warning(disable : 26812)

namespace NvencPlugin
{
    D3D12EncoderDevice::D3D12EncoderDevice(ID3D12Device* const nativeDevice, IUnityGraphicsD3D12v5* const unityInterface) :
        m_d3d12Device(nativeDevice),
        m_d3d12CommandQueue(unityInterface->GetCommandQueue()),
        m_d3d11Device(nullptr),
        m_d3d11Context(nullptr),
        m_copyResourceEventHandle(nullptr),
        m_copyResourceFence(nullptr),
        m_copyResourceFenceValue(1)
    {
    }

    D3D12EncoderDevice::~D3D12EncoderDevice()
    {
    }

    bool D3D12EncoderDevice::Initialize()
    {
        ID3D11Device* legacyDevice;
        ID3D11DeviceContext* legacyContext;

        D3D11CreateDevice(nullptr,
                          D3D_DRIVER_TYPE_HARDWARE,
                          nullptr,
                          0,
                          nullptr,
                          0,
                          D3D11_SDK_VERSION,
                          &legacyDevice,
                          nullptr,
                          &legacyContext);

        legacyDevice->QueryInterface(IID_PPV_ARGS(&m_d3d11Device));

        legacyDevice->GetImmediateContext(&legacyContext);
        legacyContext->QueryInterface(IID_PPV_ARGS(&m_d3d11Context));

        m_d3d12Device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocator));
        m_d3d12Device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, m_commandAllocator, nullptr, IID_PPV_ARGS(&m_commandList));

        // Command lists are created in the recording state, but there is nothing
        // to record yet. The main loop expects it to be closed, so close it now.
        m_commandList->Close();

        m_d3d12Device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_copyResourceFence));
        m_copyResourceEventHandle = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (m_copyResourceEventHandle == nullptr)
        {
            HRESULT_FROM_WIN32(GetLastError());
        }
        return true;
    }

    void D3D12EncoderDevice::InitializeConverter(const int width, const int height)
    {
        m_Converter.reset(new RGBToNV12ConverterD3D11(m_d3d11Device,
                          m_d3d11Context,
                          width,
                          height));
    }

    bool D3D12EncoderDevice::InitializeMultithreadingSecurity()
    {
        // We don't need to use the 'SetMultithreading' protection
        // because we are already using 2 different devices to do
        // the encoding (Nvenc is not compatible with D3D12).
        return true;
    }

    void D3D12EncoderDevice::Cleanup()
    {
        m_commandList->Release();
        m_commandAllocator->Release();

        if (m_d3d11Device)
        {
            m_d3d11Device->Release();
            m_d3d11Device = nullptr;
        }

        if (m_d3d11Context)
        {
            m_d3d11Context->Release();
            m_d3d11Context = nullptr;
        }

        if (m_copyResourceFence)
        {
            m_copyResourceFence->Release();
            m_copyResourceFence = nullptr;
        }

        if (m_copyResourceEventHandle)
        {
            CloseHandle(m_copyResourceEventHandle);
            m_copyResourceEventHandle = nullptr;
        }
    }

    GraphicsDeviceType D3D12EncoderDevice::GetDeviceType()
    {
        return GraphicsDeviceType::GRAPHICS_DEVICE_D3D12;
    }

    ITexture2D* D3D12EncoderDevice::CreateDefaultTexture(uint32_t width, uint32_t height, bool forceNV12)
    {
        HANDLE handle;
        auto nativeTex = CreateD3D12Resource(width, height);
        auto sharedTex = CreateSharedD3D11Resource(nativeTex, handle);

        ID3D11Texture2D* nv12Tex = nullptr;
        if (forceNV12)
        {
            nv12Tex = CreateNV12Texture(width, height);
        }

        return new D3D12Texture2D(width, height, nativeTex, handle, sharedTex, nv12Tex);
    }

    bool D3D12EncoderDevice::ConvertRGBToNV12(IUnknown* nativeSrcD3D12, void* tex2DDest)
    {
        // Convert the shared D3D11Texture to another one in the NV12 format.
        auto text2D = static_cast<D3D12Texture2D*>(tex2DDest);

        // Copy shared resources (initial RGB texture to the D3D12 shared resource).
        CopyResource(nativeSrcD3D12, tex2DDest);

        auto nativeSrcD3D11 = static_cast<ID3D11Texture2D*>(text2D->GetEncodeTexturePtrV());
        auto nativeDstD3D11 = static_cast<ID3D11Texture2D*>(text2D->GetNV12Texture());
        return m_Converter->ConvertRGBToNV12(nativeSrcD3D11, nativeDstD3D11);
    }

    bool D3D12EncoderDevice::CopyResource(IUnknown* nativeSrc, void* tex2DDest)
    {
        auto text2D = static_cast<D3D12Texture2D*>(tex2DDest);
        auto nativeSrcRes = static_cast<ID3D12Resource*>(nativeSrc);
        auto nativeDestRes = static_cast<ID3D12Resource*>(text2D->GetNativeTexturePtrV());

        if (nativeSrcRes == nativeDestRes)
            return false;
        if (nativeSrcRes == nullptr || nativeDestRes == nullptr)
            return false;

        m_commandAllocator->Reset();

        m_commandList->Reset(m_commandAllocator, nullptr);
        m_commandList->CopyResource(nativeDestRes, nativeSrcRes);
        m_commandList->Close();

        ID3D12CommandList* cmdList[] = { m_commandList };
        m_d3d12CommandQueue->ExecuteCommandLists(_countof(cmdList), cmdList);

        WaitForFence(m_copyResourceFence, m_copyResourceEventHandle, &m_copyResourceFenceValue);

        return true;
    }

    void D3D12EncoderDevice::WaitForFence(ID3D12Fence* fence, HANDLE handle, uint64_t* fenceValue)
    {
        m_d3d12CommandQueue->Signal(fence, *fenceValue);
        fence->SetEventOnCompletion(*fenceValue, handle);
        WaitForSingleObject(handle, INFINITE);
        ++(*fenceValue);
    }

    ID3D12Resource* D3D12EncoderDevice::CreateD3D12Resource(uint32_t width, uint32_t height)
    {
        D3D12_RESOURCE_DESC desc{};
        desc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;
        desc.Alignment = 0;
        desc.Width = width;
        desc.Height = height;
        desc.DepthOrArraySize = 1;
        desc.MipLevels = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM; // Only supported format (4 bytes) -> DX12_BYTES_PER_PIXEL
        desc.SampleDesc.Count = 1;
        desc.SampleDesc.Quality = 0;
        desc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
        desc.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
        desc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET | D3D12_RESOURCE_FLAG_ALLOW_SIMULTANEOUS_ACCESS;

        const auto flags = D3D12_HEAP_FLAG_SHARED;
        const auto initialState = D3D12_RESOURCE_STATE_COPY_DEST;

        ID3D12Resource* nativeTex = nullptr;

        const D3D12_HEAP_PROPERTIES D3D12_DEFAULT_HEAP_PROPS =
        {
            D3D12_HEAP_TYPE_DEFAULT,
            D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
            D3D12_MEMORY_POOL_UNKNOWN,
            0,
            0
        };

        m_d3d12Device->CreateCommittedResource(&D3D12_DEFAULT_HEAP_PROPS, flags, &desc, initialState,
            nullptr, IID_PPV_ARGS(&nativeTex));

        return nativeTex;
    }

    ID3D11Texture2D* D3D12EncoderDevice::CreateSharedD3D11Resource(ID3D12Resource* nativeTex, HANDLE& handleRef)
    {
        ID3D11Texture2D* sharedTex = nullptr;
        HANDLE handle = nullptr;
        m_d3d12Device->CreateSharedHandle(nativeTex, nullptr, GENERIC_ALL, nullptr, &handle);
        
        //ID3D11Device::OpenSharedHandle() doesn't accept handles created by d3d12. OpenSharedHandle1() is needed.
        m_d3d11Device->OpenSharedResource1(handle, IID_PPV_ARGS(&sharedTex));
        handleRef = handle;

        return sharedTex;
    }

    ID3D11Texture2D* D3D12EncoderDevice::CreateNV12Texture(uint32_t width, uint32_t height)
    {
        ID3D11Texture2D* texture = nullptr;
        D3D11_TEXTURE2D_DESC desc = { 0 };
        desc.Width = width;
        desc.Height = height;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_NV12;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_DEFAULT;
        desc.BindFlags = D3D11_BIND_RENDER_TARGET;
        desc.CPUAccessFlags = 0;
        const auto r = m_d3d11Device->CreateTexture2D(&desc, NULL, &texture);
        return texture;
    }
}
