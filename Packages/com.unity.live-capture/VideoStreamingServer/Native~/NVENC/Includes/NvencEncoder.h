#pragma once

#include <mutex>
#include <queue>
#include <list>

#include "nvEncodeAPI.h"
#include "d3d11.h"

#include "Unity/IUnityGraphics.h"
#include "NvencFrame.h"
#include "NvencEncoderSessionData.h"
#include "IGraphicsEncoderDevice.h"

#include "NvThread.h"

namespace NvencPlugin
{
    enum class ENvencSupport
    {
        Supported,
        NotSupportedOnPlatform,
        NoDriver,
        DriverVersionNotSupported
    };

    enum class ENvencStatus
    {
        NotInitialized,
        Success,
        DriverNotInstalled,
        DriverVersionDoesNotSupportAPI,
        APINotFound,
        EncoderInitializationFailed
    };

    struct EncodedFrameDataKey
    {
        int index;
        unsigned long long int timestamp;
        bool isKeyFrame;
    };

    class NvEncoder
    {
        using NvEncodeAPICreateInstance_Type = NVENCSTATUS(NVENCAPI*)(NV_ENCODE_API_FUNCTION_LIST*);
        using DataSequence = std::vector<uint8_t>;

        const int  k_MaxWidth = 3840;
        const int  k_MaxHeight = 2160;
        const int  k_MaxQueueLength = 8;

    public:
        NvEncoder(NV_ENC_DEVICE_TYPE deviceType,
                  const NvencEncoderSessionData& other,
                  IGraphicsEncoderDevice* device,
                  bool forceNv12);

        ~NvEncoder() = default;

        static ENvencSupport IsEncoderAvailable();

        // Initialization
        ENvencStatus InitEncoder();
        void         DestroyResources();

        // Update & Encode
        bool         UpdateEncoderSessionData(const NvencEncoderSessionData& other);
        void         EncodeFrame(void* frameSourceData, unsigned long long int timeStamp);

        // Get encoded frames
        bool          RemoveEncodedFrame();
        EncodedFrame* GetEncodedFrame();
        void          GetSequenceParams(DataSequence& spsSequence, DataSequence& ppsSequence);

        // Getters
        inline bool  IsInitialized() { return m_InitializationResult == ENvencStatus::Success; }

    private:
        // Initialize / destroy resources
        static HMODULE LoadModule();
        static bool    CheckDriverVersion(HMODULE module);
        ENvencStatus   LoadCodec();
        void           SetEncoderParameters();

        // Initialize encoding resources
        void                  MapResources(InputFrame& inputFrame);
        void                  InitEncoderResources();
        NV_ENC_REGISTERED_PTR RegisterResource(void* buffer, NV_ENC_BUFFER_FORMAT format);
        NV_ENC_OUTPUT_PTR     InitializeBitstreamBuffer();

        //Encoding frames
        void UpdateSettings();
        bool CopyBufferResources(int frameIndex, void* frameSourceData);
        void ProcessEncodedFrame(Frame& frame, unsigned long long int timeStamp, bool isKeyFrame);

        // Release Resources
        void UnloadModule();
        void ReleaseFrameInputBuffer(Frame& frame);
        void ReleaseEncoderResources();
        void ClearEncodedFrameQueue();

        // Encoded frame actions
        void AddEncodedFrame(Frame& frame, unsigned long long int timeStamp, bool isKeyFrame);

        // Async methods
        void InitializeAsyncResources();
        void DestroyAsyncResources();

        void* GetCompletionEvent(uint32_t eventIdx);
        Frame& GetBufferedFrame(int index);

        static void ProcessEncodedFrameAsyncSingle(NvEncoder* encoder);

    private:
        // Device specific
        IGraphicsEncoderDevice* m_Device;

        // Load Codec
        void* m_HModule;
        void* m_HEncoder;

        // Open an encode session
        NV_ENCODE_API_FUNCTION_LIST m_Nvenc;
        NV_ENC_INITIALIZE_PARAMS    m_NvEncInitializeParams;
        NV_ENC_DEVICE_TYPE          m_DeviceType;
        NV_ENC_CONFIG               m_NvEncConfig;

        // Encode processing
        ENvencStatus m_InitializationResult;
        bool         m_IsIdrFrame;

        // Frame infos
        NvencEncoderSessionData m_FrameData;
        uint64_t                m_FrameCount;
        uint64_t                m_GOPCount;
        bool                    m_ForceNV12;
        
        // Global resources. Note from NVIDIA doc:
        // "It is also recommended to allocate many input and output buffers
        // in order to avoid resource hazards and improve overall encoder throughput."
        ITexture2D* m_RenderTextures[k_BufferedFrameNum];
        Frame       m_BufferedFrames[k_BufferedFrameNum];

        std::queue<EncodedFrame>  m_FrameQueue;

        // Async members
        std::vector<void*> m_vpCompletionEvent;
        std::queue<EncodedFrameDataKey> m_BufferToRead;

        NvThread* m_Thread;
        NvSpinlock m_NvSpinlock;

        int32_t m_nEncoderBuffer = 0;
        bool m_IsAsync;
        
    };
}
