#include <windows.h>
#include <iostream>
#include <sstream>
#include <fstream>

#include "d3d11.h"
#include "d3d12.h"

#include "NvencPluginEvents.h"
#include "NvencEncoder.h"
#include "ObjectIDMap.h"
#include "PluginUtils.h"

#include "D3D11EncoderDevice.h"
#include "D3D12EncoderDevice.h"

#include "Unity/IUnityRenderingExtensions.h"
#include "Unity/IUnityGraphicsD3D11.h"
#include "Unity/IUnityGraphicsD3D12.h"

// Disable the 'unscoped enum' Unity LIP warnings
#pragma warning(disable : 26812)

enum class VideoStreamRenderEventID
{
    Initialize = 0,
    Update,
    Encode,
    Finalize
};

namespace NvencPlugin
{
    static IUnityInterfaces*       s_UnityInterfaces = nullptr;
    static IUnityGraphics*         s_UnityGraphics = nullptr;
    static IUnityGraphicsD3D11*    s_UnityGraphicsD3D11 = nullptr;
    static IUnityGraphicsD3D12v5*  s_UnityGraphicsD3D12 = nullptr;

    static IGraphicsEncoderDevice* s_GraphicsEncoderDevice = nullptr;
    static IUnknown*               s_GraphicsDevice = nullptr;
    static bool                    s_Initialized = false;

    static IDObjectMap<NvEncoder>      s_EncoderMap;
    static IDObjectMap<EncodedFrame>   s_EncodedFrameMap;

#pragma region Low Level Plugin Interface
    // Override the function defining the load of the plugin
    extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
        UnityPluginLoad(IUnityInterfaces * unityInterfaces)
    {
        WriteFileDebug("Load plugin\n", false);
        if (unityInterfaces)
        {
            s_UnityInterfaces = unityInterfaces;

            const auto unityGraphics = s_UnityInterfaces->Get<IUnityGraphics>();
            if (unityGraphics)
            {
                s_UnityGraphics = unityGraphics;
                unityGraphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
            }
        }
    }

    // Override the function defining the unload of the plugin
    extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
    {
        WriteFileDebug("Unload plugin\n");
        if (s_UnityGraphics != nullptr)
        {
            s_UnityGraphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
        }
    }

    static bool GetRenderDeviceInterface(UnityGfxRenderer renderer)
    {
        switch (renderer)
        {
        case UnityGfxRenderer::kUnityGfxRendererD3D11:
            s_UnityGraphicsD3D11 = s_UnityInterfaces->Get<IUnityGraphicsD3D11>();
            s_GraphicsDevice = s_UnityGraphicsD3D11->GetDevice();
            return true;
        case UnityGfxRenderer::kUnityGfxRendererD3D12:
            s_UnityGraphicsD3D12 = s_UnityInterfaces->Get<IUnityGraphicsD3D12v5>();
            s_GraphicsDevice = s_UnityGraphicsD3D12->GetDevice();
            return true;
        default:
            WriteFileDebug("Error, graphics API not supported.\n");
            return false;
        }
    }

    // Override function to receive graphics event 
    static void UNITY_INTERFACE_API
        OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
    {
        WriteFileDebug("OnGraphicsDeviceEvent\n");

        if (eventType == kUnityGfxDeviceEventInitialize && !s_Initialized)
        {
            auto renderer = s_UnityInterfaces->Get<IUnityGraphics>()->GetRenderer();

            if (!GetRenderDeviceInterface(renderer))
                return;

            s_Initialized = true;
        }
        else if (eventType == kUnityGfxDeviceEventShutdown)
        {
            s_Initialized = false;
            s_UnityGraphicsD3D11 = nullptr;
            s_UnityGraphicsD3D12 = nullptr;
            s_GraphicsDevice = nullptr;
        }
    }

    extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
        GetRenderEventFunc()
    {
        return OnRenderEvent;
    }

    void Initialize(void* data);
    void Update(void* data);
    void Encode(void* data);
    void Finalize(void* data);

