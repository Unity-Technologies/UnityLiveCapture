#include "H264Encoder.hpp"
#include "MetalGraphicsEncoderDevice.hpp"

#define ENABLE_COLORSPACE_CONVERSION 0

namespace MacOsEncodingPlugin
{
    const NSInteger H264Encoder::k_BufferedFrameNumbers;
    const int H264Encoder::k_MaxQueueLength;

    H264Encoder::H264Encoder(const MacOSEncoderSessionData& frameData,
                             MetalGraphicsEncoderDevice* const device)
        : m_GraphicDevice(device)
        , m_EncodingSession(nullptr)
        , m_SessionCreated(false)
        , m_InitializationResult(MacOSEncoderStatus::NotInitialized)
        , m_FrameData(frameData)
        , m_FrameCount(0)
    {
        WriteFileDebug("Info: [H264Encoder()] - Constructor called.\n");
    }
    
    H264Encoder::~H264Encoder()
    {
        Dispose();
    }
    
    void H264Encoder::Initialize(bool useSRGB, bool buffersAllocation)
    {
        m_UseSRGB = useSRGB;
        
        auto sessionCreated = createSession();
        auto buffersCreated = (buffersAllocation) ? allocateBuffers() : true;
        
        if (sessionCreated && buffersCreated)
        {
            m_InitializationResult = MacOSEncoderStatus::Success;
            m_SessionCreated = true;
        }
        else
        {
            m_InitializationResult = MacOSEncoderStatus::EncoderInitializationFailed;
            m_SessionCreated = false;
        }
    }

    void H264Encoder::Dispose()
    {
        WriteFileDebug("Info: ~[H264Encoder()] - Dispose.\n");

        if (m_EncodingSession == nullptr)
            return;
        
        WriteFileDebug("Info: ~[H264Encoder()] - encoding session is valid.\n");
    
        endSession();
        m_EncodingSession = nullptr;
        m_SessionCreated = false;
    }

