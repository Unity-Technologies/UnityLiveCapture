using System;
using UnityEngine;
using Unity.LiveCapture;
using Unity.LiveCapture.CompanionApp;

namespace Unity.CompanionAppCommon
{
    interface ISessionStateListener
    {
        void SetSessionState(bool active);
    }

    interface IDeviceModeListener
    {
        void SetDeviceMode(DeviceMode mode);
    }

    interface IRecordingStateListener
    {
        void SetRecordingState(bool isRecording);
    }

    interface IPlayStateListener
    {
        void SetPlayState(bool value);
    }

    interface ITimeListener
    {
        void SetTime(double time, double duration, FrameRate frameRate);
    }

    interface ITakeDescriptorsListener
    {
        void SetTakeDescriptors(TakeDescriptor[] descriptors);
    }

    interface ITakeSelectionListener
    {
        void SetSelectedTake(int selected);
    }

    interface ISlateIterationBaseListener
    {
        void SetSlateIterationBase(int iterationBase);
    }

    interface ISlateTakeNumberListener
    {
        void SetSlateTakeNumber(int takeNumber);
    }

    interface ISlateShotNameListener
    {
        void SetSlateShotName(string shotName);
    }

    interface ITexturePreviewListener
    {
        void SetTexturePreview(Guid guid, Texture2D texture);
    }
}
