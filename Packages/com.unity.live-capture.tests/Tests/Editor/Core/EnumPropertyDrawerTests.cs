using System;
using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.LiveCapture.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.LiveCapture.Tests.Editor
{
    public class EnumPropertyDrawerTests
    {
        class TestGui : EnumButtonGroupAttributeDrawer.IGui
        {
            public readonly Queue<bool> toggleValues = new Queue<bool>();
            public readonly Dictionary<string, string> tooltips = new Dictionary<string, string>();
            public bool storeTooltips;

            public bool Toggle(Rect position, bool value, GUIContent content)
            {
                if (storeTooltips)
                {
                    tooltips[content.text] = content.tooltip;
                }

                return toggleValues.Dequeue();
            }

            public void LabelField(Rect position, GUIContent label) {}

            public float GetSegmentWidth(PropertyDrawer drawer) => 64;
            public GUIContent BeginProperty(Rect position, GUIContent label, SerializedProperty property) => label;
            public void EndProperty() {}

            public float SingleLineHeight => 64;
            public float StandardVerticalSpacing => 64;
            public float CurrentViewWidth => 64;
            public float LabelWidth => 64;
            public float Indent => 10;
        }

        enum EnumWithoutNone
        {
            Value0,
            Value1,
        }

        enum EnumWithNone
        {
            None,
            Value0,
            Value1,
        }

        enum EnumWithTooltips
        {
            [Description("Lorem Ispum.")]
            Value0,
            [Description("Dolor sit amet.")]
            Value1,
            Value2,
        }

        [Flags]
        enum FlagsEnum
        {
            None = 0,
            ZValue0 = 1, // Z to make sure values are not alphabetically indexed.
            Value1 = 1 << 1,
            Value3 = 1 << 2,
            All = ~0
        }

        [Flags]
        enum FlagsEnumInvalid0
        {
            Value1 = 0,
            ZValue0 = 1,
            None = 1 << 1,
            Value3 = 1 << 2
        }

        [Flags]
        enum FlagsEnumInvalid1
        {
            None = 0,
            ZValue0 = 1,
            Value1 = 1 << 1,
            Value3 = 1 << 2,
            Everything = 1 << 2,
            All = ~0
        }

        class TestComponent : MonoBehaviour
        {
            public EnumWithoutNone enumWithoutNone;
            public EnumWithNone enumWithNone;
            public EnumWithTooltips enumWithTooltips;
            public FlagsEnum flagsEnum;
            public FlagsEnumInvalid0 flagsEnumInvalid0;
            public FlagsEnumInvalid1 flagsEnumInvalid1;
        }

        static readonly Rect k_DefaultRect = new Rect(0, 0, 512, 512);
        static readonly GUIContent k_DefaultGUIContent = new GUIContent("Test");
        static Type s_Type = typeof(TestComponent);
        static BindingFlags s_Flags = BindingFlags.Public | BindingFlags.Instance;
        static FieldInfo s_EnumWithoutNoneField = s_Type.GetField("enumWithoutNone", s_Flags);
        static FieldInfo s_EnumWithNoneField = s_Type.GetField("enumWithNone", s_Flags);
        static FieldInfo s_EnumWithTooltipsField = s_Type.GetField("enumWithTooltips", s_Flags);
        static FieldInfo s_FlagsEnumField = s_Type.GetField("flagsEnum", s_Flags);
        static FieldInfo s_FlagsEnumInvalid0Field = s_Type.GetField("flagsEnumInvalid0", s_Flags);
        static FieldInfo s_FlagsEnumInvalid1Field = s_Type.GetField("flagsEnumInvalid1", s_Flags);
        static FieldInfo s_FieldInfo = typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance);

        TestComponent m_TestComponent;
        TestGui m_Gui;

        [SetUp]
        public void Setup()
        {
            var go = new GameObject()
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            m_TestComponent = go.AddComponent<TestComponent>();

            m_Gui = new TestGui();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_TestComponent.gameObject);
        }

        [Test]
        public void CanSelectEnumValueWithoutNone()
        {
            m_TestComponent.enumWithoutNone = EnumWithoutNone.Value0;

            var drawer = new EnumButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_EnumWithoutNoneField);
            drawer.Gui = m_Gui;

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(true);

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("enumWithoutNone");

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            serializedObject.ApplyModifiedProperties();

            Assert.IsTrue(m_TestComponent.enumWithoutNone == EnumWithoutNone.Value1);
        }

        [Test]
        public void EnumValueWithoutNoneDoesNotChangeOnDeselect()
        {
            m_TestComponent.enumWithoutNone = EnumWithoutNone.Value0;

            var drawer = new EnumButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_EnumWithoutNoneField);
            drawer.Gui = m_Gui;

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(false);

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("enumWithoutNone");

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            serializedObject.ApplyModifiedProperties();

            Assert.IsTrue(m_TestComponent.enumWithoutNone == EnumWithoutNone.Value0);
        }

        [Test]
        public void CanSelectEnumValueWithNone()
        {
            m_TestComponent.enumWithNone = EnumWithNone.Value0;

            var drawer = new EnumButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_EnumWithNoneField);
            drawer.Gui = m_Gui;

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(true);

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("enumWithNone");

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            serializedObject.ApplyModifiedProperties();

            Assert.IsTrue(m_TestComponent.enumWithNone == EnumWithNone.Value1);
        }

        [Test]
        public void CanDeselectEnumValueWithNone()
        {
            m_TestComponent.enumWithNone = EnumWithNone.Value0;

            var drawer = new EnumButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_EnumWithNoneField);
            drawer.Gui = m_Gui;

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(false);

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("enumWithNone");

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            serializedObject.ApplyModifiedProperties();

            Assert.IsTrue(m_TestComponent.enumWithNone == EnumWithNone.None);
        }

        [Test]
        public void CanSetFlagsEnumValue()
        {
            m_TestComponent.flagsEnum = FlagsEnum.None;

            var drawer = new EnumFlagButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_FlagsEnumField);
            drawer.Gui = m_Gui;

            m_Gui.toggleValues.Enqueue(true);
            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(false);

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("flagsEnum");

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            serializedObject.ApplyModifiedProperties();

            Assert.IsTrue((m_TestComponent.flagsEnum.HasFlag(FlagsEnum.ZValue0)));
            Assert.IsFalse((m_TestComponent.flagsEnum.HasFlag(FlagsEnum.Value1)));
            Assert.IsFalse((m_TestComponent.flagsEnum.HasFlag(FlagsEnum.Value3)));
        }

        [Test]
        public void CanSetFlagsEnumValue2()
        {
            m_TestComponent.flagsEnum = FlagsEnum.ZValue0 | FlagsEnum.Value3;

            var drawer = new EnumFlagButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_FlagsEnumField);
            drawer.Gui = m_Gui;

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(true);
            m_Gui.toggleValues.Enqueue(true);

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("flagsEnum");

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            serializedObject.ApplyModifiedProperties();

            Assert.IsFalse((m_TestComponent.flagsEnum.HasFlag(FlagsEnum.ZValue0)));
            Assert.IsTrue((m_TestComponent.flagsEnum.HasFlag(FlagsEnum.Value1)));
            Assert.IsTrue((m_TestComponent.flagsEnum.HasFlag(FlagsEnum.Value3)));
        }

        [Test]
        public void CanSetFlagsEnumValueToNone()
        {
            m_TestComponent.flagsEnum = FlagsEnum.Value1 | FlagsEnum.Value3;

            var drawer = new EnumFlagButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_FlagsEnumField);
            drawer.Gui = m_Gui;

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(false);

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("flagsEnum");

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            serializedObject.ApplyModifiedProperties();

            Assert.IsTrue((m_TestComponent.flagsEnum == FlagsEnum.None));
        }

        [Test]
        public void InvalidFlagsEnumThrows()
        {
            m_TestComponent.flagsEnumInvalid0 = FlagsEnumInvalid0.None;
            m_TestComponent.flagsEnumInvalid1 = FlagsEnumInvalid1.None;

            var drawer = new EnumFlagButtonGroupAttributeDrawer();
            drawer.Gui = m_Gui;

            for (var i = 0; i != 12; ++i)
            {
                m_Gui.toggleValues.Enqueue(false);
            }

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop0 = serializedObject.FindProperty("flagsEnumInvalid0");
            var prop1 = serializedObject.FindProperty("flagsEnumInvalid1");

            Assert.Throws<InvalidOperationException>(() =>
            {
                s_FieldInfo.SetValue(drawer, s_FlagsEnumInvalid0Field);
                drawer.OnGUI(k_DefaultRect, prop0, k_DefaultGUIContent);
            });
            Assert.Throws<InvalidOperationException>(() =>
            {
                s_FieldInfo.SetValue(drawer, s_FlagsEnumInvalid1Field);
                drawer.OnGUI(k_DefaultRect, prop1, k_DefaultGUIContent);
            });
        }

        [Test]
        public void TestTooltips()
        {
            m_Gui.storeTooltips = true;

            var drawer = new EnumButtonGroupAttributeDrawer();
            s_FieldInfo.SetValue(drawer, s_EnumWithTooltipsField);
            drawer.Gui = m_Gui;

            var serializedObject = new SerializedObject(m_TestComponent);
            var prop = serializedObject.FindProperty("enumWithTooltips");

            // Select all 3 values, one by one.
            m_Gui.toggleValues.Enqueue(true);
            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(false);

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(true);
            m_Gui.toggleValues.Enqueue(false);

            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(false);
            m_Gui.toggleValues.Enqueue(true);

            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);
            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);
            drawer.OnGUI(k_DefaultRect, prop, k_DefaultGUIContent);

            var tooltip = String.Empty;

            Assert.IsTrue(m_Gui.tooltips.TryGetValue("Value 0", out tooltip));
            Assert.IsTrue(tooltip == "Lorem Ispum.");

            Assert.IsTrue(m_Gui.tooltips.TryGetValue("Value 1", out tooltip));
            Assert.IsTrue(tooltip == "Dolor sit amet.");

            Assert.IsTrue(m_Gui.tooltips.TryGetValue("Value 2", out tooltip));
            Assert.IsTrue(tooltip == null);
        }
    }
}
