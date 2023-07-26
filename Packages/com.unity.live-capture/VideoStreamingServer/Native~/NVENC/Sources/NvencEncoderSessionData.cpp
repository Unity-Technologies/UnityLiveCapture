#include "NvencEncoderSessionData.h"

namespace NvencPlugin
{
    NvencEncoderSessionData::NvencEncoderSessionData(const NvencEncoderSessionData& other) :
        width(other.width),
        height(other.height),
        frameRate(other.frameRate),
        bitRate(other.bitRate * BitRateInKilobits),
        gopSize(other.gopSize)
    { }

    bool NvencEncoderSessionData::operator==(const NvencEncoderSessionData& other) const
    {
        return width == other.width &&
            height == other.height &&
            frameRate == other.frameRate &&
            bitRate == other.bitRate * BitRateInKilobits &&
            gopSize == other.gopSize;
    }

    void NvencEncoderSessionData::Update(const NvencEncoderSessionData& other)
    {
        width = other.width;
        height = other.height;
        frameRate = other.frameRate;
        bitRate = other.bitRate * BitRateInKilobits;
        gopSize = other.gopSize;
    }
};
