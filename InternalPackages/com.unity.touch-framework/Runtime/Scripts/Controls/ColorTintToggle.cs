using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class ColorTintToggle : BaseToggle
    {
        [SerializeField]
        Color m_OffColor;
        [SerializeField]
        Color m_OnColor;
        [SerializeField]
        Image m_Background;

        protected override void UpdateView()
        {
            m_Background.color = IsOn ? m_OnColor : m_OffColor;
        }
    }
}
