#pragma once

#include <iostream>

namespace NvencPlugin
{
    static const uint64_t BitRateInKilobits = 1000;

    struct NvencEncoderSessionData
    {
        NvencEncoderSessionData() = default;
        NvencEncoderSessionData(const NvencEncoderSessionData& other);

        bool operator==(const NvencEncoderSessionData& other) const;
        void Update(const NvencEncoderSessionData& other);

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

    class NvEncoder;

    // Retrieve the encoder by using the id parameter and set it's new settings.
    struct EncoderSettingsID
    {
        NvencEncoderSessionData settings;
        int id;
        EncoderFormat encoderFormat;
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
}
