using UnityEngine;
using UnityEngine.UI;

namespace Unity.CompanionAppCommon
{
    public class CustomLayoutGroup : VerticalLayoutGroup
    {
        RectTransform m_ContentTransform;

        RectTransform GetContentTransform()
        {
            if (m_ContentTransform == null)
            {
                m_ContentTransform = transform.Find("Content") as RectTransform;
            }

            return m_ContentTransform;
        }

        public override void SetLayoutVertical()
        {
            var restore = GetContentTransform().anchoredPosition;
            base.SetLayoutVertical();
            GetContentTransform().anchoredPosition = restore;
        }
    }
}
