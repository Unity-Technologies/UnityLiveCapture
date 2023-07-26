#include "TargetConditionals.h"
#define ENABLE_TRACE 0
#if ENABLE_TRACE
#include <iostream>
#include <sstream>
#include <thread>
#endif

#include <mutex>
#include <queue>
#include <CoreVideo/CoreVideo.h>
#include <VideoToolbox/VideoToolbox.h>
#include <Metal/Metal.h>

#define ENABLE_HARDWARE_DECODING !TARGET_OS_IPHONE
#define ENABLE_ASYNCHRONOUS_DECODING 1
#define ENABLE_COLORSPACE_CONVERSION 1
#define ENABLE_DATA_TRACING 0
#if ENABLE_TRACE
#define TRACE_TIMESTAMP (std::chrono::duration_cast<std::chrono::microseconds>(std::chrono::time_point_cast<std::chrono::microseconds>(std::chrono::high_resolution_clock::now()).time_since_epoch()).count())
#define TRACE(msg) { std::stringstream os; os << TRACE_TIMESTAMP << " | " << msg; std::cout << os.str() << std::endl; }
#else
#define TRACE(msg) {}
#endif

extern "C" struct UnityTextureHandles
{
    void* texture;
    void* textureCbCr;
};

struct MetalFrameTextures
{
    CVMetalTextureRef texture;
    CVMetalTextureRef textureCbCr;

    MetalFrameTextures()
    : texture(0)
    , textureCbCr(0)
    {

    }

    void Release()
    {
        if (texture != nil)
            CFRelease(texture);
        if (textureCbCr != nil)
            CFRelease(textureCbCr);
    }
};

class H264Decoder
{
public:
    
    const int32_t _id;
    
    H264Decoder()
    : _id(++m_DecoderId)
    {
        TRACE("H264Decoder::H264Decoder " << _id);
        id<MTLDevice> metalDevice = MTLCreateSystemDefaultDevice();
        CVMetalTextureCacheCreate(NULL, NULL, metalDevice, NULL, &m_TextureCache);
        TRACE("H264Decoder::H264Decoder texturecache" << m_TextureCache);
    }
    
    ~H264Decoder()
    {
        TRACE("H264Decoder::~H264Decoder " << _id);
        Stop();

        while (m_Queue.size() > 0)
        {
            MetalFrameTextures textures = m_Queue.front();
            textures.Release();
            m_Queue.pop();
        }

        if (m_TextureCache != nil)
            CFRelease(m_TextureCache);
    }
    
    void Stop()
    {
        TRACE("H264Decoder::Stop << " << _id);
        {
            m_Stopped = true;
        }
        if (m_Session == nil)
            return;
        VTDecompressionSessionWaitForAsynchronousFrames(m_Session);
        VTDecompressionSessionInvalidate(m_Session);
        CFRelease(m_Session);
        m_Session = nil;
        CFRelease(m_Format);
        m_Format = nil;
        
        m_CurrentTextures.texture = nil;
        m_CurrentTextures.textureCbCr = nil;
        
        TRACE("H264Decoder::Stop done");
    }
    
    int32_t Configure(void* spsNalu, int32_t spsNaluLength, void* ppsNalu, int32_t ppsNaluLength)
    {
        TRACE("H264Decoder::Configure " << _id << " sps: " << spsNalu << "/sz=" << spsNaluLength << " pps: " << ppsNalu << "/sz=" << ppsNaluLength << " thread: " << pthread_self());
        if (spsNalu == nullptr || spsNaluLength <= 0 || ppsNalu == nullptr || ppsNaluLength <= 0)
            return -1;
        
        if (m_Session != nil)
            Stop();
        
#if ENABLE_DATA_TRACING
        TRACE("SPS:");
        for (int i = 0; i < spsDataLength; ++i)
            TRACE(std::dec << i << ": " << std::hex << (unsigned)(((uint8_t*)spsNalu)[i]));
        std::cout << "PPS:");
        for (int i = 0; i < ppsDataLength; ++i)
            TRACE(std::dec << i << ": " << std::hex << (unsigned)(((uint8_t*)ppsNalu)[i]));
        TRACE(std::dec);
#endif
        CMVideoFormatDescriptionRef format = nil;
        const uint8_t* parameterSetPointers[] = {
            (uint8_t*)spsNalu,
            (uint8_t*)ppsNalu
        };
        
        size_t parameterSetSizes[] =
        {
            (size_t)spsNaluLength,
            (size_t)ppsNaluLength
        };
        
