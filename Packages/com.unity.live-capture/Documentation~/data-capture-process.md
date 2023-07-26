# Data capture process

The main purpose of the Live Capture package is to:
* Capture real-time data from external physical devices having real world tracking capabilities.
* Interpret the captured data in the Unity Editor and apply it to specific GameObjects of the Scene.

For example, you can drive a Unity camera or animate a character in the Unity Scene using a physical device having the capability of tracking the corresponding data in the real world.

## Chain of involved elements

<a name="client-device"></a><a name="connection"></a><a name="capture-device"></a><a name="actor"></a>

The data capture process implies a chain of hardware and software elements that allow the streaming and processing of the data from the physical device (called Client Device) to the GameObject (called Actor) within the Unity Editor.

| Element | Description | In practice |
| :--- | :--- | :--- |
| **Client Device** | A hardware device capable of tracking data from the real world and outputting it for further live streaming. | Use a mobile device with a  companion app installed, or any tracking device or mocap array supported by the Live Capture package. |
| **Connection** | A server-like element in the Unity Editor that enables a local network connection and data streaming between the Client Device and the Unity Editor. | Use the [Connections window](ref-window-connections.md) to create and set up a Connection for your Client Device, and then enable the connection on both sides. |
| **Capture Device** | A Unity GameObject with a component that represents the connected Client Device in the Unity Editor. It is responsible for mirroring and updating the connected Client Device status and for feeding the target Actor with the latest data received from the Client Device. | Use the [Take Recorder window](ref-window-take-recorder.md#capture-devices) to create and set up a Capture Device that corresponds to your Client Device. |
| **Actor** | A Unity GameObject with relevant components that allow the Capture Device to drive it according to data received from the Client Device. | Use the Hierarchy, the Inspector, and Live Capture components to create and set up the GameObject you need to drive from your Client Device. |

>**Note:** The type of Connection, Capture Device, and Actor to set up varies according to the Client Device you’re using.

## Examples

To drive a Unity Camera from the [Unity Virtual Camera](virtual-camera.md) app installed on a mobile device, you have to create:

* For the Connection, a [Companion App Server](connection-device.md#companion-app-connection),
* For the Capture Device, a [Virtual Camera Device](ref-component-virtual-camera-device.md), and
* For the Actor, a GameObject including a Camera (or Cinemachine Camera Driver) component and a [Virtual Camera Actor](ref-component-virtual-camera-actor.md) component, among others.

To animate a character face in Unity from the [Unity Face Capture](face-capture.md) app installed on a mobile device, you have to create:

* For the Connection, a [Companion App Server](connection-device.md#companion-app-connection),
* For the Capture Device, an [ARKit Face Device](ref-component-arkit-face-device.md), and
* For the Actor, a GameObject including a rigged character head and an [ARKit Face Actor](ref-component-arkit-face-actor.md) component, among others.

## Additional resources

* [Connection setup](connection.md)
* [Capture Device and Actor setup](capture-device-setup.md)
* [Get started with Virtual Camera setup](virtual-camera-getting-started.md)
* [Get started with Face Capture setup](face-capture-getting-started.md)
