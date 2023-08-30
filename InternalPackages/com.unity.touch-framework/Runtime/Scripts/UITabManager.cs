using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Manages the selection of and transition between multiple tabs contained in a parent window. Activated tabs will
    /// have their position moved or animated to the the x-center of the container, and the container will resize to fit
    /// the tab contents' vertical height.
    ///
    /// Note that the Property Control does not automatically drive the active tab;
    /// it should be driven from another class. Also, the
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UITabManager : MonoBehaviour
    {
        const float k_MoveWindowTransitionDuration = 0.2f;

#pragma warning disable CS0649
        [SerializeField]
        int m_ActiveTabIndex;
        [Tooltip("The panel holding the tabs.")]
        [SerializeField]
        RectTransform m_DialogContainer;
        [Tooltip("The list of tabs to cycle through.")]
        [SerializeField]
        List<DialogWindow> m_NestedDialogs = new List<DialogWindow>();
#pragma warning restore CS0649

        RectTransform m_RectTransform;
        DialogWindow m_ActiveDialog;
        FloatTween m_MoveWindowTween;
        TweenRunner<FloatTween> m_MoveWindowTweenRunner;
        float m_StartXPos;

        public int activeTabIndex => m_ActiveTabIndex;

        /// <summary>
        /// Set the active tab using the tab index.
        /// </summary>
        /// <param name="tabIndex">The index of the tab to set as active.</param>
        /// <param name="instant">If true, skips animated transition and opens instantly.</param>
        public void SetActiveTab(int tabIndex, bool instant = false)
        {
            if (tabIndex >= m_NestedDialogs.Count)
            {
                Debug.LogError("You are trying to set a tab index for a tab dialog that doesn't exist.",
                    gameObject);
                return;
            }

            m_ActiveTabIndex = tabIndex;
            var tab = m_NestedDialogs[activeTabIndex];

            if (tab == m_ActiveDialog)
                return;

            m_StartXPos = m_DialogContainer.anchoredPosition.x;

            // Prevent interaction on dialog while moving
            foreach (var dialog in m_NestedDialogs)
                dialog.SetInteractable(false);

            // Prevent interaction on dialog while moving
            m_ActiveDialog = tab;
            m_ActiveDialog.Open(true, false);

            if (!instant && Application.isPlaying)
            {
                m_MoveWindowTweenRunner.StartTween(m_MoveWindowTween);
            }
            else
            {
                OnMoveTab(1f);
                OnMoveTransitionComplete();
            }
        }

        void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();

            m_MoveWindowTween = new FloatTween()
            {
                duration = k_MoveWindowTransitionDuration,
                ignoreTimeScale = true,
                startValue = 0f,
                targetValue = 1f
            };
            m_MoveWindowTween.AddOnChangedCallback(OnMoveTab);
            m_MoveWindowTween.AddOnCompleteCallback(OnMoveTransitionComplete);
            m_MoveWindowTweenRunner = new TweenRunner<FloatTween>();
            m_MoveWindowTweenRunner.Init(this);
        }

        void OnMoveTransitionComplete()
        {
            // Only set interactable when transition is complete
            m_ActiveDialog.SetInteractable(true);

            // Hide other tabs
            foreach (var dialog in m_NestedDialogs)
            {
                if (dialog != m_ActiveDialog)
                    dialog.Close(true);
            }
        }

        void OnMoveTab(float alpha)
        {
            var tabIndex = m_NestedDialogs.IndexOf(m_ActiveDialog);
            var newXPos = Mathf.Lerp(m_StartXPos, -1f * tabIndex * m_RectTransform.sizeDelta.x, alpha);
            m_DialogContainer.anchoredPosition = new Vector2(newXPos, m_DialogContainer.anchoredPosition.y);
        }

        void OnValidate()
        {
            m_ActiveTabIndex = Mathf.Clamp(activeTabIndex, 0, m_NestedDialogs.Count - 1);
        }
    }
}