    void postEncodeParser(H264Encoder* encoder, CMSampleBufferRef sampleBuffer)
    {
        EncodedFrame encodedFrameClass;
        
        CFArrayRef attachments = CMSampleBufferGetSampleAttachmentsArray(sampleBuffer, false);
        
        // Check if the actual frame is a KeyFrame / IDR frame.
        encodedFrameClass.isKeyFrame = false;
        if(attachments != nullptr && CFArrayGetCount(attachments))
        {
            CFDictionaryRef attachment = static_cast<CFDictionaryRef>(CFArrayGetValueAtIndex(attachments, 0));
            encodedFrameClass.isKeyFrame = !CFDictionaryContainsKey(attachment, kCMSampleAttachmentKey_NotSync);
        }
        
        CMVideoFormatDescriptionRef description = CMSampleBufferGetFormatDescription(sampleBuffer);
        
        int nalu_header_size = 0;
        size_t param_set_count = 0;
        OSStatus status = CMVideoFormatDescriptionGetH264ParameterSetAtIndex(description,
                                                                             0,
                                                                             nullptr,
                                                                             nullptr,
                                                                             &param_set_count,
                                                                             &nalu_header_size);
        if (status != noErr)
        {
            WriteFileDebug("Error: [postEncodeParser] - H264ParameterSetAtIndex failed.\n");
            return;
        }
        
        if (encodedFrameClass.isKeyFrame)
        {
            // Variables for PPS/SPS
            size_t spsSize = 0;
            size_t ppsSize = 0;
            
            size_t spsCnt = 0;
            size_t ppsCnt = 0;
            
            const uint8_t *sps = nullptr;
            const uint8_t *pps = nullptr;
            
            // Get SPS
            OSStatus status = CMVideoFormatDescriptionGetH264ParameterSetAtIndex(description,
                                                                                 0,
                                                                                 &sps,
                                                                                 &spsSize,
                                                                                 &spsCnt,
                                                                                 0);
            if (status != noErr)
            {
                WriteFileDebug("Error: [postEncodeParser] - Get SPS failed.\n");
                return;
            }
            
            // Get PPS
            status = CMVideoFormatDescriptionGetH264ParameterSetAtIndex(description,
                                                                        1,
                                                                        &pps,
                                                                        &ppsSize,
                                                                        &ppsCnt,
                                                                        0);
            if (status != noErr)
            {
                WriteFileDebug("Error: [postEncodeParser] - Get PPS failed.\n");
                return;
            }
            
            encodedFrameClass.spsSequence.insert(encodedFrameClass.spsSequence.end(),
                                                 &sps[0],
                                                 &sps[spsSize]);
            
            encodedFrameClass.ppsSequence.insert(encodedFrameClass.ppsSequence.end(),
                                                 &pps[0],
                                                 &pps[ppsSize]);
        }
        
        CMBlockBufferRef block_buffer = CMSampleBufferGetDataBuffer(sampleBuffer);

        if (block_buffer == nullptr)
        {
            WriteFileDebug("Error: [postEncodeParser] - CMSampleBufferGetDataBuffer failed.\n");
            return;
        }
        
        CMBlockBufferRef contiguous_buffer = nullptr;
        
        // Make sure block buffer is contiguous.
        if (!CMBlockBufferIsRangeContiguous(block_buffer, 0, 0))
        {
            WriteFileDebug("Info: [postEncodeParser] - Create contiguous buffer.\n");
            status = CMBlockBufferCreateContiguous(nullptr,
                                                   block_buffer,
                                                   nullptr,
                                                   nullptr,
                                                   0,
                                                   0,
                                                   0,
                                                   &contiguous_buffer);
            if (status != noErr)
            {
                WriteFileDebug("Error: [postEncodeParser] - Buffer is not contiguous.\n");
                return;
            }
        }
        else
        {
            contiguous_buffer = block_buffer;
            CFRetain(contiguous_buffer);
            block_buffer = nullptr;
        }
        
        // Now copy the actual data.
        char* data_ptr = nullptr;
        size_t block_buffer_size = CMBlockBufferGetDataLength(contiguous_buffer);
        status = CMBlockBufferGetDataPointer(contiguous_buffer,
                                             0,
                                             nullptr,
                                             nullptr,
                                             &data_ptr);
        if (status != noErr)
        {
            WriteFileDebug("Error: [postEncodeParser] - Failed to get block buffer data.\n");
            CFRelease(contiguous_buffer);
            return;
        }
        
        const char kAnnexBHeaderBytes[4] = {0, 0, 0, 1};
        size_t bytes_remaining = block_buffer_size;
        
        while (bytes_remaining > 0)
        {
            uint32_t* uint32_data_ptr = reinterpret_cast<uint32*>(data_ptr);
            uint32_t packet_size = CFSwapInt32BigToHost(*uint32_data_ptr);

            encodedFrameClass.imageData.insert(encodedFrameClass.imageData.end(),
                                               &kAnnexBHeaderBytes[0],
                                               &kAnnexBHeaderBytes[4]);
              
            encodedFrameClass.imageData.insert(encodedFrameClass.imageData.end(),
                                               data_ptr + nalu_header_size,
                                               data_ptr + nalu_header_size + packet_size);
    
            size_t bytes_written = packet_size + nalu_header_size;
              
            bytes_remaining -= bytes_written;
            data_ptr += bytes_written;
        }
        
        if (bytes_remaining > 0)
        {
            WriteFileDebug("Error: [postEncodeParser] - bytes_remaining > 0.\n");
        }
        
        encodedFrameClass.timestamp = encoder->GetLatestTimestamp();
        auto& frameQueue = encoder->GetFrameQueue();
        
        if (frameQueue.size() < encoder->GetMaxQueueLength())
        {
            frameQueue.push(std::move(encodedFrameClass));
        }
        else
        {
            WriteFileDebug("Warning: [postEncodeParser] - too much encoded frames in the queue.\n");
            
            frameQueue.pop();
            frameQueue.push(std::move(encodedFrameClass));
        }
    }

    void postEncodeCallback(void *outputCallbackRefCon,
                            void *sourceFrameRefCon,
                            OSStatus status,
                            VTEncodeInfoFlags infoFlags,
                            CMSampleBufferRef sampleBuffer )
    {
        if (status != noErr)
        {
            WriteFileDebug("Error: [postEncodeCallback] - Frame received is invalid.\n");
            return;
        }
        
        if (!CMSampleBufferDataIsReady(sampleBuffer))
        {
            WriteFileDebug("Error: [postEncodeCallback] - Frame received is not ready.\n");
            return;
        }
        
        H264Encoder* encoder = reinterpret_cast<H264Encoder*>(outputCallbackRefCon);
        
        if(encoder == nullptr)
        {
            WriteFileDebug("Error: [postEncodeCallback] - Params received are invalid.\n");
            return;
        }
        
        postEncodeParser(encoder, sampleBuffer);
    }

    namespace internal
    {
        // Convenience function for creating a dictionary.
        inline CFDictionaryRef CreateCFDictionary(CFTypeRef* keys,
                                                  CFTypeRef* values,
                                                  size_t size)
        {
            return CFDictionaryCreate(kCFAllocatorDefault, keys, values, size,
                                      &kCFTypeDictionaryKeyCallBacks,
                                      &kCFTypeDictionaryValueCallBacks);
        }
    }

