#include <iostream>
#include <sstream>
#include <fstream>
#include <algorithm>

#include "NvencEncoder.h"
#include "windows.h"
#include "D3D12Texture2D.h"
#include "PluginUtils.h"
#include "D3D11EncoderDevice.h"

#include "Unity/IUnityProfiler.h"

// Disable the 'unscoped enum' Nvenc warnings
#pragma warning(disable : 26812)

namespace NvencPlugin
{
#pragma region Codec & Initialize API

    ENvencSupport NvEncoder::IsEncoderAvailable()
    {
        auto module = LoadModule();

        if (module == nullptr)
        {
            return ENvencSupport::NoDriver;
        }
        else if (!CheckDriverVersion(module))
        {
            return ENvencSupport::DriverVersionNotSupported;
        }

        return ENvencSupport::Supported;
    }

    ENvencStatus NvEncoder::LoadCodec()
    {
        WriteFileDebug("Start to call: LoadCodec\n");

        m_Nvenc = { NV_ENCODE_API_FUNCTION_LIST_VER };

        auto module = LoadModule();
        if (module == nullptr)
        {
            WriteFileDebug("Error, DriverNotInstalled in NVENC library\n");
            return ENvencStatus::DriverNotInstalled;
        }
        m_HModule = module;

        if (!CheckDriverVersion(module))
        {
            WriteFileDebug("Error, DriverVersionDoesNotSupportAPI in NVENC library\n");
            return ENvencStatus::DriverVersionDoesNotSupportAPI;
        }

#if defined(_WIN32)
        auto NvEncodeAPICreateInstance =
            (NvEncodeAPICreateInstance_Type)GetProcAddress((HMODULE)m_HModule, "NvEncodeAPICreateInstance");
#else
        auto NvEncodeAPICreateInstance =
            (NvEncodeAPICreateInstance_Type)dlsym(s_HModule, "NvEncodeAPICreateInstance");
#endif

        if (!NvEncodeAPICreateInstance)
        {
            WriteFileDebug("Error, APINotFound (NvEncodeAPICreateInstance) in NVENC library\n");
            return ENvencStatus::APINotFound;
        }

        if (NvEncodeAPICreateInstance(&m_Nvenc) != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, APINotFound (NvEncodeAPICreateInstance) in Nvenc.\n");
            return ENvencStatus::APINotFound;
        }

        WriteFileDebug("End to call: LoadCodec\n");

        return ENvencStatus::Success;
    }

    bool NvEncoder::CheckDriverVersion(HMODULE module)
    {
        using NvEncodeAPIGetMaxSupportedVersion_Type = NVENCSTATUS(NVENCAPI*)(uint32_t*);
#if defined(_WIN32)
        auto NvEncodeAPIGetMaxSupportedVersion =
            (NvEncodeAPIGetMaxSupportedVersion_Type)GetProcAddress(module, "NvEncodeAPIGetMaxSupportedVersion");
#else
        auto NvEncodeAPIGetMaxSupportedVersion =
            (NvEncodeAPIGetMaxSupportedVersion_Type)dlsym(s_HModule, "NvEncodeAPIGetMaxSupportedVersion");
#endif
        uint32_t version = 0;
        uint32_t currentVersion = (NVENCAPI_MAJOR_VERSION << 4) | NVENCAPI_MINOR_VERSION;
        NvEncodeAPIGetMaxSupportedVersion(&version);
        return (currentVersion > version) ? false : true;
    }

    HMODULE NvEncoder::LoadModule()
    {
#if defined(_WIN32)
#if defined(_WIN64)
        HMODULE module = LoadLibrary(TEXT("nvEncodeAPI64.dll"));
#else
        HMODULE module = LoadLibrary(TEXT("nvEncodeAPI.dll"));
#endif
#else
        void* module = dlopen("libnvidia-encode.so.1", RTLD_LAZY);
#endif

        return module;
    }
#pragma endregion

#pragma region Constructor & Initialize
    NvEncoder::NvEncoder(const NV_ENC_DEVICE_TYPE deviceType,
        const NvencEncoderSessionData& other,
        IGraphicsEncoderDevice* device,
        bool forceNv12) :
        m_Device(device),
        m_HModule(nullptr),
        m_HEncoder(nullptr),
        m_InitializationResult(ENvencStatus::NotInitialized),
        m_IsIdrFrame(false),
        m_DeviceType(NV_ENC_DEVICE_TYPE_DIRECTX),
        m_NvEncConfig{ 0 },
        m_FrameData(other),
        m_FrameCount(0),
        m_GOPCount(0),
        m_ForceNV12(forceNv12),
        m_Thread(nullptr),
        m_IsAsync(false)
    {
        WriteFileDebug("--- Initialize NvEncoder ---\n", false);

        for (auto& renderTexture : m_RenderTextures)
        {
            renderTexture = nullptr;
        }
    }