    // Plugin function to handle a specific rendering event
    static void UNITY_INTERFACE_API OnRenderEvent(int eventID, void* data)
    {
        auto event = static_cast<VideoStreamRenderEventID>(eventID);
        switch (event)
        {
        case VideoStreamRenderEventID::Initialize:
        {
            Initialize(data);
            break;
        }
        case VideoStreamRenderEventID::Update:
        {
            Update(data);
            break;
        }
        case VideoStreamRenderEventID::Encode:
        {
            Encode(data);
            break;
        }
        case VideoStreamRenderEventID::Finalize:
        {
            Finalize(data);
            break;
        }
        default:
            break;
        }
    }
#pragma endregion

#pragma region Render event commands
    // Verify if the data parameter and the D3D11 Device are valid.
    bool AreParametersValid(void* data)
    {
        if (!data)
        {
            WriteFileDebug("Error, Data send is null.\n");
            return true;
        }

        if (!s_GraphicsDevice)
        {
            WriteFileDebug("Error, s_D3D11Device is null.\n");
            return true;
        }

        return true;
    }

    void Initialize(void* data)
    {
        WriteFileDebug("OnRenderEvent: Initialize\n");

        if (!AreParametersValid(data))
            return;

        auto encoderData = static_cast<EncoderSettingsID*>(data);
        if (encoderData != nullptr)
        {
            WriteFileDebug("Initial Width: ", encoderData->settings.width);
            WriteFileDebug("Initial Height: ", encoderData->settings.height);
            WriteFileDebug("Initial FrameRate: ", encoderData->settings.frameRate);
            WriteFileDebug("Initial Bitrate: ", encoderData->settings.bitRate);
            WriteFileDebug("Initial GopSize: ", encoderData->settings.gopSize);

            if (s_GraphicsEncoderDevice == nullptr)
            {
                if (s_UnityGraphicsD3D11)
                {
                    auto d3d11Device = static_cast<ID3D11Device*>(s_GraphicsDevice);
                    s_GraphicsEncoderDevice = new D3D11EncoderDevice(d3d11Device);
                    WriteFileDebug("D3D11 encoder device succesfully created.\n");
                }
                else if (s_UnityGraphicsD3D12)
                {
                    auto d3d12Device = static_cast<ID3D12Device*>(s_GraphicsDevice);
                    s_GraphicsEncoderDevice = new D3D12EncoderDevice(d3d12Device, s_UnityGraphicsD3D12);
                    WriteFileDebug("D3D12 encoder device succesfully created.\n");
                }
                else
                {
                    WriteFileDebug("Error, graphics API failed to create an Encoder device.\n");
                    return;
                }

                if (!s_GraphicsEncoderDevice->Initialize())
                {
                    WriteFileDebug("Error, Failed to Initialize Graphics encoder device.\n");
                    return;
                }
            }

            bool forceNV12 = encoderData->encoderFormat != EncoderFormat::NV12;

            auto encoder = new NvEncoder(_NV_ENC_DEVICE_TYPE::NV_ENC_DEVICE_TYPE_DIRECTX,
                                         encoderData->settings,
                                         s_GraphicsEncoderDevice,
                                         forceNV12);

            if (encoder->InitEncoder() != NvencPlugin::ENvencStatus::Success)
            {
                WriteFileDebug("Error, Failed to Initialize 'InitEncoder'\n");
            }

            s_EncoderMap.Add(encoderData->id, encoder);
        }
        else
        {
            WriteFileDebug("Error, Initialize: invalid parameters.\n");
        }
    }

    void Update(void* data)
    {
        if (!AreParametersValid(data))
            return;

        auto encoderData = static_cast<EncoderSettingsID*>(data);
        if (encoderData && encoderData->id > 0)
        {
            auto encoder = s_EncoderMap.GetInstance(encoderData->id);
            if (encoder && encoder->UpdateEncoderSessionData(encoderData->settings))
            {
                WriteFileDebug("Info, Data has been updated.\n");
            }
        }
        else
        {
            WriteFileDebug("Error, Update: invalid parameters.\n");
        }
    }

    void Encode(void* data)
    {
        if (!AreParametersValid(data))
            return;

        auto encoderData = static_cast<EncoderTextureID*>(data);
        if (encoderData && encoderData->id > 0)
        {
            auto encoder = s_EncoderMap.GetInstance(encoderData->id);
            if (encoder)
            {
                encoder->EncodeFrame(encoderData->renderTexture, encoderData->timestamp);
            }
        }
    }

