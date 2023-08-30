using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Unity.CompanionAppCommon;

namespace Unity.CompanionAppCommonEditor
{
    internal class PreBuildVersionInjection : IPreprocessBuildWithReport
    {
        public PreBuildVersionInjection()
        {
        }

        internal PreBuildVersionInjection(IAssetAccessProxy assetAccessProxy)
        {
            m_AssetProxy = assetAccessProxy;
        }

        public int callbackOrder => 0;
        private readonly IAssetAccessProxy m_AssetProxy = new AssetAccessProxy();

        internal Func<DateTime> RetrieveCurrentDateTime { get; set; } = () => DateTime.Now;

        internal Action<string, PackageRequestHandle> CheckForVersionName { get; set; } = (packageName, packageRequestHandle) =>
        {
            if (packageRequestHandle.CurrentRequest == null)
            {
                packageRequestHandle.CurrentRequest = Client.List();
            }

            if (packageRequestHandle.CurrentRequest.IsCompleted)
            {
                if (packageRequestHandle.CurrentRequest.Status == StatusCode.Success)
                {
                    packageRequestHandle.IsCompleted = true;
                    packageRequestHandle.PackageVersion = packageRequestHandle.CurrentRequest.Result.FirstOrDefault(x => x.name == packageName)?.version;
                }
                else
                {
                    throw new Exception($"Package version for {packageName} could not be retrieved.Error message: {packageRequestHandle.CurrentRequest.Error?.message}");
                }
            }
        };

        public void OnPreprocessBuild(BuildReport report)
        {
            var timeout = TimeSpan.FromSeconds(5);
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            var packageRequestHandle = new PackageRequestHandle();

            while (!cts.IsCancellationRequested &&  !packageRequestHandle.IsCompleted)
            {
                CheckForVersionName("com.unity.live-capture", packageRequestHandle);
            }

            if (cts.IsCancellationRequested)
            {
                throw new TimeoutException($"The package information could not be retrieved within { timeout.Seconds } seconds.");
            }

            var buildInfo = ScriptableObject.CreateInstance<BuildInfo>();
            buildInfo.m_LiveCaptureVersion = packageRequestHandle.PackageVersion;
            buildInfo.m_BuildDate = RetrieveCurrentDateTime().ToString("O", CultureInfo.InvariantCulture);

            if (!m_AssetProxy.IsValidFolder("Assets/Resources"))
            {
                m_AssetProxy.CreateFolder("Assets", "Resources");
            }

            m_AssetProxy.CreateAsset(buildInfo, "Assets/Resources/buildInfo.asset");
            m_AssetProxy.SaveAssets();
        }
    }

    internal class PackageRequestHandle
    {
        internal ListRequest CurrentRequest { get; set; }

        internal bool IsCompleted { get; set; }

        internal string PackageVersion { get; set; }
    }
}
