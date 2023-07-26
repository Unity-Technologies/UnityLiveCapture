using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    /// <summary>
    /// Component for handling the visibility and interactivity of a window represented by a <see cref="Canvas"/>.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster))]
    public class DialogWindow : MonoBehaviour, IDialog
    {
        enum TransitionType
        {
            Fade,
            Slide,
        }

#pragma warning disable 649
        [SerializeField]
        TransitionType m_TransitionType = TransitionType.Fade;

        [SerializeField]
        RectTransform m_TranslateRect;

        [SerializeField]
        bool m_OverrideTransitionDuration;

        [SerializeField]
        float m_TransitionDuration = 0.3f;
#pragma warning restore 649

        bool m_Open;
        Canvas m_Canvas;
        CanvasGroup m_CanvasGroup;
        FloatTween m_OpenDialogTween;
        FloatTween m_CloseDialogTween;
        TweenRunner<FloatTween> m_TweenRunner;
        ScrollRect m_ScrollRect;

        public bool open => m_Open;

        public UnityEvent dialogClose { get; } = new UnityEvent();

        public UnityEvent dialogOpen { get; } = new UnityEvent();

        public void Open(bool instant = false, bool setInteractable = true, float fadeTime = -1f)
        {
            if (!m_Open)
                dialogOpen.Invoke();

            m_Open = true;

            var fadeDuration = UIConfig.dialogFadeTime;

            if (m_OverrideTransitionDuration)
                fadeDuration = m_TransitionDuration;
            else if (fadeTime > 0f)
                fadeDuration = fadeTime;

            if (instant || !Application.isPlaying)
            {
                OpenTransitionCompletedHandler();
            }
            else
            {
                SetInteractable(setInteractable);

                m_OpenDialogTween.duration = fadeDuration;

                if (m_TransitionType == TransitionType.Fade)
                {
                    m_OpenDialogTween.startValue = m_CanvasGroup.alpha;
                    m_OpenDialogTween.targetValue = 1f;
                }
                else
                {
                    m_OpenDialogTween.startValue = m_TranslateRect.anchoredPosition.y;
                    m_OpenDialogTween.targetValue = 0f;
                }

                m_TweenRunner.StartTween(m_OpenDialogTween, EaseType.EaseInCubic);
            }
        }

        public void Close(bool instant = false, float fadeTime = -1f)
        {
            if (m_Open)
                dialogClose.Invoke();

            m_Open = false;

            SetInteractable(false);

            var fadeDuration = UIConfig.dialogFadeTime;

            if (m_OverrideTransitionDuration)
                fadeDuration = m_TransitionDuration;
            else if (fadeTime > 0f)
                fadeDuration = fadeTime;

            if (instant || !Application.isPlaying)
            {
                CloseTransitionCompleteHandler();
            }
            else
            {
                m_CloseDialogTween.duration = fadeDuration;

                if (m_TransitionType == TransitionType.Fade)
                {
                    m_CloseDialogTween.startValue = m_CanvasGroup.alpha;
                    m_CloseDialogTween.targetValue = 0f;
                }
                else
                {
                    m_CloseDialogTween.startValue = m_TranslateRect.anchoredPosition.y;
                    m_CloseDialogTween.targetValue = -1f * Screen.height;
                }

                m_TweenRunner.StartTween(m_CloseDialogTween, EaseType.EaseInCubic);
            }
        }

        /// <summary>
        /// Set the window to be interactable, meaning that it will block raycasts and UI objects inside of the window
        /// will generate UI events. This is useful if you want to set a window to not be interactable during a
        /// transition.
        ///
        /// Note that this does not change visibility of the canvas.
        /// </summary>
        /// <param name="interactable"></param>
        public void SetInteractable(bool interactable)
        {
            if (interactable && !m_Open)
                Debug.LogWarning("You are setting a window to be interactable but it is not visible.",
                    gameObject);

            if (m_ScrollRect != null)
                m_ScrollRect.enabled = interactable;

            m_CanvasGroup.interactable = interactable;
            m_CanvasGroup.blocksRaycasts = interactable;
        }

        void SetVisible(bool visible)
        {
            m_Canvas.enabled = visible;
            m_CanvasGroup.alpha = visible ? 1f : 0f;
        }

        void Awake()
        {
            m_Canvas = GetComponent<Canvas>();
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_ScrollRect = GetComponent<ScrollRect>();

            m_OpenDialogTween = new FloatTween()
            {
                ignoreTimeScale = true
            };
            m_OpenDialogTween.AddOnChangedCallback(TweenValueChangedHandler);
            m_OpenDialogTween.AddOnCompleteCallback(OpenTransitionCompletedHandler);

            m_CloseDialogTween = new FloatTween()
            {
                ignoreTimeScale = true
            };
            m_CloseDialogTween.AddOnChangedCallback(TweenValueChangedHandler);
            m_CloseDialogTween.AddOnCompleteCallback(CloseTransitionCompleteHandler);

            m_TweenRunner = new TweenRunner<FloatTween>();
            m_TweenRunner.Init(this);

            m_Open = m_Canvas.enabled;
        }

        void TweenValueChangedHandler(float alpha)
        {
            switch (m_TransitionType)
            {
                case TransitionType.Fade:
                    m_CanvasGroup.alpha = alpha;
                    break;
                case TransitionType.Slide:
                {
                    var pos = m_TranslateRect.anchoredPosition;
                    m_TranslateRect.anchoredPosition = new Vector2(pos.x, alpha);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OpenTransitionCompletedHandler()
        {
            SetInteractable(true);

            switch (m_TransitionType)
            {
                case TransitionType.Fade:
                    SetVisible(true);
                    break;
                case TransitionType.Slide:
                {
                    var pos = m_TranslateRect.anchoredPosition;
                    m_TranslateRect.anchoredPosition = new Vector2(pos.x, 0f);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void CloseTransitionCompleteHandler()
        {
            SetInteractable(false);

            switch (m_TransitionType)
            {
                case TransitionType.Fade:
                    SetVisible(false);
                    break;
                case TransitionType.Slide:
                {
                    var pos = m_TranslateRect.anchoredPosition;
                    m_TranslateRect.anchoredPosition = new Vector2(pos.x, -1f * Screen.height);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
