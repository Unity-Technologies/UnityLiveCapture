using System.Collections;
using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class DimController : IRecordingStateListener
    {
        const float k_DimTime = 1f;
        const float k_ScreenDimBrightness = 0.1f;

        [Inject]
        ICoroutineRunner m_Runner;

        [Inject]
        ISettings m_Settings;

        Coroutine m_Coroutine;
        bool m_IsRecording;

        public void SetRecordingState(bool isRecording)
        {
            m_IsRecording = isRecording;

            if (isRecording && m_Settings.DimScreen)
            {
                StopCoroutine();

                m_Coroutine = m_Runner.StartCoroutine(DimSequence());
            }
        }

        void StopCoroutine()
        {
            if (m_Coroutine != null)
            {
                m_Runner.StopCoroutine(m_Coroutine);
            }
        }

        IEnumerator DimSequence()
        {
            var brightness = GetDeviceBrightness();

            yield return LerpBrightnessCoroutine(brightness, k_ScreenDimBrightness);

            while(m_IsRecording)
            {
                yield return null;
            }

            yield return LerpBrightnessCoroutine(k_ScreenDimBrightness, brightness);

            m_Coroutine = null;
        }

        IEnumerator LerpBrightnessCoroutine(float from , float to)
        {
            var time = 0f;

            while (time <= k_DimTime)
            {
                time += Time.deltaTime;

                var t = time / k_DimTime;

                SetDeviceBrightness(Mathf.Lerp(from, to, t));

                yield return null;
            }

            SetDeviceBrightness(to);
        }

        float GetDeviceBrightness()
        {
            return Screen.brightness;
        }

        void SetDeviceBrightness(float value)
        {
            Screen.brightness = value;
        }
    }
}
