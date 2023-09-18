# Live Capture in Unity Editor

With Unity Live Capture package (`com.unity.live-capture`), you can capture and record animations from multiple physical devices and record 
them in the Unity Editor to animate characters and cameras.    

This includes the Live Capture Companion Apps that you can install on your iOS device to capture animations from your device’s camera and sensors.

This repository contains  all in-development packages applications, tests, and demo projects to enable Live Capture in Unity Editor.

# Get started

## Documentation

The documentation for the latest external release can be found **[here](https://docs.unity3d.com/Packages/com.unity.live-capture@3.0/manual/index.html)**.

## Contributing
Read our [contribution and maintenance guidelines](CONTRIBUTING.md).

## Licensing

The Live Capture packages and companion apps are licensed under the [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).

## Contents of the repository
### Live Capture Package
- **[Live Capture](Packages/com.unity.live-capture/README.md)**

### Companion App projects

The following projects are used to build and deploy the companion apps.

- **[Virtual Camera Client](https://apps.apple.com/us/app/unity-virtual-camera/id1478175507)** -- project used to develop and deploy the Unity Virtual Camera App.
- **[Face Capture Client](https://apps.apple.com/app/id1544159771)** -- project used to develop and deploy the Unity Face Capture App.

### Sample projects
The following projects are used for manual testing:

- **[Virtual Camera HDRP Demo](CompanionApps/VirtualCamera/VirtualCameraDemo/Assets/Readme.asset)** -- The main demo project used for internal and external testers in some cases.
- **Face Capture Demo** -- Contains a rigged version of the WiNDUP girl to test the Face Capture app.

### Test packages

- com.unity.live-capture.tests
- com.unity.live-capture.pipelines.tests

### Test projects

| Project                  | Description |
| ------------------------ | ----------- |
| HDRPAndURPTests          | Runs the pipelines tests with HDRP and URP installed.|
| HDRPTests                | Runs the pipelines tests with HDRP and URP installed. |
| HDRPWithCinemachineTests | Runs the pipelines tests with HDRP and the Cinemachine package installed. |
| PkgTests                 | Runs a subset of tests that are not dependent on a render pipeline (the one in com.unity.live-capture.tests. |
| URPTests                 | Runs the pipelines tests with URP installed. |
| URPWithCinemachineTests  | Runs the pipelines tests with URP and the Cinemachine package installed.|


# Technical annexes

## Installation
This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github.

Both iOS apps are also uploaded on a daily basis to Test Flight. To be added as a tester to Test Flight, contact <guillaume.levasseur@unity3d.com>.

You can also install the latest release from the App Store.

## Architecture

![image](https://user-images.githubusercontent.com/6132575/236289177-ff7d87aa-e3e5-4f8e-ab6d-2a7488078319.png)
