# Create and set up a Capture Device

Create a [Capture Device](data-capture-process.md#capture-device) to map a connected [Client Device](data-capture-process.md#client-device) to an [Actor](data-capture-process.md#actor) of the Scene.

## Requirements

To be able to set up a functional Capture Device, you have to:
* [Connect a Client Device to the Unity Editor](connection-device.md) through the local network.
* [Set up a GameObject of the Scene as an Actor](capture-device-setup-actor.md) with the proper Live Capture components.

## Setup

1. Open the Take Recorder window: **Window** > **Live Capture** > **Take Recorder**.

2. In the **Capture Devices** section, click on the **+** (plus) button and select the type of device to add.

3. Select the newly created Capture Device.

4. In the Take Recorder window's right pane:
    * Select the connected **Client Device** to use to drive the Actor.
    * Select the **Actor**, which is the GameObject you need to drive from the Client Device.

>**Note:** Some types of Capture Devices may automatically detect the connected Client Device, in which case you would only have to select the GameObject to use as the Actor to drive.

## Additional references

* [Get started with Virtual Camera setup](virtual-camera-getting-started.md)
* [Get started with Face Capture setup](face-capture-getting-started.md)