    ENvencStatus NvEncoder::InitEncoder()
    {
        WriteFileDebug("Start to call: InitEncoder\n");

        if (m_InitializationResult == ENvencStatus::NotInitialized)
        {
            m_InitializationResult = LoadCodec();
        }

        if (m_InitializationResult != ENvencStatus::Success)
        {
            WriteFileDebug("Nvec failed to initialize (LoadCodec).\n");
            return m_InitializationResult;
        }

        auto device = m_Device->GetDevice();
        if (device == nullptr)
        {
            WriteFileDebug("Error, graphics device is null.\n");
            return ENvencStatus::NotInitialized;
        }

        if (!m_Nvenc.nvEncOpenEncodeSession)
        {
            WriteFileDebug("Error, EncodeAPI not found.\n");
        }

        NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS openEncodeSessionExParams = { 0 };
        openEncodeSessionExParams.version = NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER;
        openEncodeSessionExParams.device = static_cast<void*>(device);
        openEncodeSessionExParams.deviceType = m_DeviceType;
        openEncodeSessionExParams.apiVersion = NVENCAPI_VERSION;

        const auto errorCode = m_Nvenc.nvEncOpenEncodeSessionEx(&openEncodeSessionExParams, &m_HEncoder);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, nvEncOpenEncodeSessionEx failed.\n");
            m_InitializationResult = ENvencStatus::EncoderInitializationFailed;
            return m_InitializationResult;
        }

        SetEncoderParameters();

        m_Device->InitializeConverter(m_FrameData.width, m_FrameData.height);

