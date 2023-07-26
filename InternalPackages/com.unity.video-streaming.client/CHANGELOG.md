# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.2.0-preview] - 2020-06-09
### Added
- Added method to check for platform support for the VideoStreamingClient

### Changed
- Removed unneeded debug logging

### Fixed
- Fixed incorrect decoder null check
- Updated platform define for IOS from deplicated platform define

## [1.1.3-preview.1] - 2020-05-14
### Added
- VideoStreamingClient has a new lowLatencyMode option that reduces latency by a frame for a moderate performance impact

## [1.1.2-preview] - 2020-03-16
### Fixed
- Video color in linear space for 2019.3

## [1.1.1-preview] - 2020-02-14
### Changed
- Aligning minor patch level for yamato support

## [1.0.1-preview] - 2020-02-13
### Added
- Yamato support

## [1.0.1] - 2020-01-23
### Changed
- Upgraded to 2019.3.

## [1.0.0] - 2019-03-13
### This is the first release of *Video Streaming Server*.
* Add decoding on iOS
