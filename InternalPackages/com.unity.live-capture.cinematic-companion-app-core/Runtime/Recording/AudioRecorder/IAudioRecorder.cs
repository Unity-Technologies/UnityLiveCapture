namespace Unity.CompanionAppCommon
{
    interface IAudioRecorder
    {
        bool IsRecording { get; }

        void Update();
        void StartRecording(string directory, string fileName);
        void StopRecording();
    }
}