    bool H264Encoder::createSession()
    {
          const size_t attributes_size = 3;
          CFTypeRef keys[attributes_size] =
          {
             kCVPixelBufferMetalCompatibilityKey,
             kCVPixelBufferIOSurfacePropertiesKey,
             kCVPixelBufferPixelFormatTypeKey
          };
        
          CFDictionaryRef io_surface_value = internal::CreateCFDictionary(nullptr, nullptr, 0);
        
#if ENABLE_COLORSPACE_CONVERSION
          int32_t pixType = kCVPixelFormatType_420YpCbCr8BiPlanarFullRange; //NV12
#else
          int32_t pixType = kCVPixelFormatType_32BGRA;
#endif
        
          CFNumberRef pixel_format = CFNumberCreate(nullptr, kCFNumberLongType, &pixType);
        
          CFTypeRef values[attributes_size] =
          {
            kCFBooleanTrue,
            io_surface_value,
            pixel_format
          };
        
          CFDictionaryRef source_attributes = internal::CreateCFDictionary(keys, values, attributes_size);
        
          if (io_surface_value)
          {
            CFRelease(io_surface_value);
            io_surface_value = nullptr;
          }
        
          if (pixel_format)
          {
            CFRelease(pixel_format);
            pixel_format = nullptr;
          }

        OSStatus status = VTCompressionSessionCreate(NULL,
                                                     m_FrameData.width,
                                                     m_FrameData.height,
                                                     kCMVideoCodecType_H264,
                                                     nullptr,//encoderSpecifications,
                                                     source_attributes,//imageAttr,
                                                     NULL,
                                                     postEncodeCallback,
                                                     (__bridge void*)(this),
                                                     &m_EncodingSession);
         
        if (status != 0)
        {
            WriteFileDebug("Error: [createSession] - VTCompressionSessionCreate session creation failed.\n");
            return false;
        }
        
        NSNumber *frameRate = [NSNumber numberWithInt:m_FrameData.frameRate];
        NSNumber *bitRate = [NSNumber numberWithInt:m_FrameData.bitRate];
        NSNumber *gopSize = [NSNumber numberWithInt:(m_FrameData.gopSize)];

        // Set the properties
        VTSessionSetProperty(m_EncodingSession,
                             kVTCompressionPropertyKey_RealTime,
                             kCFBooleanTrue);
        
        VTSessionSetProperty(m_EncodingSession,
                             kVTCompressionPropertyKey_AllowFrameReordering,
                             kCFBooleanFalse);
        
        VTSessionSetProperty(m_EncodingSession,
                             kVTVideoEncoderSpecification_EnableHardwareAcceleratedVideoEncoder,
                             kCFBooleanTrue);
        
        VTSessionSetProperty(m_EncodingSession,
                             kVTCompressionPropertyKey_MaxKeyFrameInterval,
                             (__bridge CFTypeRef _Nonnull)(gopSize));
        
        VTSessionSetProperty(m_EncodingSession,
                             kVTCompressionPropertyKey_ProfileLevel,
                             kVTProfileLevel_H264_Baseline_AutoLevel);
                
        VTSessionSetProperty(m_EncodingSession,
                             kVTCompressionPropertyKey_ExpectedFrameRate,
                             (__bridge CFTypeRef _Nonnull)(frameRate));
        
        VTSessionSetProperty(m_EncodingSession,
                             kVTCompressionPropertyKey_AverageBitRate,
                             (__bridge CFTypeRef _Nonnull)(bitRate));
        
        // Tell the encoder to start encoding
        status = VTCompressionSessionPrepareToEncodeFrames(m_EncodingSession);
        
        if (status != 0)
        {
            WriteFileDebug("Error: [createSession] - VTCompressionSessionPrepareToEncodeFrames session creation failed.\n");
            return false;
        }
        
        WriteFileDebug("Success: [createSession] - Session creation succeeded.\n");
        
        return true;
    }

    void H264Encoder::endSession()
    {
        VTCompressionSessionCompleteFrames(m_EncodingSession, kCMTimeInvalid);
        VTCompressionSessionInvalidate(m_EncodingSession);
        
        CFRelease(m_EncodingSession);
        m_EncodingSession = NULL;
        
        releaseBuffers();
    }

