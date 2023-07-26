using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    [Serializable]
    public class ControlTransition
    {
        public enum Type
        {
            SetColor,
            SpriteSwap,
        }

        public enum State
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled,
        }

        [SerializeField]
        Graphic m_TargetGraphic;

        [SerializeField]
        ColorBlock m_ColorBlock;

        [SerializeField]
        SpriteState m_SpriteState;
    }
}