    void Finalize(void* data)
    {
        WriteFileDebug("OnRenderEvent: Finalize\n");

        auto id = static_cast<int*>(data);
        if (id && *id > 0)
        {
            auto encoder = s_EncoderMap.GetInstance(*id);
            if (encoder)
            {
                encoder->DestroyResources();
                delete encoder;
                encoder = nullptr;
                s_EncoderMap.Remove(*id);
            }
        }

        if (s_GraphicsEncoderDevice != nullptr)
        {
            s_GraphicsEncoderDevice->Cleanup();
            s_GraphicsEncoderDevice = nullptr;
        }

        s_Initialized = false;
    }
#pragma endregion

#pragma region Extern functions
    extern "C" bool UNITY_INTERFACE_EXPORT EncoderIsInitialized(int* id)
    {
        if (id && *id > 0)
        {
            auto encoder = s_EncoderMap.GetInstance(*id);
            return (encoder && encoder->IsInitialized());
        }
        return false;
    }

    extern "C" int UNITY_INTERFACE_EXPORT EncoderIsCompatible()
    {
        return static_cast<int>(NvencPlugin::NvEncoder::IsEncoderAvailable());
    }

    extern "C" bool UNITY_INTERFACE_EXPORT BeginConsume(int* id)
    {
        auto encoder = (id && *id > 0) ? s_EncoderMap.GetInstance(*id) : nullptr;

        auto currentFrame = (encoder && encoder->IsInitialized())
            ? encoder->GetEncodedFrame()
            : nullptr;

        if (currentFrame != nullptr)
        {
            s_EncodedFrameMap.Add(*id, currentFrame);
        }
        return currentFrame != nullptr;
    }

    extern "C" bool UNITY_INTERFACE_EXPORT EndConsume(int* id)
    {
        auto encoder = (id && *id > 0) ? s_EncoderMap.GetInstance(*id) : nullptr;
        if (encoder && encoder->IsInitialized() && s_EncodedFrameMap[*id] != nullptr)
        {
            s_EncodedFrameMap.Remove(*id);
            return encoder->RemoveEncodedFrame();
        }
        return false;
    }

    EncodedFrame* IsEncodedFrameValid(int* id)
    {
        if (id && *id > 0)
            return s_EncodedFrameMap[*id];

        return nullptr;
    }

    extern "C" uint32_t UNITY_INTERFACE_EXPORT GetSps(int* id, uint8_t * spsOut)
    {
        auto encodedFrame = IsEncodedFrameValid(id);
        if (encodedFrame == nullptr)
            return 0;

        const auto sizeSpsData = encodedFrame->spsSequence.size();
        if (spsOut != nullptr)
        {
            memcpy(spsOut, encodedFrame->spsSequence.data(), sizeSpsData);
        }
        return static_cast<uint32_t>(sizeSpsData);
    }

    extern "C" uint32_t UNITY_INTERFACE_EXPORT GetPps(int* id, uint8_t * ppsOut)
    {
        auto encodedFrame = IsEncodedFrameValid(id);
        if (encodedFrame == nullptr)
            return 0;

        const auto sizePpsData = encodedFrame->ppsSequence.size();
        if (ppsOut != nullptr)
        {
            memcpy(ppsOut, encodedFrame->ppsSequence.data(), sizePpsData);
        }
        return static_cast<uint32_t>(sizePpsData);
    }

    extern "C" uint32_t UNITY_INTERFACE_EXPORT GetEncodedData(int* id, uint8_t * dataOut)
    {
        auto encodedFrame = IsEncodedFrameValid(id);
        if (encodedFrame == nullptr)
            return 0;

        const auto sizeImageData = encodedFrame->imageData.size();
        if (dataOut != nullptr)
        {
            memcpy(dataOut, encodedFrame->imageData.data(), sizeImageData);
        }
        return static_cast<uint32_t>(sizeImageData);
    }

    extern "C" unsigned long long int UNITY_INTERFACE_EXPORT GetTimeStamp(int* id)
    {
        auto encodedFrame = IsEncodedFrameValid(id);
        if (encodedFrame == nullptr)
            return 0;

        return encodedFrame->timestamp;
    }

    extern "C" bool UNITY_INTERFACE_EXPORT GetIsKeyFrame(int* id)
    {
        auto encodedFrame = IsEncodedFrameValid(id);
        if (encodedFrame == nullptr)
            return 0;

        return encodedFrame->isKeyFrame;
    }
#pragma endregion
}