        WriteFileDebug("End to call: InitEncoder\n");
        m_InitializationResult = ENvencStatus::Success;
        return m_InitializationResult;
    }

    void NvEncoder::SetEncoderParameters()
    {
        m_NvEncInitializeParams = { NV_ENC_INITIALIZE_PARAMS_VER };
        NV_ENC_CONFIG encodeConfig = { NV_ENC_CONFIG_VER };

        m_NvEncInitializeParams.encodeConfig = &encodeConfig;
        memset(m_NvEncInitializeParams.encodeConfig, 0, sizeof(NV_ENC_CONFIG));

        if (m_FrameData.width > k_MaxWidth || m_FrameData.height > k_MaxHeight ||
            m_FrameData.width < 0 || m_FrameData.height < 0)
        {
            WriteFileDebug("Error, size is invalid.\n");
        }

        // Set initialization parameters
        m_NvEncInitializeParams.encodeConfig->version = NV_ENC_CONFIG_VER;;
        m_NvEncInitializeParams.version = NV_ENC_INITIALIZE_PARAMS_VER;
        m_NvEncInitializeParams.encodeWidth = m_FrameData.width;
        m_NvEncInitializeParams.encodeHeight = m_FrameData.height;
        m_NvEncInitializeParams.darWidth = m_NvEncInitializeParams.encodeWidth;
        m_NvEncInitializeParams.darHeight = m_NvEncInitializeParams.encodeHeight;
        m_NvEncInitializeParams.encodeGUID = NV_ENC_CODEC_H264_GUID;
        m_NvEncInitializeParams.presetGUID = NV_ENC_PRESET_LOW_LATENCY_HP_GUID;
        m_NvEncInitializeParams.frameRateNum = m_FrameData.frameRate;
        m_NvEncInitializeParams.frameRateDen = 1;
        m_NvEncInitializeParams.enablePTD = 1;
        m_NvEncInitializeParams.reportSliceOffsets = 0;
        m_NvEncInitializeParams.enableSubFrameWrite = 0;
        m_NvEncInitializeParams.maxEncodeWidth = 3840;
        m_NvEncInitializeParams.maxEncodeHeight = 2160;

        // Get encoder capability
        NV_ENC_CAPS_PARAM capsParam = { 0 };
        capsParam.version = NV_ENC_CAPS_PARAM_VER;
        capsParam.capsToQuery = NV_ENC_CAPS_ASYNC_ENCODE_SUPPORT;
        signed int asyncMode = 0;
        auto errorCode = m_Nvenc.nvEncGetEncodeCaps(m_HEncoder,
                                                    m_NvEncInitializeParams.encodeGUID,
                                                    &capsParam,
                                                    &asyncMode);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, Failed to get NVEncoder capability params.\n");
        }

        if (asyncMode == 1)
        {
            m_IsAsync = m_Device->InitializeMultithreadingSecurity() && std::thread::hardware_concurrency() > 0;
            m_NvEncInitializeParams.enableEncodeAsync = static_cast<int>(m_IsAsync);

            if (m_IsAsync)
            {
                WriteFileDebug("Info, AsyncMode is enabled.\n");
                // The second thread is used to retrieve the data when async mode is available.
                m_Thread = new NvThread(std::thread(ProcessEncodedFrameAsyncSingle, this));
            }
            else
                WriteFileDebug("Info, AsyncMode is disabled.\n");
        }
        else
            WriteFileDebug("Error, AsyncMode is disabled.\n");

        m_NvEncInitializeParams.encodeConfig = &m_NvEncConfig;

        // Get and set preset config
        NV_ENC_PRESET_CONFIG presetConfig = { NV_ENC_PRESET_CONFIG_VER, { NV_ENC_CONFIG_VER } };

        errorCode = m_Nvenc.nvEncGetEncodePresetConfig(m_HEncoder,
            m_NvEncInitializeParams.encodeGUID,
            m_NvEncInitializeParams.presetGUID,
            &presetConfig);

        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, Failed to select NVEncoder preset config.\n");
        }

        std::memcpy(&m_NvEncConfig, &presetConfig.presetCfg, sizeof(NV_ENC_CONFIG));
        m_NvEncConfig.profileGUID = NV_ENC_H264_PROFILE_BASELINE_GUID;
        m_NvEncConfig.frameIntervalP = 1;
        m_NvEncConfig.gopLength = NVENC_INFINITE_GOPLENGTH;

        m_NvEncConfig.encodeCodecConfig.h264Config.idrPeriod = m_NvEncConfig.gopLength;
        m_NvEncConfig.encodeCodecConfig.h264Config.sliceMode = 0;
        m_NvEncConfig.encodeCodecConfig.h264Config.sliceModeData = 0;
        m_NvEncConfig.encodeCodecConfig.h264Config.disableSPSPPS = 1;
        m_NvEncConfig.encodeCodecConfig.h264Config.repeatSPSPPS = 1;
        m_NvEncConfig.encodeCodecConfig.h264Config.enableIntraRefresh = 1;
        m_NvEncConfig.encodeCodecConfig.h264Config.level = NV_ENC_LEVEL_AUTOSELECT;
        m_NvEncConfig.version = NV_ENC_CONFIG_VER;

        m_NvEncConfig.rcParams.rateControlMode = NV_ENC_PARAMS_RC_CBR;

        /* This parameter doesn't work (failed to initialized input parameters).
        m_NvEncConfig.rcParams.multiPass = NV_ENC_TWO_PASS_FULL_RESOLUTION;
        */

        /* From Nvenc sample, the average is about 500 000 for 1080p. It's pretty low.
        m_NvEncConfig.rcParams.averageBitRate =
            (static_cast<unsigned int>(5.0f * m_NvEncInitializeParams.encodeWidth
            * m_NvEncInitializeParams.encodeHeight)
            / (m_NvEncInitializeParams.encodeWidth * m_NvEncInitializeParams.encodeHeight))
            * 100000;
        */

        m_NvEncConfig.rcParams.averageBitRate = m_FrameData.bitRate;
        m_NvEncConfig.rcParams.maxBitRate = m_NvEncConfig.rcParams.averageBitRate;
        m_NvEncConfig.rcParams.constQP = { 28, 31, 25 };
        m_NvEncConfig.rcParams.enableAQ = 1;
        m_NvEncConfig.rcParams.vbvBufferSize = (m_NvEncConfig.rcParams.averageBitRate
            * m_NvEncInitializeParams.frameRateDen
            / m_NvEncInitializeParams.frameRateNum);
        m_NvEncConfig.rcParams.vbvInitialDelay = m_NvEncConfig.rcParams.vbvBufferSize;

        // Initialize hardware encoder session
        errorCode = m_Nvenc.nvEncInitializeEncoder(m_HEncoder, &m_NvEncInitializeParams);

        if (errorCode != NV_ENC_SUCCESS)
        {
            std::ostringstream errorLog;
            WriteFileDebug("Error, Failed to initialize NVEncoder.\n");
            errorLog << "Error is: " << errorCode << "\n";
            auto test = errorLog.str();
            WriteFileDebug(test.c_str());
            return;
        }
        else
        {
            WriteFileDebug("Success, initialized NVEncoder.\n");
        }

        if (m_IsAsync)
        {
            InitializeAsyncResources();
        }

        InitEncoderResources();
    }

    void NvEncoder::InitializeAsyncResources()
    {
        m_vpCompletionEvent.resize(k_BufferedFrameNum, nullptr);

        for (uint32_t i = 0; i < m_vpCompletionEvent.size(); i++)
        {
            m_vpCompletionEvent[i] = CreateEvent(NULL, FALSE, FALSE, NULL);
            NV_ENC_EVENT_PARAMS eventParams = { NV_ENC_EVENT_PARAMS_VER };
            eventParams.completionEvent = m_vpCompletionEvent[i];
            m_Nvenc.nvEncRegisterAsyncEvent(m_HEncoder, &eventParams);
        }
    }

    void NvEncoder::InitEncoderResources()
    {
        for (auto i = 0; i < k_BufferedFrameNum; i++)
        {
            m_RenderTextures[i] = m_Device->CreateDefaultTexture(m_FrameData.width, m_FrameData.height, m_ForceNV12);

            auto& frame = m_BufferedFrames[i];
            const auto format = (m_ForceNV12) ? NV_ENC_BUFFER_FORMAT_NV12 : NV_ENC_BUFFER_FORMAT_ARGB;

            auto registeredTexture = (m_ForceNV12)
                ? m_RenderTextures[i]->GetNV12Texture()
                : m_RenderTextures[i]->GetEncodeTexturePtrV();

            frame.inputFrame.registeredResource = RegisterResource(registeredTexture, format);


            frame.inputFrame.bufferFormat = format;
            MapResources(frame.inputFrame);
            frame.outputFrame = InitializeBitstreamBuffer();

            WriteFileDebug("Allocate one frame buffer.\n");
        }
    }

    NV_ENC_REGISTERED_PTR NvEncoder::RegisterResource(void* const buffer, NV_ENC_BUFFER_FORMAT format)
    {
        NV_ENC_REGISTER_RESOURCE registerResource = { 0 };
        registerResource.version = NV_ENC_REGISTER_RESOURCE_VER;
        registerResource.resourceType = NV_ENC_INPUT_RESOURCE_TYPE_DIRECTX;
        registerResource.resourceToRegister = buffer;

        if (!registerResource.resourceToRegister)
        {
            WriteFileDebug("Error, ResourceToRegister: resource is not initialized.\n");
        }
        registerResource.width = m_FrameData.width;
        registerResource.height = m_FrameData.height;
        registerResource.bufferFormat = format;
        registerResource.bufferUsage = NV_ENC_INPUT_IMAGE;

        const auto errorCode = m_Nvenc.nvEncRegisterResource(m_HEncoder, &registerResource);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, Error on register resource: nvEncRegisterResource.\n");
        }
        return registerResource.registeredResource;
    }

    NV_ENC_OUTPUT_PTR NvEncoder::InitializeBitstreamBuffer()
    {
        NV_ENC_CREATE_BITSTREAM_BUFFER createBitstreamBuffer = { 0 };
        createBitstreamBuffer.version = NV_ENC_CREATE_BITSTREAM_BUFFER_VER;

        const auto errorCode = m_Nvenc.nvEncCreateBitstreamBuffer(m_HEncoder, &createBitstreamBuffer);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, Error on creation: nvEncCreateBitstreamBuffer.\n");
        }
        return createBitstreamBuffer.bitstreamBuffer;
    }

    void NvEncoder::MapResources(InputFrame& inputFrame)
    {
        NV_ENC_MAP_INPUT_RESOURCE mapInputResource = { 0 };
        mapInputResource.version = NV_ENC_MAP_INPUT_RESOURCE_VER;
        mapInputResource.registeredResource = inputFrame.registeredResource;

        const auto errorCode = m_Nvenc.nvEncMapInputResource(m_HEncoder, &mapInputResource);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error on creation: nvEncCreateBitstreamBuffer.\n");
        }
        inputFrame.mappedResource = mapInputResource.mappedResource;
    }

