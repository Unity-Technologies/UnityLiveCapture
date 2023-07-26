#ifndef Tentacle_h
#define Tentacle_h

#include <Tentacle/TentacleAdvertisement.h>

typedef struct DeviceState
{
    uint8_t name[TENTACLE_ADVERTISEMENT_NAME_LENGTH_MAX];
    uint8_t nameLength;
    uint8_t identifier[TENTACLE_ADVERTISEMENT_IDENTIFIER_LENGTH_MAX];
    uint8_t identifierLength;
    uint8_t isDisappeared;
    uint8_t isUnavailable;
    uint8_t battery;
    uint8_t isCharging;
    uint8_t rssi;
    int32_t frameRate;
    uint8_t isNtsc;
    uint8_t isDropFrame;
    double seconds;
} DeviceState;

#ifdef __cplusplus
extern "C" {
#endif

bool IsScanning(void);
void StartScanning(void);
void StopScanning(void);
int32_t GetDeviceCount(void);
bool GetDeviceState(int32_t index, DeviceState* state);

#ifdef __cplusplus
}
#endif

#endif /* Tentacle_h */
