# Manage Shots in Timeline

Use [Timeline](https://docs.unity3d.com/Packages/com.unity.timeline@latest) to prepare Shots and record Takes in a cinematic context.

**Note:** These scenarios assume you already have a GameObject with a Timeline associated to it.

## Create an empty Shot

1. Select the GameObject that holds your Timeline.

2. In the Timeline window, right click in the left pane and select **Unity.LiveCapture** > **Take Recorder Track**.

3. In the new Take Recorder Track, in the right pane, right click and select **Add Shot**.

   The new Shot appears as a clip in the Take Recorder Track.

4. Adjust the position and duration of the clip according to your recording needs.

## Create a Shot from an existing Take

1. Select the GameObject that holds your Timeline.

2. In the Timeline window, right click in the left pane and select **Unity.LiveCapture** > **Take Recorder Track**.

3. In the new Take Recorder Track, in the right pane, right click and select **Add from Take**.

4. Double-click on the targeted Take in the list.

   The new Shot appears as a clip in the Take Recorder Track. Its duration corresponds to the duration of the Take you selected.

## Record and manage Takes in a Shot

Use the [Take Recorder window](ref-window-take-recorder.md#shot-browser) to manage Shot properties, record a Take in the context of a Shot, or select any existing Take to use for a Shot.