        const int nalUnitLength = 4;
        OSStatus status = CMVideoFormatDescriptionCreateFromH264ParameterSets(kCFAllocatorDefault,
            2, parameterSetPointers, parameterSetSizes, nalUnitLength, &format);
        if (status != 0)
        {
            TRACE("H264Decoder::Configure error getting format description: " << status);
            return status;
        }
        
        CMVideoDimensions dim = CMVideoFormatDescriptionGetDimensions(format);
        TRACE("H264Decoder::Configure dimensions: " << dim.width << " x " << dim.height);
        
        CGSize sz = CMVideoFormatDescriptionGetPresentationDimensions(format, false, false);
        TRACE("H264Decoder::Configure presentation dimensions: " << sz.width << " x " << sz.height);

        CFMutableDictionaryRef decoderAttr = nil;
#if ENABLE_HARDWARE_DECODING
        decoderAttr = CFDictionaryCreateMutable(kCFAllocatorDefault, 1,
            &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
        CFDictionarySetValue(decoderAttr,
                             kVTVideoDecoderSpecification_EnableHardwareAcceleratedVideoDecoder,
                             kCFBooleanTrue);
#endif
        
        int32_t w = sz.width;
        int32_t h = sz.height;
        CFNumberRef width = CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &w);
        CFNumberRef height = CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &h);

        CFMutableDictionaryRef imageAttr = CFDictionaryCreateMutable(kCFAllocatorDefault, 3,
           &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);

#if ENABLE_COLORSPACE_CONVERSION
        int32_t pixType = kCVPixelFormatType_32BGRA;
#else
        int32_t pixType = kCVPixelFormatType_420YpCbCr8BiPlanarFullRange; //NV12
#endif
        CFNumberRef formatType = CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &pixType);
        CFDictionarySetValue(imageAttr, kCVPixelBufferPixelFormatTypeKey, formatType);
        CFDictionarySetValue(imageAttr, kCVPixelBufferMetalCompatibilityKey, kCFBooleanTrue);
        CFDictionarySetValue(imageAttr, kCVPixelBufferWidthKey, width);
        CFDictionarySetValue(imageAttr, kCVPixelBufferHeightKey, height);
        CFRelease(width);
        CFRelease(height);
        
        VTDecompressionOutputCallbackRecord outputCallback = {
            OutputCallback,
            this
        };

        TRACE("H264Decoder:Configure creating decompression session.");
        status = VTDecompressionSessionCreate(kCFAllocatorDefault, format,
            decoderAttr, imageAttr, &outputCallback, &m_Session);

        CFRelease(imageAttr);

        if (decoderAttr != nil)
            CFRelease(decoderAttr);
        
        if (status != 0)
        {
            TRACE("H264Decoder::Configure error creating decompression session: " << status);
            CFRelease(format);
            return status;
        }
        
        if (m_Format != nil)
            CFRelease(m_Format);
        
        m_Format = format;
        m_Width = w;
        m_Height = h;
        m_Stopped = false;

#if ENABLE_TRACING
        CFBooleanRef usingHardware = nil;
        status = VTSessionCopyProperty(
            m_Session, kVTDecompressionPropertyKey_UsingHardwareAcceleratedVideoDecoder,
            kCFAllocatorDefault, &usingHardware);
        
        if (status != 0)
            TRACE("H264Decoder::Configure error detecting hardware acceleration: " << status)
        else
        {
            TRACE("H264Decoder::Configure using hardware acceleration: " << (bool)CFBooleanGetValue(usingHardware));
            CFRelease(usingHardware);
        }
