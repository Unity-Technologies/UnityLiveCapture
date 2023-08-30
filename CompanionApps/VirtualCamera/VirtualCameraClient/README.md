## Getting Started
This repository uses [Git LFS](https://git-lfs.github.com/) so make sure you have LFS installed to get all the files. Unfortunately this means that the large files are also not included in the "Download ZIP" option on Github.

Requires the [Live Capture](https://github.com/Unity-Technologies/VirtualProduction/) Package to be installed. Check out the readme there for setup instructions.

## Building to device
### Development Environment
* iPad with iOS 12 and ARKit functionality
* MacBook with OSX 10.13 or 10.14
    * Xcode 10 installed
    * Unity 2019.2 or higher AND iOS build support
* Apple Developer account so that you can build to the device

See [Getting started with iOS development](https://docs.unity3d.com/Manual/iphone-GettingStarted.html)

### Unity Build Settings
* Ensure your platform is set to iOS in the `Build Settings`
* Build the `VirtualCameraClient` scene to an ARKit enabled iOS device (iOS 12 or later) using Unity and Xcode

## Using the Mock Client
To test without building to the device:
1. Open the `VirtualCameraClient` scene
2. Additively load your server scene
3. Enter play mode
4. Start the server in the `Performance Capture` window by pressing the `Start` button in the `Server` (note that there is a hidden local host socket not shown in `Available Interfaces`)
5. In the client UI, set the `IP Address` field to `127.0.0.1` and press connect

### Testing UI using the Mock
You can update the visual UI state using the `UIStateManager` inspector in the UI game object.
