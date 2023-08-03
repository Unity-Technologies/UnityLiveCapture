using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Unity.CompanionAppCommon;
using NUnit.Framework;

namespace Tests
{
    class ContinuousIntegrationTests
    {
        [TearDown]
        public void CleanUp()
        {
            AssetDatabase.DeleteAsset("Assets/Resources/buildInfo.asset");
        }

        [Test]
        public void BuildRuntimeApp()
        {
#if UNITY_EDITOR_OSX
            var buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new[]
                {
                    "Assets/Runtime/Mobile/App.unity",
                    "Assets/Runtime/Tablet/App.unity"
                }
            };

            buildPlayerOptions.locationPathName = "iOSBuild";
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                throw new AssertionException($"Build failed");
            }

            // This is a functional assertion that PreBuildVersionInjection has been called
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<BuildInfo>("Assets/Resources/buildInfo.asset"));
#else
            Assert.Pass("The application can only build on iOS for now.");
#endif
        }
    }
}
