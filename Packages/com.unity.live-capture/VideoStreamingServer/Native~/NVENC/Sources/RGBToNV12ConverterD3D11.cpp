#include "RGBToNV12ConverterD3D11.h"

namespace NvencPlugin
{
    RGBToNV12ConverterD3D11::RGBToNV12ConverterD3D11(ID3D11Device* const pDevice,
                                                     ID3D11DeviceContext* const pContext,
                                                     const int nWidth,
                                                     const int nHeight) :
        m_D3D11Device(pDevice),
        m_D3D11Context(pContext),
        m_VideoDevice(nullptr),
        m_VideoContext(nullptr),
        m_VideoProcessor(nullptr),
        m_TexBgra(nullptr),
        m_InputView(nullptr),
        m_OutputView(nullptr),
        m_VideoProcessorEnumerator(nullptr)
    {
        m_D3D11Device->AddRef();
        m_D3D11Context->AddRef();

        m_TexBgra = NULL;
        D3D11_TEXTURE2D_DESC desc;
        ZeroMemory(&desc, sizeof(D3D11_TEXTURE2D_DESC));
        desc.Width = nWidth;
        desc.Height = nHeight;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_DEFAULT;
        desc.BindFlags = D3D11_BIND_RENDER_TARGET;
        desc.CPUAccessFlags = 0;
        pDevice->CreateTexture2D(&desc, NULL, &m_TexBgra);

        pDevice->QueryInterface(__uuidof(ID3D11VideoDevice), (void**)&m_VideoDevice);
        pContext->QueryInterface(__uuidof(ID3D11VideoContext), (void**)&m_VideoContext);

        D3D11_VIDEO_PROCESSOR_CONTENT_DESC contentDesc =
        {
            D3D11_VIDEO_FRAME_FORMAT_PROGRESSIVE,
            { 1, 1 }, desc.Width, desc.Height,
            { 1, 1 }, desc.Width, desc.Height,
            D3D11_VIDEO_USAGE_PLAYBACK_NORMAL
        };

        if (FAILED(m_VideoDevice->CreateVideoProcessorEnumerator(&contentDesc, &m_VideoProcessorEnumerator)))
        {
            m_IsValid = false;
            return;
        }

        if (FAILED(m_VideoDevice->CreateVideoProcessor(m_VideoProcessorEnumerator, 0, &m_VideoProcessor)))
        {
            m_IsValid = false;
            return;
        }

        SetOutputColorSpace();
        SetStreamColorSpace();

        D3D11_VIDEO_PROCESSOR_INPUT_VIEW_DESC inputViewDesc = { 0, D3D11_VPIV_DIMENSION_TEXTURE2D, { 0, 0 } };
        if (FAILED(m_VideoDevice->CreateVideoProcessorInputView(m_TexBgra, m_VideoProcessorEnumerator, &inputViewDesc, &m_InputView)))
        {
            m_IsValid = false;
            return;
        }

        m_IsValid = true;
    }

    void RGBToNV12ConverterD3D11::SetOutputColorSpace()
    {
        D3D11_VIDEO_PROCESSOR_COLOR_SPACE outputColorSpace;
        outputColorSpace.Usage = 0;
        outputColorSpace.RGB_Range = 0;
        outputColorSpace.YCbCr_Matrix = 1;
        outputColorSpace.YCbCr_xvYCC = 0;
        outputColorSpace.Nominal_Range = D3D11_VIDEO_PROCESSOR_NOMINAL_RANGE_16_235;

        m_VideoContext->VideoProcessorSetOutputColorSpace(m_VideoProcessor, &outputColorSpace);
    }

    void RGBToNV12ConverterD3D11::SetStreamColorSpace()
    {
        D3D11_VIDEO_PROCESSOR_COLOR_SPACE steamColorSpace;
        steamColorSpace.Usage = 1;
        steamColorSpace.RGB_Range = 0;
        steamColorSpace.YCbCr_Matrix = 1;
        steamColorSpace.YCbCr_xvYCC = 0;
        steamColorSpace.Nominal_Range = D3D11_VIDEO_PROCESSOR_NOMINAL_RANGE_0_255;

        m_VideoContext->VideoProcessorSetStreamColorSpace(m_VideoProcessor, 0, &steamColorSpace);
    }

    RGBToNV12ConverterD3D11::~RGBToNV12ConverterD3D11()
    {
        for (auto& it : m_OutputViewMap)
        {
            ID3D11VideoProcessorOutputView* pOutputView = it.second;
            pOutputView->Release();
        }

        m_InputView->Release();
        m_VideoProcessorEnumerator->Release();
        m_VideoProcessor->Release();
        m_VideoContext->Release();
        m_VideoDevice->Release();
        m_TexBgra->Release();
        m_D3D11Context->Release();
        m_D3D11Device->Release();
    }

    bool RGBToNV12ConverterD3D11::ConvertRGBToNV12(ID3D11Texture2D* const pRGBSrcTexture,
                                                   ID3D11Texture2D* const pDestTexture)
    {
        if (!m_IsValid)
            return false;

        m_D3D11Context->CopyResource(m_TexBgra, pRGBSrcTexture);
        ID3D11VideoProcessorOutputView* pOutputView = nullptr;
        auto it = m_OutputViewMap.find(pDestTexture);
        if (it == m_OutputViewMap.end())
        {
            D3D11_VIDEO_PROCESSOR_OUTPUT_VIEW_DESC outputViewDesc = { D3D11_VPOV_DIMENSION_TEXTURE2D };
            m_VideoDevice->CreateVideoProcessorOutputView(pDestTexture, m_VideoProcessorEnumerator, &outputViewDesc, &pOutputView);
            m_OutputViewMap.insert({ pDestTexture, pOutputView });
        }
        else
        {
            pOutputView = it->second;
        }

        D3D11_VIDEO_PROCESSOR_STREAM stream = { TRUE, 0, 0, 0, 0, NULL, m_InputView, NULL };
        return SUCCEEDED(m_VideoContext->VideoProcessorBlt(m_VideoProcessor, pOutputView, 0, 1, &stream));
    }
}
