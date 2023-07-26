# Unity Virtual Camera features

## Preview, recording, and playback

* Live preview via **video streaming** between the Unity Editor and the mobile app.

* **Editor viewport** to see the virtual camera's view from the editor workstation.

* Video streaming support for Unity's built-in, URP and HDRP render pipelines.

* **Record camera performances** to animation clips and play back the results.

* Works in both Edit mode and Play mode in the Unity Editor.

## Physical camera controls

* Bi-directional control of camera settings from either the Unity Editor or the mobile app.

* Control **focal length**, **focus distance** and **aperture** with configurable damping for a smoother feel.

* **Reticle Autofocus (AF)** mode to automatically set the focus distance based on a screen reticle.

* **Tracking Autofocus (AF)** mode to have focus distance dynamically match a scene object's distance to the camera.

## Camera motion tracking

* Camera tracking via Apple's ARKit.

* Additional camera motion controls via **virtual joysticks**.

* Temporarily halt tracking and reposition around the physical space.

* **Motion scaling** to multiply physical motion.

## Dolly simulation and ergonomics

* **Axis locking** to prevent motion in any combination of 6 degrees of movement.

* Camera **motion damping**.

* Steadicam simulation using Cinemachine.

## Workflows

* Non-destructive and iterative workflow – change camera settings such as damping and focal length after a capture session.

* [**Snapshots** system](virtual-camera-snapshots.md) to capture the current state of your Virtual Camera and reuse it later. This includes, for example, the position of the camera in the Scene, and other metadata such as the lens information.

  >**Note:** The Snapshots feature is only available from the Virtual Camera Device in the Unity Editor. There is currently no interface to manage Snapshots from the Virtual Camera app.

## Render pipeline compatibility

  | **Feature** | **Built-in Render Pipeline** | **Universal Render Pipeline (URP)** | **High Definition Render Pipeline (HDRP)** |
  | :--- | :--- | :--- | :--- |
  | **Depth Of Field**  | Yes (1, 2)  | Yes (2) | Yes |
  | **Film Format** | Yes | Yes | Yes |
  | **Focus Plane** | Yes | Yes | Yes |
  | **Video Streaming** | Yes | Yes | Yes |

_(1) To benefit from this feature, you need to have the [Post-Processing Stack V2](https://docs.unity3d.com/Packages/com.unity.postprocessing@latest) package installed.<br />
(2) Only supported when using a basic Virtual Camera Actor (i.e. not with Cinemachine)._
