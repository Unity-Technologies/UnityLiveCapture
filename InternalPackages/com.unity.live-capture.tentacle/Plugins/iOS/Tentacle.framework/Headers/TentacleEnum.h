//
//  TentacleEnum.h
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
 @brief Tentacle enumerations.
 */

#ifndef tentacleenum_h
#define tentacleenum_h

#ifdef TentacleAdvertisement
extern "C" {
#endif

/**
 State the Sync E device is in.
 */
typedef enum TentacleDeviceStateSyncE {
    /// Device is powered off.
    TentacleDeviceStateSyncEPowerOff = 0,
    
    /// Device is booting.
    TentacleDeviceStateSyncEStartup = 1,
    
    /// Device is in standby.
    TentacleDeviceStateSyncEStandby = 2,
    
    /// No cable is plugged into the device, no audio timecode is generated.
    TentacleDeviceStateSyncEUnplugged = 3,
    
    /// Cable is plugged in and device is generating timecode.
    TentacleDeviceStateSyncEGeneratingTimecode = 4,
    
    /// Device is reading timecode.
    TentacleDeviceStateSyncEReadingTimecode = 5,
    
    /// Device is stopped.
    TentacleDeviceStateSyncEStopped = 6
} TentacleDeviceStateSyncE;

/**
 State the Track E device is in.
 */
typedef enum TentacleDeviceStateTrackE {
    /// Device is powered off.
    TentacleDeviceStateTrackEPowerOff = 0x00,
    
    /// Device is starting.
    TentacleDeviceStateTrackEStartup = 0x01,
    
    /// Device is in standby.
    TentacleDeviceStateTrackEStandby = 0x02,
    
    /// Device is ready.
    TentacleDeviceStateTrackEReady = 0x03,
    
    /// Device is recording.
    TentacleDeviceStateTrackERecording = 0x04,
    
    /// Device is playing audio.
    TentacleDeviceStateTrackEPlayback = 0x05,
    
    /// Device is reading timecode.
    TentacleDeviceStateTrackEReadTc = 0x06,
    
    /// Device is syncing.
    TentacleDeviceStateTrackEWirelessSync = 0x07,
    
    /// Device is formatting SD card.
    TentacleDeviceStateTrackEFormatSd = 0x08,
    
    /// Device is testing SD card speed.
    TentacleDeviceStateTrackESpeedTestSd = 0x09,
    
    /// Device is coping files.
    TentacleDeviceStateTrackEFileTransfer = 0x0a,
    
    /// Device is in card reader mode.
    TentacleDeviceStateTrackECardReaderMode = 0x0b,
    
    /// Device is in USB audio mode.
    TentacleDeviceStateTrackEUsbAudio = 0x0c,
    
    /// SD card is missing.
    TentacleDeviceStateTrackENoSd = 0x80,
    
    /// SD card is in wrong format.
    TentacleDeviceStateTrackESdWrongFormat= 0x81,
    
    /// SD card reading or writing failed.
    TentacleDeviceStateTrackESdReadWriteError = 0x82,
    
    /// SD card is full.
    TentacleDeviceStateTrackESdFull = 0x83,
    
    /// SD card index error.
    TentacleDeviceStateTrackESdIndexError = 0x84,
    
    /// Codec error.
    TentacleDeviceStateTrackECodecError = 0x90
    
} TentacleDeviceStateTrackE;

/**
 Hardware device type.
 */
typedef enum TentacleProductId {
    /// Generic
    TentacleProductIdGeneric = 0,
    
    /// Sync E
    TentacleProductIdSyncE = 1,
    
    /// Track E
    TentacleProductIdTrackE = 2,
} TentacleProductId;

/**
 When producing timecode to be recorded on an audio track, the timecode is
 encoded as an audio signal using a certain volume. This volume can be
 low (Mic), high (Line), something in between (Custom) or be automatically
 detected (AutoMic).
 */
typedef enum TentacleAudioLevel {
    /// Audio timecode is produced at a low volume
    TentacleAudioLevelMic = 0,
    
    /// Audio timecode is produced at a high volume
    TentacleAudioLevelLine = 1,
    
    /// Audio timecode is produced at a medium volume
    TentacleAudioLevelCustom = 2,
    
    /// Audio timecode is automatically set to a low volume
    TentacleAudioLevelAutoMic = 3
} TentacleAudioLevel;

/**
 Wireless sync status.
 */
typedef enum TentacleWirelessSyncStatus {
    /// Wireless sync has not been started
    TentacleWirelessSyncStatusNotStarted = 0,
    
    /// Wireless sync is in progress
    TentacleWirelessSyncStatusStarted = 1,
    
    /// Wireless sync failed for this device
    TentacleWirelessSyncStatusFailed = 2,
    
    /// Wireless sync has been successful for this device
    TentacleWirelessSyncStatusSuccess = 3
} TentacleWirelessSyncStatus;

/**
 Advertisement version 2 packet type.
 */
typedef enum TentaclePacketType {
    /// Packet contains status information only
    TentaclePacketTypeStatusOnly = 0,
    
    /// Simple timestamp format, hh:mm:ss:ff (length 9 bytes)
    TentaclePacketTypeSimpleTimestamp = 1,
    
    /// Simple timestamp including microseconds since last frame (length 11 bytes)
    TentaclePacketTypeMicroseconds = 2,
    
    /// Microseconds since midnight (length 10 bytes)
    TentaclePacketTypeMicrosecondsSinceMidnight = 3
} TentaclePacketType;

#endif /* tentacleenum_h */
