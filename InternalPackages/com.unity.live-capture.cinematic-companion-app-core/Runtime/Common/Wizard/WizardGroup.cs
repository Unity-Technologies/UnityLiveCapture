using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Unity.CompanionAppCommon
{
    sealed class WizardSignals
    {
        public class Open {}
        public class Skip {}
        public class Next {}
    }

    class WizardGroup : IInitializable, IDisposable
    {
        [Inject]
        List<IWizardView> m_WizardViews;

        [Inject]
        SignalBus m_SignalBus;

        int ViewCount => m_WizardViews.Count;

        int m_StepIndex;

        public void Initialize()
        {
            m_SignalBus.Subscribe<WizardSignals.Open>(OnOpenWizard);
            m_SignalBus.Subscribe<WizardSignals.Skip>(OnSkipClicked);
            m_SignalBus.Subscribe<WizardSignals.Next>(OnNextClicked);

            if (!WizardPrefs.WasShown)
            {
                OnOpenWizard();
            }
        }

        public void Dispose()
        {
            m_SignalBus.Unsubscribe<WizardSignals.Open>(OnOpenWizard);
            m_SignalBus.Unsubscribe<WizardSignals.Skip>(OnSkipClicked);
            m_SignalBus.Unsubscribe<WizardSignals.Next>(OnNextClicked);
        }

        void OnOpenWizard()
        {
            m_StepIndex = 0;
            ShowView(m_StepIndex);
        }

        void HideViews()
        {
            foreach (var view in m_WizardViews)
            {
                view.Hide();
            }
        }

        void ShowView(int index)
        {
            HideViews();
            m_WizardViews[index].Show();
        }

        void OnSkipClicked()
        {
            HideViews();
            WizardPrefs.WasShown = true;
        }

        void OnNextClicked()
        {
            m_StepIndex++;
            if (m_StepIndex < ViewCount)
            {
                ShowView(m_StepIndex);
            }
            else
            {
                HideViews();
                WizardPrefs.WasShown = true;
            }
        }
    }
}
