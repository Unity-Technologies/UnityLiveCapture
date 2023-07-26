#pragma once

#include "nvEncodeAPI.h"
#include "d3d11.h"

#include <vector>
#include <atomic>
#include <fstream>
#include <sstream>
#include <unordered_map>

namespace NvencPlugin
{
    class RGBToNV12ConverterD3D11
    {
        using MapTextureOutputView = std::unordered_map<ID3D11Texture2D*, ID3D11VideoProcessorOutputView*>;

    public:
        RGBToNV12ConverterD3D11(ID3D11Device* pDevice,
                                ID3D11DeviceContext* pContext,
                                int nWidth,
                                int nHeight);

        ~RGBToNV12ConverterD3D11();

        bool ConvertRGBToNV12(ID3D11Texture2D* pRGBSrcTexture,
                              ID3D11Texture2D* pDestTexture);

    private:
        ID3D11Device*           m_D3D11Device;
        ID3D11DeviceContext*    m_D3D11Context;
        ID3D11VideoDevice*      m_VideoDevice;
        ID3D11VideoContext*     m_VideoContext;
        ID3D11VideoProcessor*   m_VideoProcessor;
        ID3D11Texture2D*        m_TexBgra;

        ID3D11VideoProcessorInputView*  m_InputView;
        ID3D11VideoProcessorOutputView* m_OutputView;
        ID3D11VideoProcessorEnumerator* m_VideoProcessorEnumerator;

        MapTextureOutputView m_OutputViewMap;
        bool                 m_IsValid;

        void SetOutputColorSpace();
        void SetStreamColorSpace();
    };
}