#pragma endregion

#pragma region Update settings & Encode frames
    bool NvEncoder::UpdateEncoderSessionData(const NvencEncoderSessionData& other)
    {
        const auto updateData = !(other == m_FrameData);
        if (updateData)
        {
            m_FrameData.Update(other);
            UpdateSettings();
        }
        return updateData;
    }

    void NvEncoder::UpdateSettings()
    {
        auto settingChanged = false;
        auto sizeChanged = false;

        if (m_NvEncInitializeParams.frameRateNum != m_FrameData.frameRate)
        {
            m_NvEncInitializeParams.frameRateNum = m_FrameData.frameRate;
            settingChanged = true;
        }

        if (m_NvEncInitializeParams.encodeWidth != m_FrameData.width)
        {
            m_NvEncInitializeParams.encodeWidth = m_FrameData.width;
            m_NvEncInitializeParams.darWidth = m_FrameData.width;
            settingChanged = sizeChanged = true;
        }

        if (m_NvEncInitializeParams.encodeHeight != m_FrameData.height)
        {
            m_NvEncInitializeParams.encodeHeight = m_FrameData.height;
            m_NvEncInitializeParams.darHeight = m_FrameData.height;
            settingChanged = sizeChanged = true;
        }

        if (m_NvEncConfig.rcParams.averageBitRate != m_FrameData.bitRate)
        {
            m_NvEncConfig.rcParams.averageBitRate = m_FrameData.bitRate;
            m_NvEncConfig.rcParams.maxBitRate = m_FrameData.bitRate;
            settingChanged = true;
            WriteFileDebug("New bitrate value: ", m_FrameData.bitRate);
        }

        if (settingChanged)
        {
            NV_ENC_RECONFIGURE_PARAMS nvEncReconfigureParams;
            std::memcpy(&nvEncReconfigureParams.reInitEncodeParams,
                &m_NvEncInitializeParams,
                sizeof(m_NvEncInitializeParams));

            nvEncReconfigureParams.version = NV_ENC_RECONFIGURE_PARAMS_VER;
            nvEncReconfigureParams.forceIDR = 1;
            nvEncReconfigureParams.resetEncoder = 1;

            const auto result = m_Nvenc.nvEncReconfigureEncoder(m_HEncoder, &nvEncReconfigureParams);
            if (result != NV_ENC_SUCCESS)
            {
                WriteFileDebug("Failed to reconfigure encoder setting.\n");
            }

            // Reconfigure the Textures size (width & height).
            if (sizeChanged)
            {
                ReleaseEncoderResources();
                InitEncoderResources();

                m_Device->InitializeConverter(m_FrameData.width, m_FrameData.height);

                WriteFileDebug("New Width: ", m_FrameData.width);
                WriteFileDebug("New Height: ", m_FrameData.height);
                WriteFileDebug("New FrameRate: ", m_FrameData.frameRate);
            }
        }
    }

    void* NvEncoder::GetCompletionEvent(uint32_t eventIdx)
    {
        return (m_vpCompletionEvent.size() == k_BufferedFrameNum)
            ? m_vpCompletionEvent[eventIdx]
            : nullptr;
    }

    bool NvEncoder::CopyBufferResources(int frameIndex, void* frameSourceData)
    {
        const auto destTexture = m_RenderTextures[frameIndex];

        if (!destTexture || !frameSourceData)
        {
            WriteFileDebug("Error, incorrect input texture(s).\n");
            return false;
        }

        auto nativeSrc = static_cast<IUnknown*>(frameSourceData);

        if (!destTexture || !nativeSrc)
        {
            WriteFileDebug("Error, invalid IUnknown resource(s).\n");
            return false;
        }

        if (m_ForceNV12)
        {
            if (!m_Device->ConvertRGBToNV12(nativeSrc, destTexture))
            {
                WriteFileDebug("Error, Conversion from RGB to NV12 failed.\n");
            }
        }
        else
        {
            if (!m_Device->CopyResource(nativeSrc, destTexture))
            {
                WriteFileDebug("Error, Couldn't copy resources.\n");
                return false;
            }

        }
        return true;
    }

    void NvEncoder::EncodeFrame(void* frameSourceData, unsigned long long int timeStamp)
    {
        if (frameSourceData == nullptr)
        {
            WriteFileDebug("Error, Encoded frame data is null.\n");
            return;
        }

        const int frameIndex = m_FrameCount % k_BufferedFrameNum;

        if (!CopyBufferResources(frameIndex, frameSourceData))
        {
            WriteFileDebug("Error, copy resources failed.\n");
            return;
        }

        WriteFileDebug("Info, Start encoding new frame.\n");

        auto& bufferedFrame = m_BufferedFrames[frameIndex];

        if (bufferedFrame.isEncoding)
        {
            WriteFileDebug("Error: frame is already encoding.\n");
            return;
        }
        bufferedFrame.isEncoded = false;
        bufferedFrame.isEncoding = true;

        NV_ENC_PIC_PARAMS picParams = { 0 };
        picParams.version = NV_ENC_PIC_PARAMS_VER;
        picParams.encodePicFlags = 0;
        picParams.pictureStruct = NV_ENC_PIC_STRUCT_FRAME;
        picParams.inputBuffer = bufferedFrame.inputFrame.mappedResource;
        picParams.bufferFmt = bufferedFrame.inputFrame.bufferFormat;
        picParams.inputWidth = m_NvEncInitializeParams.encodeWidth;
        picParams.inputHeight = m_NvEncInitializeParams.encodeHeight;
        picParams.outputBitstream = bufferedFrame.outputFrame;
        picParams.inputTimeStamp = m_FrameCount;

        if (m_NvEncInitializeParams.enableEncodeAsync == 1)
        {
            picParams.completionEvent = GetCompletionEvent(m_FrameCount % k_BufferedFrameNum);
        }

        const int gopIndex = m_GOPCount % k_GOPSize;
        bool isKeyFrame = gopIndex == 0;

        if (isKeyFrame)
        {
            picParams.encodePicFlags = NV_ENC_PIC_FLAG_FORCEIDR | NV_ENC_PIC_FLAG_OUTPUT_SPSPPS;
        }
        else
        {
            picParams.codecPicParams.h264PicParams.refPicFlag = 1;
        }
        m_GOPCount++;

        const auto errorCode = m_Nvenc.nvEncEncodePicture(m_HEncoder, &picParams);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Failed to encode frame: ", errorCode, true);
            bufferedFrame.isEncoding = false;
            return;
        }

        if (m_NvEncInitializeParams.enableEncodeAsync == 1)
        {
            EncodedFrameDataKey dataKey;
            dataKey.index = frameIndex;
            dataKey.timestamp = timeStamp;
            dataKey.isKeyFrame = (isKeyFrame);

            std::lock_guard<NvSpinlock> lock(m_NvSpinlock);
            m_BufferToRead.push(dataKey);

            WriteFileDebug("Info, frameIndex added to the queue.\n");
        }
        else
        {
            ProcessEncodedFrame(bufferedFrame, timeStamp, gopIndex == 0);
            bufferedFrame.isEncoded = true;
        }

        m_FrameCount++;
    }

    Frame& NvEncoder::GetBufferedFrame(int index)
    {
        return m_BufferedFrames[index];
    }

    void NvEncoder::ProcessEncodedFrameAsyncSingle(NvEncoder* encoder)
    {
        while (encoder->m_IsAsync)
        {
            EncodedFrameDataKey dataKey;
            {
                std::lock_guard<NvSpinlock> lock(encoder->m_NvSpinlock);
                if (encoder->m_BufferToRead.empty())
                    continue;
                dataKey = encoder->m_BufferToRead.front();
                encoder->m_BufferToRead.pop();
            }

            if (WaitForSingleObject(encoder->m_vpCompletionEvent[dataKey.index], 1000) == WAIT_FAILED)
            {
                WriteFileDebug("Failed in the ProcessEncodedFrameAsync.\n");
                continue;
            }
            auto& frame = encoder->GetBufferedFrame(dataKey.index);
            encoder->ProcessEncodedFrame(frame, dataKey.timestamp, dataKey.isKeyFrame);
            frame.isEncoded = true;
            WriteFileDebug("Info, frameIndex used from the queue.\n");
        }
    }

    void NvEncoder::ProcessEncodedFrame(Frame& frame, unsigned long long int timestamp, bool isKeyFrame)
    {
        if (!frame.isEncoding)
        {
            WriteFileDebug("Error; the frame hasn't been encoded.\n");
            return;
        }
        frame.isEncoding = false;

        NV_ENC_LOCK_BITSTREAM lockBitStream = { 0 };
        lockBitStream.version = NV_ENC_LOCK_BITSTREAM_VER;
        lockBitStream.outputBitstream = frame.outputFrame;
        //lockBitStream.doNotWait = m_NvEncInitializeParams.enableEncodeAsync;

        auto errorCode = m_Nvenc.nvEncLockBitstream(m_HEncoder, &lockBitStream);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, failed to lock bit stream.\n");
        }

        if (lockBitStream.bitstreamSizeInBytes)
        {
            WriteFileDebug("Success, encoded size: ", static_cast<int>(lockBitStream.bitstreamSizeInBytes));
            frame.encodedFrame.resize(lockBitStream.bitstreamSizeInBytes);
            std::memcpy(frame.encodedFrame.data(), lockBitStream.bitstreamBufferPtr, lockBitStream.bitstreamSizeInBytes);
        }

        errorCode = m_Nvenc.nvEncUnlockBitstream(m_HEncoder, frame.outputFrame);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, failed to unlock bit stream.\n");
        }

        // Add encoded data to a queue.
        AddEncodedFrame(frame, timestamp, isKeyFrame);
    }
