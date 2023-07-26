#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;
using System.Xml;

namespace Unity.CompanionAppCommon
{
    class XcodePostBuild
    {
        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                // Get plist
                string plistPath = pathToBuiltProject + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));

                // Get root
                PlistElementDict rootDict = plist.root;

                // Change value of ITSAppUsesNonExemptEncryption to false
                var encryptKey = "ITSAppUsesNonExemptEncryption";
                rootDict.SetString(encryptKey, "false");

                // Allow access to the application documents folder
                rootDict.SetBoolean("UIFileSharingEnabled", true);
                rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);

                // Write back to plist file
                File.WriteAllText(plistPath, plist.WriteToString());

                // Get xcodeproj
                var projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
                PBXProject proj = new PBXProject();
                proj.ReadFromString(File.ReadAllText(projPath));
                var mainTarget = proj.GetUnityMainTargetGuid();
                var frameworkTarget = proj.GetUnityFrameworkTargetGuid();

                // Updating to Apple Generic Versioning to allow Fastlane to increment versioning on deploy
                proj.UpdateBuildProperty(mainTarget, "Versioning System", new string[] { "Apple Generic" }, new string[] { "" });

                // Bitcode is deprecated since Xcode 14 but Unity still enabled it by default
                proj.SetBuildProperty(mainTarget, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(frameworkTarget, "ENABLE_BITCODE", "NO");

                // Write back project
                var processedProjectString = ProcessProjectString(proj.WriteToString());
                File.WriteAllText(projPath, processedProjectString);

                // Get scheme
                var schemePath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme";
                XcScheme scheme = new XcScheme();
                scheme.ReadFromString(File.ReadAllText(schemePath));

                // Write back scheme
                File.WriteAllText(schemePath, scheme.WriteToString());
                ProcessSchemeXML(schemePath);
            }
        }

        static string ProcessProjectString(string project)
        {
            // Disable bitcode by string replace
            // (https://forum.unity.com/threads/bitcode-bundle-could-not-be-generated-issue.897590/#post-5914007)
            var ret = project.Replace(
                "ENABLE_BITCODE = YES;",
                "ENABLE_BITCODE = NO;"
            );

            return ret;
        }

        static void ProcessSchemeXML(string schemePath)
        {
            try
            {
                var document = new XmlDocument();
                document.Load(schemePath);

                if (document.DocumentElement != null &&
                    document.DocumentElement["LaunchAction"] is {} launchAction)
                {
                    // Disable overwhelming warnings from the Thread Performance Checker
                    // It's a known ARKit 5.0.2 warning:
                    // https://forum.unity.com/threads/unity-2021-3-9f1-xcode-14-ios-16-problem.1335296/#post-8437043
                    launchAction.SetAttribute("disablePerformanceAntipatternChecker", "YES");
                }

                document.Save(schemePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}
#endif
