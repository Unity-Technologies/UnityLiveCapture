using System;

namespace Unity.CompanionAppCommon
{
    interface ICountdownView : IDialogView
    {
        event Action countdownCompleted;

        bool IsPlaying { get; }
        void PlayCountdown(int seconds);
        void StopCountdown();
    }
}
