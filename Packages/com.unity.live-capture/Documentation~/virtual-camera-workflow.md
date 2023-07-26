# Virtual Camera workflows

## Camera initial position

Position and orient the camera actor in the scene to configure the starting position and rotation (only accounts for y-rotation) of the virtual camera.

## Record and play back takes from the app

You can [record, play back, and manage your takes](virtual-camera-record-play-takes.md) directly from the Virtual Camera app.

## Anchoring/Platforming

To anchor the camera to moving object (e.g. dolly cart, car, boat, etc.), parent the Actor GameObject in the scene to the moving object.

## Control camera settings from the editor

All camera state can be controlled from the editor. This is useful if you want to have an "operator" workflow where the individual with the device only has to worry about controlling the camera.

## Save and load Virtual Camera Device settings

Easily save and load the settings in your **Virtual Camera Device** (e.g. damping, lens preset, etc.) using Unity's **[Presets](https://docs.unity3d.com/Manual/Presets.html)** system.

## Snapshot the Virtual Camera state

Use [Snapshots](virtual-camera-snapshots.md) to capture the current state of your Virtual Camera and reuse it later.
