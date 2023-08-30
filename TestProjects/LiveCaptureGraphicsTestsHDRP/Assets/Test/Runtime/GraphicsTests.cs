using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Graphics;

public class GraphicsTests
{
    public const string Path = "Assets/ReferenceImages";

    [PrebuildSetup("SetupGraphicsTestCases")]
    [UseGraphicsTestCases]
    public IEnumerator GraphicsTest(GraphicsTestCase testCase)
    {
        SceneManager.LoadScene(testCase.ScenePath);

        // Wait for the scene to load.
        for (var i = 0; i < 5; i++)
        {
            yield return null;
        }
        
        var settings = GameObject.FindObjectOfType<GraphicsTestSettings>();
        if (settings == null)
        {
            Assert.Fail("Missing test settings for graphic tests.");
        }

        // Get the test camera
        var camera = Camera.main;
        if (camera == null) camera = GameObject.FindObjectOfType<Camera>();
        if (camera == null)
        {
            Assert.Fail("Missing camera for graphic tests.");
        }

        var imageComparisonSettings = settings != null
            ? settings.ImageComparisonSettings
            : null;
        
        // Do the image comparison test
        ImageAssert.AreEqual(testCase.ReferenceImage, camera, imageComparisonSettings);
    }

#if UNITY_EDITOR
    // This step is needed to save the result images in the project.
    
    [TearDown]
    public void DumpImagesInEditor()
    {
        UnityEditor.TestTools.Graphics.ResultsUtility.ExtractImagesFromTestProperties(TestContext.CurrentContext.Test);
    }
#endif
}