#pragma endregion

#pragma region Encoded frame actions
    void NvEncoder::AddEncodedFrame(Frame& frame, unsigned long long int timestamp, bool isKeyFrame)
    {
        EncodedFrame encodedFrame;
        encodedFrame.imageData = std::move(frame.encodedFrame);
        GetSequenceParams(encodedFrame.spsSequence, encodedFrame.ppsSequence);
        encodedFrame.timestamp = timestamp;
        encodedFrame.isKeyFrame = isKeyFrame;

        WriteFileDebug("--------\n");
        WriteFileDebug("IMG SIZE: ", encodedFrame.imageData.size(), true);
        WriteFileDebug("SPS SIZE: ", encodedFrame.spsSequence.size(), true);
        WriteFileDebug("PPS SIZE: ", encodedFrame.ppsSequence.size(), true);

        if (m_FrameQueue.size() < k_MaxQueueLength)
        {
            m_FrameQueue.push(std::move(encodedFrame));
            WriteFileDebug("Info, encoded frame added in the queue.\n");
        }
        else
        {
            m_FrameQueue.pop();
            m_FrameQueue.push(std::move(encodedFrame));

            WriteFileDebug("Warning, too much encoded frames in the queue.\n");
        }
    }

    EncodedFrame* NvEncoder::GetEncodedFrame()
    {
        if (m_FrameQueue.size() > 0)
        {
            return &m_FrameQueue.front();
        }
        return nullptr;
    }

    bool NvEncoder::RemoveEncodedFrame()
    {
        // Should always be true if it was true for the previous call.
        if (m_FrameQueue.size() > 0)
        {
            m_FrameQueue.pop();
            return true;
        }
        return false;
    }

    void NvEncoder::GetSequenceParams(DataSequence& spsSequence, DataSequence& ppsSequence)
    {
        uint8_t spsppsData[1024]; // Assume maximum spspps data is 1KB or less
        memset(spsppsData, 0, sizeof(spsppsData));
        spsSequence.clear();
        ppsSequence.clear();

        NV_ENC_SEQUENCE_PARAM_PAYLOAD payload = { NV_ENC_SEQUENCE_PARAM_PAYLOAD_VER };
        uint32_t spsppsSize = 0;

        payload.spsppsBuffer = spsppsData;
        payload.inBufferSize = sizeof(spsppsData);
        payload.outSPSPPSPayloadSize = &spsppsSize;

        const auto errorCode = m_Nvenc.nvEncGetSequenceParams(m_HEncoder, &payload);
        if (errorCode != NV_ENC_SUCCESS)
        {
            WriteFileDebug("Error, nvEncGetSequenceParams failed.\n");
            return;
        }

        // Get SPS index
        auto* sps = spsppsData;
        unsigned int i_sps = 4;

        while (spsppsData[i_sps] != 0x00
            || spsppsData[i_sps + 1] != 0x00
            || spsppsData[i_sps + 2] != 0x00
            || spsppsData[i_sps + 3] != 0x01)
        {
            i_sps += 1;
            if (i_sps >= spsppsSize)
            {
                WriteFileDebug("Error, Invalid SPS/PPS.\n");
                return;
            }
        }

        // Get PPS index
        auto* pps = spsppsData + i_sps;
        unsigned int i_pps = spsppsSize - i_sps;

        // Allocate memory for SPS
        spsSequence.clear();

        // Allocate memory for PPS
        ppsSequence.clear();

        // Insert elements for SPS & PPS
        spsSequence.insert(spsSequence.begin(), &sps[4], &sps[i_sps]);
        ppsSequence.insert(ppsSequence.begin(), &pps[4], &pps[i_pps]);
    }
