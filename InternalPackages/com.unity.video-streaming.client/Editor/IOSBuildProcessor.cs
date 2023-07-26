#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace Unity.VirtualProduction.VideoStreaming.Client
{
    class IOSBuildProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.iOS)
            {
                var projPath = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";

                PBXProject proj = new PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));
                proj.AddFrameworkToProject(proj.GetUnityFrameworkTargetGuid(), "VideoToolbox.framework", false);

                File.WriteAllText(projPath, proj.WriteToString());
            }
        }
    }
}
#endif
