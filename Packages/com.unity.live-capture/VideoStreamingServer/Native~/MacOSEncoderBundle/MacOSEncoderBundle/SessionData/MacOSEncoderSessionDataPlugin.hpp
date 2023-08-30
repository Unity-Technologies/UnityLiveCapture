#pragma once

#include <iostream>
#include <vector>

namespace MacOsEncodingPlugin
{
    static const uint64_t BitRateInKilobits = 1000;

    struct MacOSEncoderSessionData
    {
        MacOSEncoderSessionData() = default;
        MacOSEncoderSessionData(const MacOSEncoderSessionData& other);

        bool operator==(const MacOSEncoderSessionData& other) const;
        void Update(const MacOSEncoderSessionData& other);

        int width = 0;
        int height = 0;
        int frameRate = 0;
        int bitRate = 0;
        int gopSize = 0;
    };

    enum class EncoderFormat
    {
        /// <summary>
        /// Represents a biplanar format with a full sized Y plane followed by a single chroma plane with weaved U and V values.
        /// </summary>
        NV12,

        /// <summary>
        /// Represents an 8 bit monochrome format.
        /// </summary>
        R8G8B8
    };

    // Retrieve the encoder by using the id parameter and set it's new settings.
    struct EncoderSettingsID
    {
        MacOSEncoderSessionData settings;
        int id;
        EncoderFormat encoderFormat;
        bool useSRGB;
    };

    // Retrieve the encoder by using the id parameter and encode the renderTexture parameter.
    struct EncoderTextureID
    {
        void* renderTexture;
        int id;
        unsigned long long int timestamp;
    };

    // Retrieve the encoder by using the id parameter, and get it's status.
    struct EncoderGetStatus
    {
        bool isValid;
        int id;
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