#pragma endregion 

#pragma region Liberate resources
    void NvEncoder::DestroyResources()
    {
        auto asyncMode = m_IsAsync;
        if (m_IsAsync)
        {
            m_IsAsync = false;
            if (m_Thread != nullptr)
            delete m_Thread;

            DestroyAsyncResources();
        }

        ReleaseEncoderResources();
        ClearEncodedFrameQueue();

        if (m_HEncoder)
        {
            if (m_Nvenc.nvEncDestroyEncoder(m_HEncoder) != NV_ENC_SUCCESS)
            {
                WriteFileDebug("Failed to destroy NV encoder interface.\n");
            }
            m_HEncoder = nullptr;
        }

        UnloadModule();
        m_InitializationResult = ENvencStatus::NotInitialized;
    }

    void NvEncoder::UnloadModule()
    {

        if (m_HModule != nullptr)
        {
#if defined(_WIN32)
            FreeLibrary((HMODULE)m_HModule);
#else
            dlclose(s_hModule);
#endif
            m_HModule = nullptr;
        }
    }

    void NvEncoder::ReleaseEncoderResources()
    {
        if (m_InitializationResult != ENvencStatus::Success)
            return;

        for (Frame& frame : m_BufferedFrames)
        {
            ReleaseFrameInputBuffer(frame);

            auto errorCode = m_Nvenc.nvEncDestroyBitstreamBuffer(m_HEncoder, frame.outputFrame);
            if (errorCode != NV_ENC_SUCCESS)
            {
                WriteFileDebug("Error, failed to destroy output buffer bit stream.\n");
            }
            frame.outputFrame = nullptr;
        }

        if (m_RenderTextures != nullptr)
        {
            for (auto& renderTexture : m_RenderTextures)
            {
                if (renderTexture != nullptr)
                {
                    delete renderTexture;
                    renderTexture = nullptr;
                }
            }
        }
    }

    void NvEncoder::ReleaseFrameInputBuffer(Frame& frame)
    {
        if (m_HEncoder != nullptr)
        {
            auto errorCode = m_Nvenc.nvEncUnmapInputResource(m_HEncoder, frame.inputFrame.mappedResource);
            if (errorCode != NV_ENC_SUCCESS)
            {
                WriteFileDebug("Error, failed to unmap input resource.\n");
            }
            frame.inputFrame.mappedResource = nullptr;

            errorCode = m_Nvenc.nvEncUnregisterResource(m_HEncoder, frame.inputFrame.registeredResource);
            if (errorCode != NV_ENC_SUCCESS)
            {
                WriteFileDebug("Error, failed to unregister input buffer resource.\n");
            }
            frame.inputFrame.registeredResource = nullptr;
        }
    }

    void NvEncoder::ClearEncodedFrameQueue()
    {
        while (!m_FrameQueue.empty())
        {
            m_FrameQueue.pop();
        }
    }

    void NvEncoder::DestroyAsyncResources()
    {
        for (uint32_t i = 0; i < m_vpCompletionEvent.size(); i++)
        {
            if (m_vpCompletionEvent[i])
            {
                NV_ENC_EVENT_PARAMS eventParams = { NV_ENC_EVENT_PARAMS_VER };
                eventParams.completionEvent = m_vpCompletionEvent[i];
                m_Nvenc.nvEncUnregisterAsyncEvent(m_HEncoder, &eventParams);
                CloseHandle(m_vpCompletionEvent[i]);
            }
        }
        m_vpCompletionEvent.clear();
    }
#pragma endregion
}
