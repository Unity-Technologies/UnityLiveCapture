#pragma once

#ifdef DEBUG_LOG
#include <iostream>
#include <sstream>
#include <thread>
#include <chrono>
#endif

#include <vector>
#include <queue>

#import <CoreMedia/CoreMedia.h>
#import <CoreVideo/CoreVideo.h>
#import <VideoToolbox/VideoToolbox.h>
#import <AVFoundation/AVFoundation.h>
#import <Metal/Metal.h>

#include "PluginUtils.hpp"
#include "MacOSEncoderSessionDataPlugin.hpp"

namespace MacOsEncodingPlugin
{

enum class MacOSEncoderSupport
{
    Supported,
    NotSupportedOnPlatform,
    NoDriver,
    DriverVersionNotSupported
};

enum class MacOSEncoderStatus
{
    NotInitialized,
    Success,
    DriverNotInstalled,
    DriverVersionDoesNotSupportAPI,
    APINotFound,
    EncoderInitializationFailed
};

class MetalGraphicsEncoderDevice;

class H264Encoder
{
    
public: // Methods
    
    H264Encoder(const MacOSEncoderSessionData& frameData, MetalGraphicsEncoderDevice* const device);
    ~H264Encoder();
    
    void Initialize(bool useSRGB, bool allocateBuffers = true);
    void Dispose();
    bool EncodeFrame(void* frameSource, unsigned long long int timestamp);
    
    bool RemoveEncodedFrame();
    EncodedFrame*  GetEncodedFrame();
    
    inline bool IsInitialized() { return m_InitializationResult == MacOSEncoderStatus::Success; }
    inline std::queue<EncodedFrame>& GetFrameQueue() { return m_FrameQueue; }
    inline int GetMaxQueueLength() const { return k_MaxQueueLength; }
    inline unsigned long long int GetLatestTimestamp() { return m_LatestTimestamp; }
    
private: // Members

    static const NSInteger k_BufferedFrameNumbers = 3;
    static const int k_MaxQueueLength = 8;
    
    MetalGraphicsEncoderDevice* m_GraphicDevice;
    VTCompressionSessionRef     m_EncodingSession;
    bool                        m_SessionCreated;
    bool                        m_UseSRGB;
    
    MacOSEncoderStatus          m_InitializationResult;
    MacOSEncoderSessionData     m_FrameData;
    uint64                      m_FrameCount;
    
    CVPixelBufferRef            m_PixelBuffers[k_BufferedFrameNumbers];
    id<MTLTexture>              m_RenderTextures[k_BufferedFrameNumbers];
    std::queue<EncodedFrame>    m_FrameQueue;
    unsigned long long int      m_LatestTimestamp;
    
private: // Methods
    
    bool createSession();
    void endSession();
    
    bool allocateBuffers();
    void releaseBuffers();
    
    bool copyBuffer(void* frameSource, int frameIndex);
};

}
