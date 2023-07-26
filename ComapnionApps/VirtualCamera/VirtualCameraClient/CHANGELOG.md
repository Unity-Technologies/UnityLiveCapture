# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.5-preview] - 2020-06-09
### Changed
- Video streaming client is only instantiated when video streaming is supported by the client device

## [1.1.4-preview] - 2020-05-04
### Changed
- Camera pose and lens data is now streamed to the server using UDP instead of TCP

## [1.1.3-preview.1] - 2020-03-16
### Fixed
- Moving the camera using joysticks during a rebase operation produces a jump in position on rebase end.

## [1.1.3-preview] - 2020-03-16
### Changed
- Updated video stream client dependency

## [1.1.2-preview] - 2020-03-06
### Changed
- Switched to using the new networking layer

## [1.1.1-preview] - 2020-02-14
### Added
- Official support for yamato and updated version

## [1.0.1] - 2020-01-12
### Changed
- Fix AxisLock locking axis at world space instead of local space of the AR Camera.
- Updated serialized values to match changes to enum backing values
- Upgraded package to support 2019.3

## [1.0.0] - 2019-03-13

### This is the first release of *Virtual Camera*.
Some of the features add include:
* Add support for video stream receiving and decoding via the [Video Streaming Client](https://github.com/Unity-Technologies/com.unity.ig.video-streaming.client) package
* Add UI and code for networked state for basic camera settings 
* Add joystick camera movement with settings for speed
* Add settings window with settings for connection and controls
* Add movement controllers for axis locking and rebasing
* Add Camera tracking via ARKit
* Add bi-directional control of camera settings, i.e. control the camera’s settings from either the editor or the companion app
