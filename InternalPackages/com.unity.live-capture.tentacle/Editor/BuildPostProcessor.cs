#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

namespace Unity.LiveCapture.Tentacle.Editor
{
    class BuildPostprocessor
    {
        const string k_PluginPath = "com.unity.live-capture.tentacle/Plugins/iOS";
        const string k_FrameworkName = "Tentacle.framework";

        [PostProcessBuild(100)]
        static void ReferenceFramework(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            // load the project file
            var proj = new PBXProject();
            var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            proj.ReadFromString(File.ReadAllText(projPath));

            // include the tentacle framework as a dependency
            var targetGuid = proj.GetUnityMainTargetGuid();
            var framework = Path.Combine(k_PluginPath, k_FrameworkName);
            var fileGuid = proj.AddFile(framework, Path.Combine("Frameworks", framework), PBXSourceTree.Sdk);
            proj.AddFileToEmbedFrameworks(targetGuid, fileGuid);

            // write back the project file
            proj.WriteToFile(projPath);
        }

        [PostProcessBuild(100)]
        static void RequestBluetooth(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
            {
                return;
            }

            // Get plist
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // request access to bluetooth
            PlistElementDict rootDict = plist.root;
            rootDict.SetString("NSBluetoothAlwaysUsageDescription", "Required to support additional Timecode sources");

            // legacy key to request access to bluetooth (required by AppStoreConnect or upload fails)
            rootDict.SetString("NSBluetoothPeripheralUsageDescription", "Required to support additional Timecode sources");

            // Write back to plist file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif
