#import <Foundation/Foundation.h>
#include "TargetConditionals.h"

#include "Unity/IUnityGraphicsMetal.h"
#include "Unity/IUnityRenderingExtensions.h"

#include "ObjectIDMap.hpp"
#include "PluginUtils.hpp"
#include "MacOSEncoderSessionDataPlugin.hpp"

#include "Encoder/H264Encoder.mm"
#include "Encoder/MetalGraphicsEncoderDevice.hpp"

// Disable the 'unscoped enum' Unity LIP warnings
#pragma warning(disable : 26812)

enum class VideoStreamRenderEventID
{
    Initialize = 0,
    Update,
    Encode,
    Finalize
};

using namespace MacOsEncodingPlugin;

namespace MacOsEncodingPlugin
{
    static IUnityInterfaces*         s_UnityInterfaces = nullptr;
    static IUnityGraphics*           s_UnityGraphics = nullptr;
    static IUnityGraphicsMetalV1*    s_MetalGraphics = nullptr;
    static MetalGraphicsEncoderDevice* s_GraphicsEncoderDevice = nullptr;
    static bool                      s_Initialized = false;
    
    static IDObjectMap<H264Encoder>  s_EncoderMap;
    static IDObjectMap<EncodedFrame> s_EncodedFrameMap;
    
#ifdef DEBUG_LOG
    static std::once_flag InitLogOnce;
#endif
    
    static bool GetRenderDeviceInterface(UnityGfxRenderer renderer)
    {
        switch (renderer)
        {
        case UnityGfxRenderer::kUnityGfxRendererMetal:
            s_MetalGraphics = s_UnityInterfaces->Get<IUnityGraphicsMetalV1>();
            return true;
        default:
            WriteFileDebug("Error - [GetRenderDeviceInterface] graphics API not supported.\n");
            return false;
        }
    }
    
    // Override function to receive graphics event
    static void UNITY_INTERFACE_API
        OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
    {
#ifdef DEBUG_LOG
        WriteFileDebug("Info - [OnGraphicsDeviceEvent] On graphics device event.\n");
#endif

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
            s_UnityInterfaces = nullptr;
            s_UnityGraphics = nullptr;
            s_MetalGraphics = nullptr;
            s_GraphicsEncoderDevice = nullptr;
        }
    }
   
    // Override the function defining the load of the plugin
    extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
        UnityPluginLoad(IUnityInterfaces * unityInterfaces)
    {
#ifdef DEBUG_LOG
        std::call_once(InitLogOnce, MacOsEncodingPlugin::InitLog);
#endif
        
        WriteFileDebug("Info - [UnityPluginLoad] Load plugin.\n", false);
        if (unityInterfaces)
        {
            s_UnityInterfaces = unityInterfaces;

            const auto unityGraphics = s_UnityInterfaces->Get<IUnityGraphics>();
            if (unityGraphics)
            {
                s_UnityGraphics = unityGraphics;
                unityGraphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
                
                OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
            }
        }
    }

    // Override the function defining the unload of the plugin
    extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
    {
        WriteFileDebug("Info - [UnityPluginUnload] Unload plugin.\n");
        if (s_UnityGraphics != nullptr)
        {
            s_UnityGraphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
        }
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

    extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
        GetRenderEventFunc()
    {
        return OnRenderEvent;
    }
    
    // Verify if the data parameter and the D3D11 Device are valid.
    bool AreParametersValid(void* data)
    {
        if (!data)
        {
            WriteFileDebug("Error, Data send is null.\n");
            return true;
        }

        if (!s_MetalGraphics)
        {
            WriteFileDebug("Error, s_MetalGraphics is null.\n");
            return true;
        }

        return true;
    }
    
    void Initialize(void* data)
    {
        WriteFileDebug("Info - [Initialize] Calling event.\n");
        
        if (!AreParametersValid(data))
        {
            WriteFileDebug("Error - [Initialize] Invalid data.\n");
            return;
        }
        
        auto encoderData = static_cast<EncoderSettingsID*>(data);
        if (encoderData == nullptr)
        {
            WriteFileDebug("Error - [Initialize] Invalid encoder data.\n");
            return;
        }
        
        WriteFileDebug("Info - [Initialize] Width: ", encoderData->settings.width);
        WriteFileDebug("Info - [Initialize] Height: ", encoderData->settings.height);
        WriteFileDebug("Info - [Initialize] FrameRate: ", encoderData->settings.frameRate);
        WriteFileDebug("Info - [Initialize] Bitrate: ", encoderData->settings.bitRate);
        WriteFileDebug("Info - [Initialize] GopSize: ", encoderData->settings.gopSize);
        
        if (s_GraphicsEncoderDevice == nullptr && s_MetalGraphics)
        {
            id<MTLDevice> device = s_MetalGraphics->MetalDevice();
            s_GraphicsEncoderDevice = new MetalGraphicsEncoderDevice(device, s_MetalGraphics);
        }
        else
        {
            WriteFileDebug("Error - [Initialize] Encoder device is invalid.\n");
        }
        
        auto metalDevice = static_cast<MetalGraphicsEncoderDevice*>(s_GraphicsEncoderDevice);
        auto instanceEncoder = new H264Encoder(encoderData->settings, metalDevice);
        instanceEncoder->Initialize(encoderData->useSRGB);
        
        s_EncoderMap.Add(encoderData->id, instanceEncoder);
        
        WriteFileDebug("Error - [Initialize] Added encoder ", encoderData->id);
    }

    void Update(void* data)
    {
        /*
        WriteFileDebug("Info - [Initialize] Calling event.\n");
        
        if (!AreParametersValid(data))
            return;
        
        auto encoderData = static_cast<EncoderSettingsID*>(data);
        if (encoderData && encoderData->id > 0)
        {
            auto encoder = s_EncoderMap.GetInstance(encoderData->id);
            if (encoder && encoder->UpdateEncoderSessionData(encoderData->settings))
            {
                WriteFileDebug("Info - [Update] Data has been updated.\n");
            }
        }
        else
        {
            WriteFileDebug("Error - [Update] invalid parameters.\n");
        }
         */
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
        WriteFileDebug("Info - [Finalize] Calling event.\n");
        
        if (!AreParametersValid(data))
            return;
        
        WriteFileDebug("Info - [Finalize] Parameters are valid.\n");
        
        auto id = *(int*)data;
        
        if (id > 0)
        {
            WriteFileDebug("Info - [Finalize] id is valid ", id);
            
            auto encoder = s_EncoderMap.GetInstance(id);
            if (encoder)
            {
                WriteFileDebug("Info - [Finalize] encoder is valid.\n");

                encoder->Dispose();
                delete encoder;
                encoder = nullptr;
                s_EncoderMap.Remove(id);
                WriteFileDebug("Info - [Finalize] Device deleted and removed ", id);
            }
            else
            {
                WriteFileDebug("Info - [Finalize] encoder is null.\n");
            }
        }
        
        s_Initialized = false;
    }
    
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
        return static_cast<int>(true);
    }

    extern "C" bool UNITY_INTERFACE_EXPORT BeginConsume(int* id)
    {
        auto encoder = (id && *id > 0) ? s_EncoderMap.GetInstance(*id) : nullptr;

        auto currentFrame = (encoder && encoder->IsInitialized())
            ? encoder->GetEncodedFrame()
            : nullptr;
        
        bool isValid = currentFrame != nullptr;
        if (isValid)
        {
            s_EncodedFrameMap.Add(*id, currentFrame);
        }
        
        return isValid;
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

 






    void LogDataSPS(EncodedFrame* frame)
    {
        std::ostringstream ossSPS;

        if (!frame->spsSequence.empty())
        {
           // Convert all but the last element to avoid a trailing ","
           std::copy(frame->spsSequence.begin(), frame->spsSequence.end(),
           std::ostream_iterator<int>(ossSPS, ","));

           // Now add the last element with no delimiter
            ossSPS << frame->spsSequence.back();
            ossSPS << "\n";
        }
        
        WriteFileDebug("Info: [postEncodeParser] - SPS DATA IS: ", (int)frame->spsSequence.size(), true);
        WriteFileDebug(ossSPS.str().c_str());
    }

    void LogDataPPS(EncodedFrame* frame)
    {
        std::ostringstream ossPPS;

        if (!frame->ppsSequence.empty())
        {
           // Convert all but the last element to avoid a trailing ","
           std::copy(frame->ppsSequence.begin(), frame->ppsSequence.end(),
           std::ostream_iterator<int>(ossPPS, ","));

           // Now add the last element with no delimiter
            ossPPS << frame->ppsSequence.back();
            ossPPS << "\n";
        }
        
        WriteFileDebug("Info: [postEncodeParser] - PPS DATA IS: ", (int)frame->ppsSequence.size(), true);
        WriteFileDebug(ossPPS.str().c_str());
    }


    void LogDataIMG(EncodedFrame* frame)
    {
        std::ostringstream ossImg;

        if (!frame->imageData.empty())
        {
           // Convert all but the last element to avoid a trailing ","
           std::copy(frame->imageData.begin(), frame->imageData.end() - 1,
           std::ostream_iterator<int>(ossImg, ","));

           // Now add the last element with no delimiter
            ossImg << frame->imageData.back();
            ossImg << "\n";
        }
        
        WriteFileDebug("Info: [postEncodeParser] - IMG DATA IS: ", (int)frame->imageData.size(), true);
        WriteFileDebug(ossImg.str().c_str());
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
            
            ///WriteFileDebug("Info - [GetSps] SPS size: ", sizeSpsData, true);
            //LogDataSPS(encodedFrame);
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
            ///WriteFileDebug("Info - [GetSps] PPS size: ", sizePpsData, true);
            //LogDataPPS(encodedFrame);
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
            ///WriteFileDebug("Info - [GetSps] IMG size: ", sizeImageData, true);
            //LogDataIMG(encodedFrame);
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
}