#endif
        
        TRACE("H264Decoder::Configure done.");
        return 0;
    }
    
    int32_t GetWidth() const
    {
        return m_Width;
    }

    int32_t GetHeight() const
    {
        return m_Height;
    }
    
    int32_t Decode(uint8_t* nalu, int32_t naluLength, int64_t presentationTimestampTicks,
        int32_t tickRate, bool doNotOutputFrame)
    {
        TRACE("H264Decoder::Decode " << _id << ": " << naluLength << "b, ts: " <<
              presentationTimestampTicks << " @ " << tickRate << " Hz, t: " <<
              (double)presentationTimestampTicks/(double)tickRate);
#if ENABLE_DATA_TRACING
        for (int i = 0; i < std::min(naluLength, 20); ++i)
            TRACE(std::dec << i << ": " << std::hex << (int)(((uint8_t*)nalu)[i]));
#endif

        std::vector<int> naluStarts;
        for (int i = 0; i < naluLength - sizeof(uint32_t); ++i)
        {
            if (nalu[i] == 0 && nalu[i + 1] == 0 && nalu[i + 2] == 0 && nalu[ i + 3] == 1)
                naluStarts.push_back(i);
        }

        TRACE("H264Decoder::Decode found " << naluStarts.size() << " NAL units in input buffer.");

        const size_t blockSize = naluLength;
        CMBlockBufferRef blockBuf = nil;
        OSStatus status = CMBlockBufferCreateWithMemoryBlock(kCFAllocatorDefault, NULL,
            blockSize, kCFAllocatorDefault, NULL, 0, blockSize, 0, &blockBuf);
        if (status != 0)
        {
            TRACE("H264Decoder::Decode failed to create memory block: " << status);
            return status;
        }

        // Make sure that the memory is actually allocated.
        // CMBlockBufferReplaceDataBytes() is documented to do this, but prints a
        // message each time starting in Mac OS X 10.10.
        status = CMBlockBufferAssureBlockMemory(blockBuf);
        if (status)
        {
            TRACE("H264Decoder::Decode failed to create memory block");
            return status;
        }

        status = CMBlockBufferReplaceDataBytes(nalu, blockBuf, 0, naluLength);
        if (status != 0) {
            TRACE("H264Decoder::Decode failed in CMBlockBufferReplaceDataBytes (data): " << status);
            CFRelease(blockBuf);
            return status;
        }

        for (int i = 0; i < naluStarts.size(); ++i)
        {
            const int naluStart = naluStarts[i];
            const int naluEnd = i == (naluStarts.size() - 1) ? naluLength : naluStarts[i + 1];
            const uint32_t naluSize = naluEnd - naluStart - sizeof(uint32_t);

            // VideoToolbox wants AVCC format for NALUs, and in AVCC, the nalu length is expected to be
            // a 32-bits integer in network endianness.
            const uint32_t avccLength = CFSwapInt32HostToBig(naluSize);

            status = CMBlockBufferReplaceDataBytes(&avccLength, blockBuf, naluStart, sizeof(avccLength));
            if (status != 0) {
                TRACE("H264Decoder::Decode failed in CMBlockBufferReplaceDataBytes (sz): " << status);
                CFRelease(blockBuf);
                return status;
            }
        }

        CMSampleTimingInfo timingInfo;
        timingInfo.decodeTimeStamp = kCMTimeInvalid;
        timingInfo.duration = kCMTimeInvalid;
        timingInfo.presentationTimeStamp = CMTimeMake(presentationTimestampTicks, tickRate);
              
        CMSampleBufferRef sampleBuf = nil;
        status = CMSampleBufferCreateReady(
            kCFAllocatorDefault, blockBuf, m_Format, 1, 1, &timingInfo, 0, nullptr, &sampleBuf);
        CFRelease(blockBuf);
        if (status != 0) {
            TRACE("H264Decoder::Decode failed in CMSampleBufferCreateReady: " << status);
            return status;
        }

        VTDecodeFrameFlags decodeFlags =
#if ENABLE_ASYNCHRONOUS_DECODING
            kVTDecodeFrame_EnableAsynchronousDecompression
#else
            kVTDecodeFrame_1xRealTimePlayback
#endif
        ;

        if (doNotOutputFrame)
            decodeFlags |= kVTDecodeFrame_DoNotOutputFrame;

        status = VTDecompressionSessionDecodeFrame(m_Session, sampleBuf, decodeFlags, NULL, NULL);
        CFRelease(sampleBuf);
        if (status != 0)
        {
            TRACE("H264Decoder::Decode failed in VTDecompressionSessonDecodeFrame: " << status);
            return status;
        }

        TRACE("H264Decoder::Decode done.");
        return naluLength;
    }

    bool IsFrameReady() const
    {
#if ENABLE_COLORSPACE_CONVERSION
        return m_CurrentTextures.texture != nil;
#else
        return m_CurrentTextures.texture != nil && m_CurrentTextures.textureCbCr != nil;
#endif
    }

    UnityTextureHandles GetVideoTextureHandles()
    {
        UnityTextureHandles handles;
 
        handles.texture = (__bridge void*)CVMetalTextureGetTexture(m_CurrentTextures.texture);
#ifndef ENABLE_COLORSPACE_CONVERSION
        handles.textureCbCr = (__bridge void*)CVMetalTextureGetTexture(m_CurrentTextures.textureCbCr);
#endif
        return handles;
    }
    