    bool H264Encoder::allocateBuffers()
    {
        CVPixelBufferPoolRef pixelBufferPool;
        pixelBufferPool = VTCompressionSessionGetPixelBufferPool(m_EncodingSession);
        
        for(NSInteger i = 0; i < k_BufferedFrameNumbers; i++)
        {
            CVReturn result = CVPixelBufferPoolCreatePixelBuffer(NULL, pixelBufferPool, &m_PixelBuffers[i]);
            if (result != kCVReturnSuccess)
            {
                WriteFileDebug("Error: [allocateBuffers] - CVPixelBufferPoolCreatePixelBuffer failed.\n");
                return false;
            }
            
            id<MTLDevice> device_ = (__bridge id<MTLDevice>)m_GraphicDevice->GetEncodeDevicePtr();

            CVMetalTextureCacheRef textureCache;
            result = CVMetalTextureCacheCreate(kCFAllocatorDefault, nil, device_, nil, &textureCache);
            if(result != kCVReturnSuccess)
            {
                WriteFileDebug("Error: [allocateBuffers] - CVMetalTextureCacheCreate failed.\n");
                return false;
            }

            CVMetalTextureRef imageTexture;
            auto width = m_FrameData.width;
            auto height = m_FrameData.height;
            auto format = m_UseSRGB ? MTLPixelFormatBGRA8Unorm_sRGB : MTLPixelFormatBGRA8Unorm;
            
            result = CVMetalTextureCacheCreateTextureFromImage(kCFAllocatorDefault,
                                                               textureCache,
                                                               m_PixelBuffers[i],
                                                               nullptr,
                                                               format,
                                                               width,
                                                               height,
                                                               0,
                                                               &imageTexture);
            if (result != kCVReturnSuccess)
            {
                WriteFileDebug("Error: [allocateBuffers] - CVMetalTextureCacheCreateTextureFromImage failed.\n");
                return false;
            }
            
            m_RenderTextures[i] = CVMetalTextureGetTexture(imageTexture);
        }
        
        WriteFileDebug("Success: [allocateBuffers] - Buffers are allocated.\n");
        return true;
    }

    void H264Encoder::releaseBuffers()
    {
        if (m_InitializationResult != MacOSEncoderStatus::Success)
            return;
        
        for (int i = 0; i < k_BufferedFrameNumbers; ++i)
        {
            [m_RenderTextures[i] release];
            CVPixelBufferRelease(m_PixelBuffers[i]);
            
            m_RenderTextures[i] = nil;
            m_PixelBuffers[i] = nil;
        }
        
        while (m_FrameQueue.size() > 0)
        {
            m_FrameQueue.pop();
        }
    }

    bool H264Encoder::copyBuffer(void* frameSource, int frameIndex)
    {
        const auto tex = m_RenderTextures[frameIndex];
        
        if (tex == nullptr)
        {
            WriteFileDebug("Error: [copyBuffer] - current renderTexture is null.\n");
            return false;
        }
        
        return m_GraphicDevice->CopyResourceFromNative(tex, frameSource);
    }

    bool H264Encoder::EncodeFrame(void* frameSource, unsigned long long int timestamp)
    {
        if (frameSource == nullptr)
        {
            WriteFileDebug("Error: [encodeFrame] - Received frame is invalid.\n");
            return false;
        }
        
        uint32 bufferIndexToWrite = m_FrameCount % k_BufferedFrameNumbers;
        
        if (!copyBuffer(frameSource, bufferIndexToWrite))
        {
            WriteFileDebug("Error: [encodeFrame] - Received frame source is invalid.\n");
            return false;
        }
        
        CMTime presentationTimeStamp = CMTimeMake(m_FrameCount * 1000 / m_FrameData.frameRate, 1000);
        
        VTEncodeInfoFlags flags;
        OSStatus status = VTCompressionSessionEncodeFrame(m_EncodingSession,
                                                          m_PixelBuffers[bufferIndexToWrite],
                                                          presentationTimeStamp,
                                                          kCMTimeInvalid,
                                                          nullptr,
                                                          nullptr,
                                                          &flags);
        
        if (status != noErr)
        {
            WriteFileDebug("Error: [encodeFrame] - Encoding failed for the current frame.\n");
            return false;
        }
        
        m_LatestTimestamp = timestamp;
        m_FrameCount++;
        return true;
    }

    EncodedFrame* H264Encoder::GetEncodedFrame()
    {
        if (m_FrameQueue.size() > 0)
        {
            return &m_FrameQueue.front();
        }
        return nullptr;
    }

    bool H264Encoder::RemoveEncodedFrame()
    {
        if (m_FrameQueue.size() > 0)
        {
            m_FrameQueue.pop();
            return true;
        }
        return false;
    }
}
