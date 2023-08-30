using UnityEngine;

namespace Unity.LiveCapture
{
    interface ITakeRecorderContext
    {
        int Version { get; }
        int Selection { get; set; }
        Shot[] Shots { get; }

        void SetShot(int index, Shot shot);
        Object GetStorage(int index);

        void Update();
        bool IsValid();
        void Play();
        bool IsPlaying();
        void Pause();
        double GetTime();
        double GetDuration();
        void SetTime(double value);
        IExposedPropertyTable GetResolver(int index);
        void ClearSceneBindings(int index);
        void SetSceneBindings(int index);
        void Rebuild(int index);
    }
}