private:

#if ENABLE_COLORSPACE_CONVERSION
    void SetCurrentImageBuffer(CVImageBufferRef imageBuffer)
    {
        CVMetalTextureRef texture = nil;

        // texture
        {
            const size_t width = CVPixelBufferGetWidthOfPlane(imageBuffer, 0);
            const size_t height = CVPixelBufferGetHeightOfPlane(imageBuffer, 0);
            MTLPixelFormat pixelFormat = MTLPixelFormatBGRA8Unorm_sRGB;
            
            CVReturn status = CVMetalTextureCacheCreateTextureFromImage(NULL, m_TextureCache, imageBuffer, NULL, pixelFormat, width, height, 0, &texture);
        }

        if (texture != nil)
        {
            dispatch_async(dispatch_get_main_queue(), ^{
                // always assign the textures atomic
                if (m_Queue.size() >= 5)
                {
                    MetalFrameTextures textures = m_Queue.front();
                    m_Queue.pop();
                    textures.Release();
                }

                m_CurrentTextures.texture = texture;

                if (texture != nil)
                    m_Queue.push(m_CurrentTextures);
            });
        }
    }
#else
    void SetCurrentImageBuffer(CVImageBufferRef imageBuffer)
    {
        CVMetalTextureRef textureY = nil;
        CVMetalTextureRef textureCbCr = nil;

        // textureY
        {
            const size_t width = CVPixelBufferGetWidthOfPlane(imageBuffer, 0);
            const size_t height = CVPixelBufferGetHeightOfPlane(imageBuffer, 0);
            MTLPixelFormat pixelFormat = MTLPixelFormatR8Unorm;
            
            CVReturn status = CVMetalTextureCacheCreateTextureFromImage(NULL, m_TextureCache, imageBuffer, NULL, pixelFormat, width, height, 0, &textureY);
        }

        // textureCbCr
        {
            const size_t width = CVPixelBufferGetWidthOfPlane(imageBuffer, 1);
            const size_t height = CVPixelBufferGetHeightOfPlane(imageBuffer, 1);
            MTLPixelFormat pixelFormat = MTLPixelFormatRG8Unorm;

            CVReturn status = CVMetalTextureCacheCreateTextureFromImage(NULL, m_TextureCache, imageBuffer, NULL, pixelFormat, width, height, 1, &textureCbCr);
        }

        if (textureY != nil && textureCbCr != nil)
        {
            dispatch_async(dispatch_get_main_queue(), ^{
                // always assign the textures atomic
                if (m_Queue.size() >= 5)
                {
                    MetalFrameTextures textures = m_Queue.front();
                    m_Queue.pop();
                    textures.Release();
                }

                m_CurrentTextures.texture = texture;
                m_CurrentTextures.textureCbCr = textureCbCr;
                
                if (texture != nil)
                    m_Queue.push(m_CurrentTextures);
            });
        }
    }
