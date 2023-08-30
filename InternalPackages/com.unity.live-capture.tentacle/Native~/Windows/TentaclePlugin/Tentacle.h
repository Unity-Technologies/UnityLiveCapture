#pragma once

#include <Tentacle/TentacleAdvertisement.h>
#include <Tentacle/TentacleDevice.h>
#include <Tentacle/TentacleDeviceCache.h>
#include <Tentacle/TentacleEnum.h>
#include <Tentacle/TentacleLog.h>
#include <Tentacle/TentacleTimecode.h>

#define TENTACLE_MANUFACTURER_ID uint16_t(0x043F)
#define TENTACLE_SYNC_E_ADVERTISEMENT_LENGTH 12
#define TENTACLE_SYNC_E_SCAN_RESPONSE_LENGTH 5

#ifdef TENTACLE_EXPORTS
#define TENTACLE_API __declspec(dllexport)
#else
#define TENTACLE_API __declspec(dllimport)
#endif

extern "C"
{
    void TENTACLE_API StartScanning();
    void TENTACLE_API StopScanning();
}
