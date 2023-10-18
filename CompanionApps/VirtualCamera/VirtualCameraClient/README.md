# Virtual Camera Client project

Use this Unity project to develop and build the Unity Virtual Camera app.

To contribute to the maintenance and development of this project, refer to the [Contribution and maintenance](../../../CONTRIBUTING.md) page.

## Building to device

### Development Environment

* iPad with iOS 14.5 or later and ARKit functionality
* MacBook with OSX 10.13 or 10.14
    * Xcode 10 installed
    * Unity 2022.2 or higher AND iOS build support
* Apple Developer account so that you can build to the device

See [Getting started with iOS development](https://docs.unity3d.com/Manual/iphone-GettingStarted.html)

### Unity Build Settings

* Ensure your platform is set to iOS in the `Build Settings`.
* Build the `VirtualCameraClient` Scene to an ARKit enabled iOS device (iOS 14.5 or later) using Unity and Xcode.

## Using the Mock Client

To test without building to the device:
1. Open the `VirtualCameraClient` Scene.
2. Additively load your server Scene.
3. Enter Play mode.
4. Start the server in the `Performance Capture` window by pressing the `Start` button in the `Server` (note that there is a hidden local host socket not shown in `Available Interfaces`).
5. In the client UI, set the `IP Address` field to `127.0.0.1` and press Connect.

### Testing UI using the Mock

You can update the visual UI state using the `UIStateManager` Inspector in the UI GameObject.
