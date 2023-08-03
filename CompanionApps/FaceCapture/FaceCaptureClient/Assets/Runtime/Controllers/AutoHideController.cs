using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.CompanionAppCommon;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class AutoHideController : IInitializable
        , IDisposable
        , ISettingsPropertyListener
        , IRecordingStateListener
        , ILateTickable
    {
        public interface IHideable
        {
            float Alpha { get; set; }
            bool Interactable { get; set; }
            bool BlocksRaycasts { get; set; }
        }

        const float k_DelayTime = 3f;
        const float k_FaceTime = 1f;

        [Inject]
        ICoroutineRunner m_Runner;

        [Inject]
        ISettings m_Settings;

        [Inject]
        IMainView m_MainView;

        [Inject]
        ICalibrationView m_CalibrationView;

        [Inject]
        List<IHideable> m_Hideables;

        [Inject]
        SignalBus m_SignalBus;

        Coroutine m_Coroutine;

        public void Initialize()
        {
            m_SignalBus.Subscribe<BackgroundEventsSignals.Clicked>(OnBackgroundClicked);
            m_SignalBus.Subscribe<BackgroundEventsSignals.Touched>(OnBackgroundTouched);

            m_MainView.VisibilityChanged += OnMainViewVisibilityChanged;
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<BackgroundEventsSignals.Clicked>(OnBackgroundClicked);
            m_SignalBus.Unsubscribe<BackgroundEventsSignals.Touched>(OnBackgroundTouched);

            m_MainView.VisibilityChanged -= OnMainViewVisibilityChanged;
        }

        public void LateTick()
        {
            if (m_CalibrationView.ViewState == ICalibrationView.State.Calibrating)
            {
                RestartAutoHide();
            }
        }

        public void SetRecordingState(bool isRecording)
        {
            RestartAutoHide();
        }

        public void SettingsPropertyChanged(SettingsProperty property, ISettings settings)
        {
            if (property == SettingsProperty.AutoHideUI)
            {
                RestartAutoHide();
            }
        }

        void OnMainViewVisibilityChanged(bool isVisible)
        {
            RestartAutoHide();
        }

        void OnBackgroundClicked(BackgroundEventsSignals.Clicked signal)
        {
            RestartAutoHide();
        }

        void OnBackgroundTouched(BackgroundEventsSignals.Touched signal)
        {
            RestartAutoHide();
        }

        void RestartAutoHide()
        {
            StopCoroutine();

            foreach (var hideable in m_Hideables)
            {
                hideable.Alpha = 1f;
                hideable.BlocksRaycasts = true;
                hideable.Interactable = true;
            }

            if (m_MainView.IsShown && m_Settings.AutoHideUI)
            {
                m_Coroutine = m_Runner.StartCoroutine(AutoHideSequence());
            }
        }

        void StopCoroutine()
        {
            if (m_Coroutine != null && m_Runner != null)
            {
                m_Runner.StopCoroutine(m_Coroutine);
            }
        }

        IEnumerator AutoHideSequence()
        {
            yield return new WaitForSeconds(k_DelayTime);

            foreach (var hideable in m_Hideables)
            {
                hideable.BlocksRaycasts = false;
                hideable.Interactable = false;
            }

            yield return LerpAlpha(1f, 0f);

            m_Coroutine = null;
        }

        IEnumerator LerpAlpha(float from , float to)
        {
            var time = 0f;

            while (time <= k_FaceTime)
            {
                time += Time.deltaTime;

                var t = time / k_FaceTime;
                var a = Mathf.Lerp(from, to, t);

                foreach (var hideable in m_Hideables)
                {
                    hideable.Alpha = a;
                }

                yield return null;
            }

            foreach (var hideable in m_Hideables)
            {
                hideable.Alpha = to;
            }
        }
    }
}
