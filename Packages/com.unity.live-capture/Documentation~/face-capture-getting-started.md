# Get started with Face Capture

Install, connect and set up all elements to animate a sample character head within Unity from the Face Capture app.

## Installation

1. [Install the Live Capture package](installation.md).

2. Install the Unity Virtual Camera app:
   | App name | Device requirements | Link |
   |:---|:---|:---|
   | **Unity Face Capture** | iPhone or iPad with:<br />• iOS 14.6 or higher<br />• ARKit _face tracking_ capabilities ([device supporting Face ID](https://support.apple.com/en-us/HT209183) **or** [device with an A12 Bionic chip](https://en.wikipedia.org/wiki/Apple_A12)) | [![Unity Face Capture](images/app-store-badge.png)](https://apps.apple.com/us/app/unity-face-capture/id1544159771) |

## Connect the app to the Unity Editor

1. Make sure to correctly [set up your local network and firewall](connection-network.md).

2. Open the [Connections window](ref-window-connections.md): from the Unity Editor main menu, select **Window** > **Live Capture** > **Connections**.

3. Create a Connection of type **Companion app Server** and enable it.

4. From the Face Capture app, [enable the connection](connection-device.md#companion-app-connection) to the created server.

## Install the sample head provided with the package

1. From the Unity Editor main menu, select **Window** > **Package Manager**.

2. Search and select **Live Capture** in the package list.

3. In the right pane, select the **Samples** tab, and beside **ARKit Face Sample**, click on **Import**.

4.  In the Project window, in the `­­Assets/Samples/Live Capture/…/ARKit Face Sample` folder, open the
`FaceCaptureSample` file.

  A new Scene opens with a SampleHead GameObject containing an ARKit Face Actor component in its Hierarchy. This is the character head you're going to animate from the Face Capture app.

## Set up the ARKit Face Device

>**Note:** The sample Scene already includes an ARKit Face Device mapped to the sample head. You don't need to create a new one.

1. Open the [Take Recorder window](ref-window-take-recorder.md): **Window** > **Live Capture** > **Take Recorder**.

2. In the **Capture Devices** section, select the device that targets the **FaceDevice** GameObject.

3. In the Take Recorder window's right pane, select the **Client Device** you previously connected to the Unity Editor.

## Test the Face Capture

1. In the Editor, in the Take Recorder window, make sure the Live mode is enabled.  
   ![Take Recorder Window](images/ref-window-take-recorder-live.png)

2. With your mobile device in hand, frame your face on the screen and try out face and head movements.

   You should see the same face movements on the character head in the Editor Game view.

## Additional resources

* [Face Capture app interface reference](face-capture-app-ui.md)
* [Record takes from the Face Capture app](face-capture-record-takes.md)
* [Default Face Mapper](ref-component-arkit-default-face-mapper.md)
