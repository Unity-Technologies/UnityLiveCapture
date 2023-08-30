using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.TouchFramework
{
    public class BasePopup : MonoBehaviour
    {
        protected enum AnimationPhase
        {
            None,
            FadeIn,
            Display,
            FadeOut
        }

#pragma warning disable CS0649
        [SerializeField]
        protected GameObject m_Container;
#pragma warning restore CS0649

        public event Action AnimationOutCompleted = delegate {};

        protected float m_DefaultDisplayDuration = 4f;
        protected float m_DefaultFadeDuration = UIConfig.dialogFadeTime;
        protected TextMeshProUGUI m_TextField;
        protected RectTransform m_PopUpRect;
        protected Rect m_WorldSpaceScreenRect;
        protected Coroutine m_Animation;
        protected AnimationPhase m_AnimationPhase;
        protected CanvasGroup m_CanvasGroup;

        protected void Initialize()
        {
            m_PopUpRect = m_Container.GetComponent<RectTransform>();
            m_CanvasGroup = m_Container.GetComponent<CanvasGroup>();
            m_TextField = m_PopUpRect.GetComponentInChildren<TextMeshProUGUI>();

            var canvas = GetComponentInParent<Canvas>();
            Assert.IsTrue(canvas.renderMode == RenderMode.ScreenSpaceOverlay,
                "PopUps expect a screen-space-overlay canvas");

            HideImmediate();
        }

        protected virtual void OnEnable()
        {
            m_CanvasGroup.alpha = 0;
            // Canvas is assumed to be static. Update screen rect if that changes.
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_PopUpRect, Vector2.zero, null, out var cornerMin);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_PopUpRect,
                new Vector2(Screen.width, Screen.height), null, out var cornerMax);
            m_WorldSpaceScreenRect = Rect.MinMaxRect(cornerMin.x, cornerMin.y, cornerMax.x, cornerMax.y);
        }

        public void HideImmediate()
        {
            StopAnimation();
            m_Container.SetActive(false);
        }

        protected void StartAnimation(IEnumerator animation)
        {
            StopAnimation();
            m_Animation = StartCoroutine(animation);
        }

        protected void StopAnimation()
        {
            if (m_Animation != null)
            {
                StopCoroutine(m_Animation);
                m_Animation = null;
            }
            m_AnimationPhase = AnimationPhase.None;
        }

        protected IEnumerator AnimationInOut(float? displayDuration, float fadeDuration, float fromAlpha = 0f)
        {
            yield return AnimationIn(fadeDuration, fromAlpha);
            m_AnimationPhase = AnimationPhase.Display;
            if (displayDuration.HasValue)
            {
                yield return new WaitForSeconds(displayDuration.Value);
                yield return AnimationOut(fadeDuration);
            }
        }

        protected IEnumerator AnimationIn(float duration, float fromAlpha = 0f)
        {
            m_AnimationPhase = AnimationPhase.FadeIn;

            m_Container.SetActive(true);
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.alpha = fromAlpha;
            // Wait for a layout pass before updating position so that arrow can be placed properly.
            yield return null;
            OnAnimationInAfterLayout();
            yield return FadeAnimation(m_CanvasGroup, fromAlpha, 1f, duration);
            m_CanvasGroup.interactable = true;

            m_AnimationPhase = AnimationPhase.None;
        }

        protected IEnumerator AnimationOut(float duration)
        {
            m_AnimationPhase = AnimationPhase.FadeOut;

            m_CanvasGroup.interactable = false;
            yield return FadeAnimation(m_CanvasGroup, 1, 0, duration);
            HideImmediate();
            AnimationOutCompleted.Invoke();

            m_AnimationPhase = AnimationPhase.None;
        }

        protected virtual void OnAnimationInAfterLayout() {}

        protected IEnumerator FadeAnimation(CanvasGroup group, float from, float to, float duration)
        {
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                group.alpha = Mathf.Lerp(from, to, normalizedTime);
            }
        }
    }
}
