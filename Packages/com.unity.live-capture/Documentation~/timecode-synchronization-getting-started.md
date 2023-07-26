# Getting started with timecode synchronization

This page gives the instructions to add a Timecode Synchronizer to a pre-configured Live Capture environment in order to get a perfect synchronization of your connected data source outputs.

## Scenario assumptions and requirements

This scenario assumes that you have already:
* Installed the [Live Capture package](installation.md) and any of the available companion apps on one or multiple mobile devices.
* [Connected](connection-device.md) all mobile devices to the Unity Editor.
* Set up a Take Recorder and created the necessary elements to get all companion apps ready for recording takes (see instructions for [Virtual Camera](virtual-camera-getting-started.md) and/or [Face Capture](face-capture-getting-started.md))

In addition, this scenario specifically requires:
* A [Tentacle Sync](https://tentaclesync.com/sync-e) timecode generator.
* An audio device capable of providing a [Linear Time Code (LTC)](https://en.wikipedia.org/wiki/Linear_timecode) signal.

**Note:** This scenario uses the Tentacle Sync as a slave of the LTC audio device, for relaying the timecode to the mobile apps. The LTC audio device is the master timecode source.


## Connect and set up the hardware

1. Connect the LTC audio device to a microphone or line-in port of the Unity Editor workstation.

2. Connect the LTC audio device to the input port of the Tentacle Sync timecode generator.

3. Make sure to have the LTC audio source and the Tentacle Sync timecode generator turned on and properly configured according to their expected master/slave relationship (see their respective documentations).

4. On all your mobile devices, enable Bluetooth.


## Set up the Timecode Synchronizer

1. In the Unity Editor, in the Hierarchy, right click in your current Scene and select **Live Capture > Timecode Synchronizer**.
   <br />This creates a **Timecode Synchronizer** GameObject containing a **Timecode Synchronizer** component.

2. Add an **LTC Timecode Source** component to this same GameObject.


## Select the timecode sources

1. In the Unity Editor:

   a. In the Timecode Synchronizer component, set the **Timecode Source** to **LTC (Timecode Synchronizer)**.

   b. In the LTC Timecode Source component, [adjust the settings](ref-component-ltc-timecode-source.md) according to the connected audio device and LTC signal.

2. In _each mobile device_:

   a. In the companion app, select the gear icon at the top left.

   b. In **Settings**, under **Timecode**, set the **Source** to **Tentacle Sync**.


## Synchronize the data sources

1. In the Unity Editor, in the Timecode Synchronizer component, click on the **+** (plus) button and select a data source.
   <br />**Note:** The data source selection list should include all your current connected companion apps.

2. Repeat the previous step for all your available data sources.

3. Click on the Calibrate button.

   All your data sources should now be synchronized.

   To fine-tune the synchronization settings and understand the displayed statuses, see the [Timecode Synchronizer component](ref-component-timecode-synchronizer.md) description.
