using System;
using NUnit.Framework;
using Unity.LiveCapture.ARKitFaceCapture.DefaultMapper;
using Unity.LiveCapture.ARKitFaceCapture.DefaultMapper.Editor;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    class DefaultFaceMapperTests
    {
        [Test]
        public void EditorHasDefaultResources()
        {
            var mapper = ScriptableObject.CreateInstance<DefaultFaceMapper>();
            var editor = UnityEditor.Editor.CreateEditor(mapper) as DefaultFaceMapperEditor;

            Assert.IsNotNull(editor.m_DefaultGlobalEvaluator);

            UnityEngine.Object.DestroyImmediate(mapper);
            UnityEngine.Object.DestroyImmediate(editor);
        }
    }
}
