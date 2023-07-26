using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Unity.CompanionAppCommon
{
    class CountdownView : DialogView, ICountdownView
    {
        const float k_LabelMaxAlpha = 0.5f;

        public event Action countdownCompleted = delegate {};

        [SerializeField]
        Image m_CountdownRadialImage;

        TextMeshProUGUI m_SecondsLabel;
        AudioSource m_AudioSource;
        Coroutine m_Coroutine;

        public bool IsPlaying { get => m_Coroutine != null; }

        void Awake()
        {
            m_SecondsLabel = GetComponentInChildren<TextMeshProUGUI>();
            m_AudioSource = GetComponentInChildren<AudioSource>();
        }

        void OnDisable()
        {
            StopCountdown();
        }

        public void PlayCountdown(int seconds)
        {
            StopCountdown();

            m_Coroutine = StartCoroutine(CountdownCoroutine(seconds));
        }

        public void StopCountdown()
        {
            if (IsPlaying)
            {
                StopCoroutine(m_Coroutine);

                m_Coroutine = null;
            }
        }

        protected override void OnShowChanged()
        {
            StopCountdown();
        }

        IEnumerator CountdownCoroutine(int seconds)
        {
            m_CountdownRadialImage.fillAmount = 0f;
            m_SecondsLabel.alpha = k_LabelMaxAlpha;
            m_SecondsLabel.text = seconds.ToString();
            m_AudioSource.Play();

            var elapsedTime = 0f;
            var alpha = 0f;
            var displayedNumber = seconds;

            while (alpha <= 1f)
            {
                var secondsRoundedUp = Mathf.CeilToInt(seconds - elapsedTime);

                if (displayedNumber != secondsRoundedUp)
                {
                    displayedNumber = secondsRoundedUp;
                    m_SecondsLabel.text = displayedNumber.ToString();
                    m_AudioSource.Play();
                }

                var perSecondAlpha = elapsedTime - Mathf.Floor(elapsedTime);

                m_CountdownRadialImage.fillAmount = perSecondAlpha;
                m_SecondsLabel.alpha = k_LabelMaxAlpha * (1f - perSecondAlpha);

                elapsedTime += Time.unscaledDeltaTime;
                alpha = elapsedTime / seconds;

                yield return null;
            }

            m_Coroutine = null;

            countdownCompleted.Invoke();
        }
    }
}
