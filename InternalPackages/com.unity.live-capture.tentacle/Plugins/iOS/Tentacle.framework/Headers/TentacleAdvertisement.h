//
//  TentacleAdvertisement.h
//  Tentacle-SDK
//
//  Copyright 2019-2020 Tentacle Sync GmbH
//
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions are met:
//
//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//  2. Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
//
//  3. Neither the name of the copyright holder nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//  AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
//  ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
//  LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//  CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
//  SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
//  INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
//  CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
//  POSSIBILITY OF SUCH DAMAGE.
//

/**
 @file
 @brief Advertisement data.
 
 Contains data of a received Bluetooth advertisement packet.
 */

#ifndef tentacleAdvertisement_h
#define tentacleAdvertisement_h

#ifdef __cplusplus
extern "C" {
#endif

#include <stdio.h>
#include <stdbool.h>
#include <string.h>
#include <Tentacle/TentacleEnum.h>
#include <Tentacle/TentacleTimecode.h>

/// Length of string returned by `tentacleAdvertisementString`.
#define TENTACLE_ADVERTISEMENT_STRING_LENGTH 104
    
/// Length of unique identifier representing a discovered device, see `tentacleAdvertisement.identifier`.
#define TENTACLE_ADVERTISEMENT_IDENTIFIER_LENGTH_MAX 37
    
/// Maximum length of a device name, see `tentacleAdvertisement.name`.
#define TENTACLE_ADVERTISEMENT_NAME_LENGTH_MAX 16

/**
 Contains data of a received Bluetooth advertisement packet.
 */
typedef struct TentacleAdvertisement {
    /// Tentacle Bluetooth protocol version. Currently supported are 1 and 2.
    uint8_t version;
    
    /// Packet type, protocol version 2 only
    enum TentaclePacketType packetType;
    
    /// True if device is connectable via Bluetooth.
    bool connectable;
    
    /// True if a mobile device can authenticate to the device.
    bool linkMode;
    
    /// Hardware device type.
    enum TentacleProductId productId;
    
    /// State the device is in, Sync E.
    enum TentacleDeviceStateSyncE deviceStateSyncE;
    
    /// State the device is in, Track E.
    enum TentacleDeviceStateTrackE deviceStateTrackE;

    /// Audio volume when generating audio timecode.
    enum TentacleAudioLevel audioLevel;
    
    /// True if device is generating NTSC timecode.
    bool ntsc;
    
    /// True if device is generating drop frame timecode.
    bool dropFrame;
    
    /// True if device is in green mode.
    ///
    /// Green mode means that the timecode has been configured, the device is
    /// generating audio timecode and other devices can be synced to this device
    /// using a cable.
    bool greenMode;
    
    /// Hour, minute, second, frame, microseconds and reception timestamp
    TentacleTimecode timecode;
    
    /// Battery level of device in percent.
    uint8_t battery;
    
    /// True if device is currently charging.
    bool charging;
    
    /// Icon index the device is set to.
    uint8_t icon;
    
    ///  Wireless sync status.
    enum TentacleWirelessSyncStatus wirelessSyncStatus;

    ///  Wireless sync update flag.
    ///
    /// Toggles between 1 and 0 after every start of wireless synchronization
    bool wirelessSyncUpdateFlag;
    
    /// Received signal strength indication
    int8_t rssi;

    /// Remaining recording time on SD card
    uint8_t remainingRecordTime;

    /// Low cut state on/off
    bool lowCutEnabled;

    /// Limiter state on/off
    bool limiterEnabled;

    /// Level Meter float mode on/off
    bool floatAudio;

    /// Limiter engaged
    bool limiterEngaged;

    /// Analog audio input clipped
    bool clipped;
    
    /// Unique identifier string representing the device this advertisement was sent from.
    ///
    /// On iOS, this is the string value of a [CBUUID](https://developer.apple.com/documentation/corebluetooth/cbuuid) object.
    ///
    /// On Android, this is the hardware address as returned by [BluetoothDevice.getAddress()](https://developer.android.com/reference/android/bluetooth/BluetoothDevice.html#getAddress()),
    ///
    /// On Nordic nRF5 SDK, this is the hardware address as passed by [bleGapAddr_t](https://www.nordicsemi.com/DocLib/Content/SoftDeviceAPIDoc/S132/v6-1-0/structble_Gap_Addr__t).
    char identifier[TENTACLE_ADVERTISEMENT_IDENTIFIER_LENGTH_MAX];
    
    /// Length of identifer string, has to be less than `tentacleAdvertisementIdentifierLength`.
    size_t identifierLength;
    
    /// Name of the device this advertisement was sent from.
    char name[TENTACLE_ADVERTISEMENT_NAME_LENGTH_MAX];
    
    /// Length of name string, has to be less than `tentacleNameLength`.
    size_t nameLength;
    
    /// Current framerate as integer value, ignoring NTSC flag.
    ///
    /// To calculate the actual framerate, you have to take the NTSC flag into
    /// account or use `tentacleAdvertisementGetFrameRate()`.
    int frameRateInt;
    
    /// True if device can act as sync master
    bool syncMasterCapable;

    /// Peak level of audio input during recording.
    int8_t audioInputPeakLevel;
    
    /// Take number of audio recording.
    uint16_t audioMediaIndex;
    
    /// Elapsed recording or playback time, in seconds.
    uint32_t audioRecordingTime;
    
    /// True if data in this struct is considered to describe a valid timecode.
    ///
    /// Value is set by `tentacleAdvertisementInit()`.
    bool valid;
} TentacleAdvertisement;
        
/**
 Returns an advertisement struct with default values.
 
 @return TentacleAdvertisement struct with default values.
 */
TentacleAdvertisement TentacleAdvertisementMake(void);

/**
 Initializes a `tentacleAdvertisement` from received Bluetooth advertisement
 packet data.

 If the passed data is considered to be valid, `advertisement.valid` is set to true.

 @param advertisementData Byte array containing advertisement packet data.
 @param advertisementDataLen Length of advertisement data array.
 @param serviceData Byte array containing service data packet.
 @param serviceDataLen Length of service data.
 @param rssi Received Signal Strength Indication.
 @param receivedTimestamp Precise timestamp of when this packet was received,
        in seconds.
 @param identifier Unique identifier string representing the device this
        advertisement was sent from.
 @param identifierLength Length of identifier string, has to be less than
        `tentacleAdvertisementIdentifierLengthMax`.
 @param name Name of the device this advertisement was sent from.
 @param nameLength Length of name string, has to be less than
        `tentacleNameLength`.
 @return Advertisement struct containing received packet data.
 */
TentacleAdvertisement TentacleAdvertisementInit(const unsigned char *advertisementData, const size_t advertisementDataLen, const unsigned char *serviceData, const size_t serviceDataLen, const int8_t rssi, const double receivedTimestamp, const char *identifier, const size_t identifierLength, const char *name, const size_t nameLength);

/**
 Check if two frame rates are equal.
 
 @param advertisement1 First frame rate to compare.
 @param advertisement2 Second frame rate to compare.
 @return True if frame rate are equal, false otherwise.
 */
bool TentacleAdvertisementFrameRateEqual(const TentacleAdvertisement *advertisement1, const TentacleAdvertisement *advertisement2);

/**
 Framerate, depending on value of NTSC flag. Also see
 `tentacleAdvertisement.frameRateInt` for framerate ignoring the NTSC flag.
 
 @param advertisement Advertisement to get the framerate of.
 @return Framerate depending on NTSC flag.
 */
double TentacleAdvertisementGetFrameRate(const TentacleAdvertisement *advertisement);

/**
 String value of advertisement, containing frame rate, battery state and more.
 
 @param advertisement Get string value of this advertisement.
 @param advertisementString Resulting string value. Has to be of length
 `TENTACLE_ADVERTISEMENT_STRING_LENGTH`, includes terminating null
 character.
 @return Number of characters of `advertisementString`, not including the
 terminating null character.
 */
int TentacleAdvertisementString(const TentacleAdvertisement * const advertisement, char *advertisementString);

/**
 Name of the device this advertisement was received from.
 
 @param advertisement advertisement to get the name of.
 @param nameString Resulting name string
 @return Length of returned string.
 */
int TentacleAdvertisementNameString(const TentacleAdvertisement * const advertisement, char *nameString);

/**
 Print string representation of advertisement to stdout.
 
 @param advertisement Advertisement struct to print.
 */
void TentacleAdvertisementPrint(const TentacleAdvertisement * const advertisement);

#ifdef __cplusplus
}
#endif

#endif /* tentacleAdvertisement_h */
