# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.0-preview] - 2020-06-09
### Added
- Multiple video streams can use the same camera
- Added method to check for platform support for the VideoStreamingServer
- It is now possible to control which video stream is previewed in the Game window

### Changed
- VideoStreamSource components are now hidden and not saved to the scene
- The maximum supported video resolution has been increased to 4096x2048, rather than the official H.264 standard of 2048x2048
- The video resolution block size has been changed to 4 from 16, allowing more granular resolutions
- Removed unneeded logging information from the RTSP server

### Fixed
- Encoder data sizing error message gave the wrong information
- RGB to NV12 convertion shader incorrectly used the the main texture texel size

## [1.1.4-preview] - 2020-05-1
### Changed
- GOP size for video stream has been set to just a few frames, helping the stream to recover from dropped packets faster.
- The fps cap for the video stream has been increased to 60.

### Fixed
- Resolved potential race condition where the readback frame data could be cleared before being copied to the encoder.
- Encoding slower than the editor frame rate no longer causes the server to buffer inreasing amounts of frames.

## [1.1.3-preview] - 2020-04-29
### Changed
- A new thread is now used for each video encoder/RTSP server, reducing latency and decreasing work done on the main thread.

## [1.1.2-preview] - 2020-04-22
### Added
- Low latency mode in VideoStreamSource that reduces latency by a few frames for a slight performance cost. Enabled by default.

## [1.1.1-preview] - 2020-03-16
### Fixed
- Missing tooltips in VideoStreamSource.

## [1.1.0-preview] - 2020-02-14
### Changed
- Updating minor ver to align with other packages for Yamato support

## [1.0.0-preview] - 2020-02-12
### Added
- Support for Yamato and building/packaging/publishing

## [0.1.1] - 2020-01-23
### Added
- Resolution Profile field in VideoStreamSource. Allows for better control of the encoded resolution by using known values and terminology.

### Changed
- Bit-rate slider changed into Quality slider. Bit-rate it is now calculated using the resolution, framerate and the quality percentage.
- Upgraded to 2019.3.

### Fixed
- Fixed video stopping when setting new frame-rate values.  

## [0.0.1] - 2019-03-13

### This is the first release of *Video Streaming Server*.
* Encoding on Windows 10
