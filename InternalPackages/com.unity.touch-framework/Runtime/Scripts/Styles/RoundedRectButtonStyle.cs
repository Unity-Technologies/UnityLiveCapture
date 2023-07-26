using System;
using UnityEngine;

namespace Unity.TouchFramework
{
    [CreateAssetMenu(menuName = "UI/TouchFramework/RoundedRectButtonStyle")]
    public class RoundedRectButtonStyle : ScriptableObject
    {
        [Serializable]
        public struct Style
        {
            public Color LabelColor;
            public Color BackgroundColor;
            public Color BackgroundBorderColor;
            public float Border;
            public bool EnableShadow;
            public Color ShadowColor;
            public float ShadowRadius;
        }

        public Style Normal;
        public Style Pressed;
    }
}
