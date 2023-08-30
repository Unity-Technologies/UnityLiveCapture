#include "MacOSEncoderSessionDataPlugin.hpp"

namespace MacOsEncodingPlugin
{
    MacOSEncoderSessionData::MacOSEncoderSessionData(const MacOSEncoderSessionData& other)
    : width(other.width)
    , height(other.height)
    , frameRate(other.frameRate)
    , bitRate(other.bitRate * BitRateInKilobits)
    , gopSize(other.gopSize)
    { }

    bool MacOSEncoderSessionData::operator==(const MacOSEncoderSessionData& other) const
    {
        return width == other.width &&
            height == other.height &&
            frameRate == other.frameRate &&
            bitRate == other.bitRate * BitRateInKilobits &&
            gopSize == other.gopSize;
    }

    void MacOSEncoderSessionData::Update(const MacOSEncoderSessionData& other)
    {
        width = other.width;
        height = other.height;
        frameRate = other.frameRate;
        bitRate = other.bitRate * BitRateInKilobits;
        gopSize = other.gopSize;
    }
};
