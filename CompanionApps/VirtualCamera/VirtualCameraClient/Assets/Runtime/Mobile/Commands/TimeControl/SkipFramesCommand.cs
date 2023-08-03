using Unity.CompanionAppCommon;
using Unity.LiveCapture;
using UnityEngine;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class SkipFramesCommand
    {
        [Inject]
        ICompanionAppHost m_CompanionApp;

        public void Execute(SkipFramesSignal skipFramesSignal)
        {
            var framesToSkip = skipFramesSignal.value;

            var skipFrameTime = FrameTime.FromFrameTime(framesToSkip);
            var frameTime = FrameTime.FromSeconds(m_CompanionApp.FrameRate, m_CompanionApp.SlatePreviewTime);

            frameTime = frameTime.Round();
            frameTime += skipFrameTime;

            var value = (float)frameTime.ToSeconds(m_CompanionApp.FrameRate);
            value = Mathf.Max(0f, value);

            m_CompanionApp.SetPlaybackTime(value);
        }
    }
}
