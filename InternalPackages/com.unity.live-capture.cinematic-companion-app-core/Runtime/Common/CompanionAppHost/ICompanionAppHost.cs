using System;
using Unity.LiveCapture;
using Unity.LiveCapture.CompanionApp;

namespace Unity.CompanionAppCommon
{
    interface ICompanionAppHost
    {
        bool IsSessionActive { get; }
        bool IsRecording { get; }
        DeviceMode DeviceMode { get; }
        FrameRate FrameRate { get; }
        bool HasSlate { get; }
        double SlateDuration { get; }
        bool SlateIsPlaying { get; }
        double SlatePreviewTime { get; }
        int SlateTakeNumber { get; }
        TakeDescriptor[] SlateTakes { get; }
        int SlateSelectedTake { get; }
        int SlateIterationBase { get; }
        string NextTakeName { get; }
        string NextAssetName { get; }

        void StartRecording();
        void StopRecording();
        void SetDeviceMode(DeviceMode value);
        void SetPlaybackTime(double value);
        void StartPlayback();
        void PausePlayback();
        void SelectTake(Guid guid);
        void SetIterationBase(Guid guid);
        void ClearIterationBase();
        void DeleteTake(Guid guid);
        void UpdateTake(TakeDescriptor descriptor);
        void RequestTexturePreview(Guid guid);
    }
}
