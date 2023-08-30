using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class SpriteSwapToggle : BaseToggle
    {
        [SerializeField]
        Sprite m_OnSprite;
        [SerializeField]
        Sprite m_OffSprite;
        [SerializeField]
        Image m_Icon;

        protected override void UpdateView()
        {
            m_Icon.sprite = IsOn ? m_OnSprite : m_OffSprite;
        }
    }
}
