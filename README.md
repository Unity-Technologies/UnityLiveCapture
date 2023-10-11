# Live Capture with the Unity Editor

With the Unity Live Capture package (`com.unity.live-capture`), you can capture animations from multiple physical devices and record them in the Unity Editor to animate characters and cameras.

This repository contains all packages, applications, tests, and demo projects related with Unity Live Capture.

* [Get started](#get-started)
* [Live Capture package](#live-capture-package)
* [Live Capture companion apps](#live-capture-companion-apps)

## Get started

### Learn about Live Capture

To learn about the Unity Live Capture package and apps (concepts, features, and workflows) read the [Live Capture package documentation](https://docs.unity3d.com/Packages/com.unity.live-capture@4.0/manual/index.html) from the latest external release.

### Get the code

This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github.

### Contribute to the repository

To contribute to the Unity Live Capture ecosystem improvement effort in this repository, read our [contribution and maintenance guidelines](CONTRIBUTING.md).

### Check out the licensing model

The Live Capture packages and companion apps are licensed under the [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).

## Live Capture package

### Access the Live Capture package folder

| Package | Description |
| :--- | :--- |
| **[Live Capture](Packages/com.unity.live-capture)** | The core package that enables all the Live Capture functionality in the Unity Editor. |

### Test the Live Capture package

Use these Unity projects to run various tests against the Live Capture package.

| Project | Description |
| :--- | :--- |
| [HDRPAndURPTests](TestProjects/HDRPAndURPTests) | Runs the pipelines tests with HDRP and URP installed. |
| [HDRPTests](TestProjects/HDRPTestsLatest) | Runs the pipelines tests with HDRP installed. |
| [HDRPWithCinemachineTests](TestProjects/HDRPWithCinemachineTests) | Runs the pipelines tests with HDRP and the Cinemachine package installed. |
| [LiveCaptureGraphicsTestsHDRP](TestProjects/LiveCaptureGraphicsTestsHDRP) | Runs the graphics tests for Live Capture with HDRP package installed. |
| [PkgTests](TestProjects/PkgTests) | Runs a subset of tests that are not dependent on a render pipeline (the ones in com.unity.live-capture.tests). |
| [URPTests](TestProjects/URPTests) | Runs the pipelines tests with URP installed. |
| [URPWithCinemachineTests](TestProjects/URPWithCinemachineTests) | Runs the pipelines tests with URP and the Cinemachine package installed. |

The Live Capture test projects use additional internal packages available in this repository:

| Package | Description |
| :--- | :--- |
| [com.unity.live-capture.tests](Packages/com.unity.live-capture.tests) | Includes all Live Capture base tests. |
| [com.unity.live-capture.hdrp.tests](Packages/com.unity.live-capture.hdrp.tests) | Includes all HDRP specific tests. |
| [com.unity.live-capture.pipelines.tests](Packages/com.unity.live-capture.pipelines.tests) | Includes all rendering pipeline base tests. |

## Live Capture companion apps

### Get the companion apps

The Unity Live Capture ecosystem includes two companion apps that you can install for free on your iOS device and use alongside the Live Capture package to capture animations from your device’s camera and sensors.

The latest published versions of the Live Capture companion apps are available on the App Store.

| App | Description |
| :--- | :--- |
| **[Unity Virtual Camera](https://apps.apple.com/us/app/unity-virtual-camera/id1478175507)** | Drive a Unity camera from your iPad or iPhone and record camera movement, rotation, focus, zoom, and more. |
| **[Unity Face Capture](https://apps.apple.com/us/app/unity-face-capture/id1544159771)** | Animate and record facial expressions and head angle. |

### Develop the companion apps

Use these Unity projects to develop and deploy the Live Capture companion apps.

| Project | Description |
| :--- | :--- |
| **[Virtual Camera Client](CompanionApps/VirtualCamera/VirtualCameraClient)** | Unity project to develop and deploy the **Unity Virtual Camera** app. |
| **[Face Capture Client](CompanionApps/FaceCapture/FaceCaptureClient)** | Unity project to develop and deploy the **Unity Face Capture** app. |

The companion app development projects use additional internal packages available in this repository:

| Package | Description |
| :--- | :--- |
| **[com.unity.live-capture.cinematic-companion-app-core](InternalPackages/com.unity.live-capture.cinematic-companion-app-core)** | Core package for the Cinematic Companion App. Used for both the Virtual Camera and the Face Capture app. |
| **[com.unity.live-capture.tentacle](InternalPackages/com.unity.live-capture.tentacle)** | Adds support for the Tentacle timecode sources. |
| **[com.unity.touch-framework](InternalPackages/com.unity.touch-framework)** | Foundation for building touch applications using Unity's in-development Touch Framework. |
| **[com.unity.video-streaming.client](InternalPackages/com.unity.video-streaming.client)** | Adds support for receiving and decoding a video stream over RTSP. |
| **[com.unity.zenject](InternalPackages/com.unity.zenject)** | Dependency injection framework used by the Virtual Camera companion apps. |

### Test the companion apps via sample projects

Use these Unity projects to manually test the companion apps.

| Project | Description |
| :--- | :--- |
| **[Virtual Camera HDRP Demo](CompanionApps/VirtualCamera/VirtualCameraDemo)** | Contains a basic scene with animated character to test the Virtual Camera app. |
| **[Face Capture Demo](CompanionApps/FaceCapture/FaceCaptureDemo)** | Contains a basic rigged character head to test the Face Capture app. |
