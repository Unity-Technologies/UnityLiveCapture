using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class Notification : BasePopup
    {
        Image m_Image;

        public struct NotificationData
        {
            public float? displayDuration;
            public float fadeDuration;
            public string text;
            public Sprite icon;
            public bool persistentAlpha;
        }

        public NotificationData DefaultData()
        {
            return new NotificationData
            {
                text = string.Empty,
                icon = null,
                displayDuration = m_DefaultDisplayDuration,
                fadeDuration = m_DefaultFadeDuration,
                persistentAlpha = false
            };
        }

        void Awake()
        {
            Initialize();
            m_Image = m_PopUpRect.Find("Icon").GetComponent<Image>();
        }

        public void Display(NotificationData data)
        {
            if (data.icon != null)
            {
                m_Image.sprite = data.icon;
                m_Image.gameObject.SetActive(true);
            }
            else
                m_Image.gameObject.SetActive(false);

            m_TextField.text = data.text;

            var fromAlpha = 0f;
            if (data.persistentAlpha)
            {
                switch (m_AnimationPhase)
                {
                    case AnimationPhase.None:
                        break;
                    case AnimationPhase.FadeIn:
                        return;
                    case AnimationPhase.Display:
                    case AnimationPhase.FadeOut:
                        fromAlpha = m_CanvasGroup.alpha;
                        break;
                }
            }

            StartAnimation(AnimationInOut(data.displayDuration, data.fadeDuration, fromAlpha));
        }
    }
}
