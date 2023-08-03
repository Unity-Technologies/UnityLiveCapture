using System.Collections;
using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    interface ICountdownController
    {
        void ToggleRecording();
    }

    class CountdownController : ICountdownController
    {
        [Inject]
        ICoroutineRunner m_Runner;

        [Inject]
        ISettings m_Settings;

        [Inject]
        ICountdownView m_CountdownView;

        [Inject]
        ICompanionAppHost m_CompanionApp;

        [Inject]
        SignalBus m_SignalBus;

        Coroutine m_Coroutine;

        public void ToggleRecording()
        {
            var isPlaying = m_CountdownView.IsPlaying;

            m_CountdownView.Hide();

            if (m_Coroutine != null)
            {
                m_Runner.StopCoroutine(m_Coroutine);
            }

            if (m_CompanionApp.IsRecording)
            {
                m_CompanionApp.StopRecording();
            }
            else if (!isPlaying)
            {
                m_Coroutine = m_Runner.StartCoroutine(RecordCoroutine());
            }
        }

        IEnumerator RecordCoroutine()
        {
            var seconds = m_Settings.CountdownTime;

            if (m_Settings.CountdownEnabled && seconds > 0)
            {
                m_CountdownView.Show();
                m_CountdownView.PlayCountdown(seconds);

                while (m_CountdownView.IsPlaying)
                {
                    yield return null;
                }

                m_CountdownView.Hide();
            }

            m_CompanionApp.StartRecording();
            m_Coroutine = null;
        }
    }
}
