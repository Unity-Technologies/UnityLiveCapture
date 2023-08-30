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
    using OutputFrame = NV_ENC_OUTPUT_PTR;

    const uint32_t k_BufferedFrameNum = 4;
    const uint32_t k_GOPSize = 2;

    struct InputFrame
    {
        NV_ENC_REGISTERED_PTR registeredResource;
        NV_ENC_INPUT_PTR      mappedResource;
        NV_ENC_BUFFER_FORMAT  bufferFormat;
    };

    struct Frame
    {
        InputFrame           inputFrame;
        OutputFrame          outputFrame;
        std::vector<uint8_t> encodedFrame;
        std::atomic<bool>    isEncoding = false;
        std::atomic<bool>    isEncoded = false;
    };

    struct EncodedFrame
    {
        std::vector<uint8_t>   spsSequence;
        std::vector<uint8_t>   ppsSequence;
        std::vector<uint8_t>   imageData;
        unsigned long long int timestamp;
        bool                   isKeyFrame;
    };
}
