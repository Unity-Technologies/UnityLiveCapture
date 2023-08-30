//
//  TentacleDeviceCache.h
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
 @brief Discovered devices.

 Caches all devices that have been seen.
 */

#ifndef tentacleDeviceCache_h
#define tentacleDeviceCache_h

#ifdef __cplusplus
extern "C" {
#endif
    
#include <stdio.h>
#include <Tentacle/TentacleAdvertisement.h>
#include <Tentacle/TentacleDevice.h>

/**
 Process a received Bluetooth advertisement packet.
 
 @param advertisement Advertisement data to be processed.
 @return Index of updated device in device cache.
 */
size_t TentacleDeviceCacheProcess(const TentacleAdvertisement *advertisement);

/**
 Get current size of device cache.
 
 @return Size of device cache.
 */
size_t TentacleDeviceCacheGetSize(void);

/**
 Get current size of timecode cache for a specific device index.
 
 @param deviceIndex Get cache size for device with this index.
 @return Timecode cache size.
 */
size_t TentacleDeviceCacheGetTimecodeCacheSize(const int deviceIndex);

/**
 Get cached device at specific index.
 
 @param deviceIndex Index of device to get.
 @return Device struct at specific index.
 */
TentacleDevice TentacleDeviceCacheGetDevice(const int deviceIndex);

/**
 Get index in the device cache for a specific device identifier.

 @param identifier Identifier of device to get.
 @param identifierLength Length of identifier.
 @return Index in the device cache for this identifier.
 */
int TentacleDeviceCacheIndexFromIdentifier(const char *identifier, const size_t identifierLength);

/**
 Get cached advertisement for device at specific index.
 
 @param deviceIndex Index of device to get advertisement for.
 @return Cached advertisement struct.
 */
TentacleAdvertisement TentacleDeviceCacheGetAdvertisement(const int deviceIndex);

/**
 Get timecode string representing a cached timecode.
 
 @param deviceIndex Get timecode string for device with this index.
 @param timecodeIndex Get timecode string for device with this index.
 @param timecodeCacheString Resulting string value. Has to be of length
 `TENTACLE_TIMECODE_STRING_LENGTH`, includes terminating null character.
 @return Number of characters of `timecodeCacheString`, not including the
 terminating null character.
 */
int TentacleDeviceCacheTimecodeString(const int deviceIndex, const int timecodeIndex, char *timecodeCacheString);

/**
 Get timecode string for a specific point in time.
 
 @param deviceIndex Get timecode string for device with this index.
 @param atTimestamp Get timecode at this timestamp.
 @param timecodeString Resulting string value. Has to be of length
        `TENTACLE_TIMECODE_STRING_LENGTH`.
 @return Number of characters of `timecodeString`, not including the
 terminating null character.
 */
int TentacleDeviceCacheTimecodeStringAtTimestamp(const int deviceIndex, const double atTimestamp, char *timecodeString);

/**
 Get timecode string for a specific point in time.

 @param identifier Identifier of device to get.
 @param identifierLength Length of identifier.
 @param atTimestamp Get timecode at this timestamp.
 @param timecodeString Resulting string value. Has to be of length `TENTACLE_TIMECODE_STRING_LENGTH`.
 @return Number of characters of `timecodeString`, not including the
 terminating null character.
 */
int TentacleDeviceCacheTimecodeStringAtTimestampForIdentifier(const char *identifier, const size_t identifierLength, const double atTimestamp, char *timecodeString);

/**
 Print string representation of device cache to stdout.
 
 @param atTimestamp Print timecodes at this timestamp.
 */
void TentacleDeviceCachePrint(const double atTimestamp);
    
#ifdef __cplusplus
}
#endif

#endif /* tentacleDeviceCache_h */
