using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.TouchFramework
{
    public class HelpTooltip : BasePopup
    {
        public struct HelpTooltipData
        {
            public string text;
            public string title;
            public RectTransform pointedAtTransform;
            public RectTransform targetTransform;
            public VerticalAlign verticalAlign;
            public HorizontalAlign horizontalAlign;
            public float width;
        }


        TextMeshProUGUI m_TitleField;
        TooltipArrow m_Arrow;
        TooltipPositionConstraint m_PositionConstraint;

        void Awake()
        {
            Initialize();

            var textFields = m_PopUpRect.GetComponentsInChildren<TextMeshProUGUI>();
            Assert.IsTrue(textFields.Length == 2);
            m_TitleField = textFields[0];
            m_TextField = textFields[1];

            m_Arrow = GetComponentInChildren<TooltipArrow>(true);
            Assert.IsNotNull(m_Arrow);

            m_PositionConstraint = GetComponentInChildren<TooltipPositionConstraint>(true);
            Assert.IsNotNull(m_PositionConstraint);
        }

        public void Display(HelpTooltipData data)
        {
            UpdateData(data);
            m_PopUpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, data.width);
            StartAnimation(AnimationIn(m_DefaultFadeDuration));
        }

        public void HideIfNeeded()
        {
            if (m_Container.activeSelf)
            {
                StartAnimation(AnimationOut(m_DefaultFadeDuration));
            }
            else // Abort an eventual animation that would activate the container.
            {
                StopAnimation();
            }
        }

        protected override void OnAnimationInAfterLayout()
        {
            m_PositionConstraint.UpdateTransform();
            m_Arrow.UpdateTransform();
        }

        void UpdateData(HelpTooltipData data)
        {
            m_TitleField.text = data.title;
            m_TextField.text = data.text;
            m_Arrow.pointedAtTransform = data.pointedAtTransform;
            m_PositionConstraint.targetTransform = data.targetTransform;
            m_PositionConstraint.horizontalAlign = data.horizontalAlign;
            m_PositionConstraint.verticalAlign = data.verticalAlign;
        }
    }
}
