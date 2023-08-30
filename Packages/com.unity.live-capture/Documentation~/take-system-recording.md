# Recording and playback

Record and play back takes from the Unity Editor for any connected Client Devices.

>**Important:** Before you start, you must
* [Connect at least one Client Device](connection.md) to the Unity Editor
* [Set up a Capture Device and an Actor](capture-device-setup.md) for it.
* Create a Shot either [in Timeline](take-system-shots-in-timeline.md) or [in a Shot Library](take-system-shots-in-library.md).

## Record a take

To record a take from the Unity Editor:

1. From the Unity Editor menu, select **Window** > **Live Capture** > **Take Recorder**.

3. In the top left pane, browse and select a Shot to record the Take in.

2. In the **Take Recorder** window, enable the **Live** mode.

4. Click **Start Recording**.  

   The recording starts immediately, without a pre-recording countdown.

5. To stop the recording, click **Stop Recording**.

>**Note:** When you record a Take in a [Take Recorder clip in Timeline](take-system-shots-in-timeline.md), you can position the playhead to start the Take recording after the clip's start and manually stop the recording before the clip's end. The boundaries of the actual recorded Take within the Shot are indicated with yellow brackets.

## Play back a recorded take

To play back a take from the Unity Editor:

1. From the Unity Editor menu, select **Window** > **Live Capture** > **Take Recorder**.

3. In the top left pane, browse and select a Shot that contains the Take you want to play.

2. In the **Take Recorder** window, disable the **Live** mode.  

   **Note:** If you play back a take with the [Live mode enabled](#live-mode), you won't be able to see the animation recorded by the active devices.

4. In the **Takes** list, select the take.  

   **Note:** The last recorded take is selected by default.

5. Click **Start Preview**.

## Live Mode

When Live mode is enabled, active devices drive their associated actors in the scene.

It's possible to playback takes while in Live mode, in which case active devices will drive their actors instead of the recorded data. This could be useful for testing or rehearsing, but in a typical production context, you should disable Live mode during playback to see the actual animations you recorded through your capture devices.

## Related topics

* [Take Recorder component reference](ref-window-take-recorder.md)
* [Record and play takes from the Virtual Camera app](virtual-camera-record-play-takes.md)
* [Record takes from the Face Capture app](face-capture-record-takes.md)
