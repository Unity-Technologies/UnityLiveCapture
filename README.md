# About

This is an internal container repository for all in-development packages, tests, and demo projects pertaining to Live Capture.

**This repository is not to be shared externally. External users need to have signed an access agreement before accessing any non-public files**

This repository uses [Git LFS](https://git-lfs.github.com/), so large files and some binary files like icons are unfortunately not included in the "Download ZIP" option on Github.

<a name="Contents"></a>
## Contents
### Packages
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

<a name="Installation"></a>
## Installation
This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github.

Both iOS apps are also uploaded on a daily basis to Test Flight. To be added as a tester to Test Flight, contact <guillaume.levasseur@unity3d.com>.

You can also install the latest release from the App Store.

## Contributing
Read the [Versioning and Changelog Guidelines](https://docs.google.com/document/d/1TEkXz3i3J5QNk3KUALfhf4uP-1B9gaB6zJIJsTAx0cI/edit?usp=sharing).

## Documentation

- The documentation for the latest external release can be found **[here](https://docs.unity3d.com/Packages/com.unity.live-capture@3.0/manual/index.html)**.

## Architecture

![image](https://user-images.githubusercontent.com/6132575/236289177-ff7d87aa-e3e5-4f8e-ab6d-2a7488078319.png)

## App Deployment
See our internal documentation on [Unity Cloud Build and TestFlight deployment](https://docs.google.com/document/d/1TtsGuDZg9TgFeF32iUdb64ulPGDsUV4Lb572PiVwyIA/edit?usp=sharing)

## Branching Strategy
### [develop]
The main branch where the source code always reflects a state with the latest delivered development changes for the next release, also known as the “integration branch”.

Branch from `main` to implement any enhancements or documentation updates, etc. If fixing a bug, branch of the tagged version with the bug and then make sure to port the fix to `main`.

### [publish]
Publish branches are the release branches. We use them to publish major, minor and patch releases. They are prefixed with the package name

Example:

- live-capture/2.0/staging
- live-capture/2.0/release

The 1.0/staging and 1.0/release branches are an exception to the naming convention and used for the live-capture 1.1.X releases.

Beta releases are under beta-release. For example:

beta-release/202208

We run the release-zip yamato job on this branch and the distribute zip file artifact. 