#endif
    
    static void OutputCallback(
        void* h264Decoder, void* sourceFrameRefCon, OSStatus status,
        VTDecodeInfoFlags infoFlags, CVImageBufferRef imageBuffer, CMTime presentationTimeStamp,
        CMTime presentationDuration)
    {
        auto decoder = static_cast<H264Decoder*>(h264Decoder);
        
        TRACE("H264Decoder::OutputCallback " << decoder->_id << "\n" <<
        "   Status: " << status << "\n"
        "   Presentation timestamp: " << presentationTimeStamp.value << " @ " <<
              presentationTimeStamp.timescale << " Hz, dur: " <<
              CMTimeGetSeconds(presentationDuration) << "s\n" <<
        "   Asynchronous: " <<
              ((infoFlags & kVTDecodeInfo_Asynchronous) ? "yes" : "no ") << "\n" <<
        "   Frame dropped: " <<
              ((infoFlags & kVTDecodeInfo_FrameDropped) ? "yes" : "no ") << "\n" <<
        "   Image modifiable: " <<
              ((infoFlags & kVTDecodeInfo_ImageBufferModifiable) ? "yes" : "no ") << "\n"
        "   Thread id: " << std::this_thread::get_id());

        if (status != 0)
            return;
#if ENABLE_TRACE
        CGRect rect = CVImageBufferGetCleanRect(imageBuffer);
        CGSize displaySize = CVImageBufferGetDisplaySize(imageBuffer);
        CGSize encodedSize = CVImageBufferGetEncodedSize(imageBuffer);
        size_t stride = CVPixelBufferGetBytesPerRow(imageBuffer);
        size_t width = CVPixelBufferGetWidth(imageBuffer);
        size_t height = CVPixelBufferGetHeight(imageBuffer);
        const char* flipped = CVImageBufferIsFlipped(imageBuffer) ? "yes" : "no";
        const char* planar = CVPixelBufferIsPlanar(imageBuffer) ? "yes" : "no";
        size_t planeCount = CVPixelBufferGetPlaneCount(imageBuffer);
        size_t dataSize = CVPixelBufferGetDataSize(imageBuffer);
        OSType dataType = CVPixelBufferGetPixelFormatType(imageBuffer);
        
        TRACE("Image\n   Resolution: " << rect.size.width << " x " << rect.size.height << "\n" <<
        "   Clean rect: [" << rect.origin.x << ", " << rect.origin.y << "], " <<
              rect.size.width << " x " << rect.size.height << "\n"
        "   Pixel rect: " << width << " x " << height << "\n" <<
        "   Display size: " << displaySize.width << " x " << displaySize.height << "\n" <<
        "   Encoded size: " << encodedSize.width << " x " << encodedSize.height << "\n" <<
        "   Stride: " << stride << "\n" <<
        "   Flipped: " << flipped << "\n" <<
        "   Planar: " << planar << "\n" <<
        "   Plane count: " << planeCount << "\n" <<
        "   Data size: " << dataSize << "\n" <<
        "   Data type: " << dataType << "\n");
#endif

        decoder->SetCurrentImageBuffer(imageBuffer);
        
        TRACE("H264Decoder::OutputCallback " << decoder->_id << " done.");
    }

    static std::atomic_int32_t   m_DecoderId;
              
    VTDecompressionSessionRef    m_Session = nil;
    CMVideoFormatDescriptionRef  m_Format = nil;
    int32_t                      m_Width = 0;
    int32_t                      m_Height = 0;
    volatile bool                m_Stopped = false;
    CVMetalTextureCacheRef       m_TextureCache;
    std::queue<MetalFrameTextures> m_Queue;
    MetalFrameTextures           m_CurrentTextures;

};

std::atomic_int32_t H264Decoder::m_DecoderId;

#if ENABLE_TRACE
static std::once_flag InitLogOnce;
void InitLog()
{
    char* home = getenv("HOME");
    if (home == nullptr)
        return;
    std::string logPath(home);
    logPath.append("/H264Decoder.log");
    freopen(logPath.c_str(), "w", stdout);
}
#endif

extern "C"
{
H264Decoder* Create()
{
#if ENABLE_TRACE
    std::call_once(InitLogOnce, InitLog);
#endif
    return new H264Decoder();
}

int32_t Destroy(H264Decoder* decoder)
{
    if (decoder == nullptr)
        return -1;
    delete decoder;
    return 0;
}

int32_t Stop(H264Decoder* decoder)
{
    if (decoder == nullptr)
        return -1;
    decoder->Stop();
    return 0;
}

int32_t Configure(
    H264Decoder* decoder, uint8_t* spsNalu, int32_t spsNaluLength, uint8_t* ppsNalu, int32_t ppsNaluLength)
{
    if (decoder == nullptr)
        return -1;
    return decoder->Configure(spsNalu, spsNaluLength, ppsNalu, ppsNaluLength);
}

int32_t GetWidth(H264Decoder* decoder)
{
    if (decoder == nullptr)
        return -1;
    return decoder->GetWidth();
}

int32_t GetHeight(H264Decoder* decoder)
{
    if (decoder == nullptr)
        return -1;
    return decoder->GetHeight();
}

int32_t Decode(H264Decoder* decoder, uint8_t* nalu, int32_t naluLength,
    int64_t presentationTimestampTicks, int32_t tickRate, bool doNotOutputFrame)
{
    if (decoder == nullptr)
        return -1;
    return decoder->Decode(nalu, naluLength, presentationTimestampTicks, tickRate, doNotOutputFrame);
}

extern "C" UnityTextureHandles GetVideoTextureHandles(H264Decoder* decoder)
{
    if (decoder != nullptr)
        return decoder->GetVideoTextureHandles();

    UnityTextureHandles handles;
    handles.texture = nil;
    handles.textureCbCr = nil;
    return handles;
}

bool IsFrameReady(H264Decoder* decoder)
{
    if (decoder == nullptr)
        return false;
    return decoder->IsFrameReady();
}
}
