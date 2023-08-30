#include "Tentacle.h"
#include "TentacleController.h"
#include <Tentacle/TentacleDevice.h>

#ifdef __cplusplus
extern "C" {
#endif

TentacleController* s_controller;
bool s_isScanning;

bool IsScanning()
{
    return s_isScanning;
}

void StartScanning()
{
    if (s_isScanning)
    {
        return;
    }
    
    s_controller = [[TentacleController alloc] init];
    s_isScanning = true;
}

void StopScanning()
{
    if (!s_isScanning)
    {
        return;
    }
    
    [s_controller stopScanning];
    s_isScanning = false;
}

int32_t GetDeviceCount()
{
    if (!s_isScanning)
    {
        return 0;
    }
    
    return (int32_t)TentacleDeviceCacheGetSize();
}

bool GetDeviceState(int32_t index, DeviceState* state)
{
    if (index < 0 || index >= GetDeviceCount())
    {
        return false;
    }
    
    const double timestamp = [TentacleBluetoothController timestamp];
    
    const TentacleDevice device = TentacleDeviceCacheGetDevice(index);
    const TentacleAdvertisement advert = device.advertisement;
    const double frameRate = TentacleAdvertisementGetFrameRate(&advert);
    const TentacleTimecode timecode = TentacleTimecodeAtTimestamp(device.timecode, frameRate, advert.dropFrame, timestamp);
    const double seconds = TentacleTimecodeSecondsAtTimestamp(&timecode, frameRate, advert.dropFrame, timestamp);
    
    memcpy(&state->name, advert.name, advert.nameLength);
    state->nameLength = advert.nameLength;
    memcpy(&state->identifier, advert.identifier, advert.identifierLength);
    state->identifierLength = advert.identifierLength;
    state->isDisappeared = TentacleDeviceIsDisappeared(&device, timestamp);
    state->isUnavailable = TentacleDeviceIsUnavailable(&device, timestamp);
    state->battery = advert.battery;
    state->isCharging = advert.charging;
    state->rssi = advert.rssi;
    state->frameRate = advert.frameRateInt;
    state->isNtsc = advert.ntsc;
    state->isDropFrame = advert.dropFrame;
    state->seconds = seconds;
    
    return true;
}

#ifdef __cplusplus
}
#endif
