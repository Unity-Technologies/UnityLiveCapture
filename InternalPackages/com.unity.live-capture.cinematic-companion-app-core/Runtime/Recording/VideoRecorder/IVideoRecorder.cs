namespace Unity.CompanionAppCommon
{
    interface IVideoRecorder
    {
        bool IsRecording { get; }
        bool IncludeAudio { get; set; }

        void Update();
        void StartRecording(string directory, string fileName);
        void StopRecording();
    }
}
