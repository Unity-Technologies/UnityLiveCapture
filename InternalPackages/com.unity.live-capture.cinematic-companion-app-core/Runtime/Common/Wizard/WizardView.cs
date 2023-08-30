using System;
using Unity.TouchFramework;
using UnityEngine;
using Zenject;

namespace Unity.CompanionAppCommon
{
    class WizardView : DialogView, IWizardView, IInitializable, IDisposable
    {
        public event Action SkipClicked = delegate {};
        public event Action NextClicked = delegate {};

        [SerializeField]
        SimpleButton m_SkipButton;

        [SerializeField]
        SimpleButton m_NextButton;

        public virtual void Initialize()
        {
            m_SkipButton.onClick += OnSkipButtonClicked;
            m_NextButton.onClick += OnNextButtonClicked;
        }

        public virtual void Dispose()
        {
            m_SkipButton.onClick -= OnSkipButtonClicked;
            m_NextButton.onClick -= OnNextButtonClicked;
        }

        void OnSkipButtonClicked()
        {
            SkipClicked.Invoke();
        }

        void OnNextButtonClicked()
        {
            NextClicked.Invoke();
        }
    }
}
