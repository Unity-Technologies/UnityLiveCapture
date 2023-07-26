using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.CompanionAppCommon;
using Unity.CompanionAppCommonEditor;
using NSubstitute;

namespace Unity.VirtualProduction.Tests.Editor
{
    public class PreBuildVersionInjectionTests
    {
        [Test]
        public void HandlePackageListRequest_ResourcesFolderExists_AssetCreatedWithInfo()
        {
            // Arrange
            string requestedPackage = string.Empty;
            var assetProxy = Substitute.For<IAssetAccessProxy>();
            assetProxy.IsValidFolder("any args").ReturnsForAnyArgs(true);
            var versionInjection = new PreBuildVersionInjection(assetProxy);

            versionInjection.RetrieveCurrentDateTime = () => DateTime.Parse("1984-08-27 12:12:12.0000000-00:00", CultureInfo.InvariantCulture).ToUniversalTime();

            versionInjection.CheckForVersionName = delegate(string s, PackageRequestHandle packageRequestHandle) {
                packageRequestHandle.PackageVersion = "MyVersion";
                requestedPackage = s;
                packageRequestHandle.IsCompleted = true;
            };

            // Act
            versionInjection.OnPreprocessBuild(null);

            // Assert
            Assert.AreEqual(requestedPackage, "com.unity.live-capture");

            assetProxy.DidNotReceiveWithAnyArgs().CreateFolder("Assets", "Resources");

            var callToCreateAsset = assetProxy.ReceivedCalls().SingleOrDefault(x => x.GetMethodInfo().Name == "CreateAsset");
            Assert.IsNotNull(callToCreateAsset);
            Assert.AreEqual("MyVersion", ((BuildInfo)callToCreateAsset.GetArguments()[0]).m_LiveCaptureVersion);
            Assert.AreEqual("1984-08-27T12:12:12.0000000Z", ((BuildInfo)callToCreateAsset.GetArguments()[0]).m_BuildDate);

            Assert.AreEqual("Assets/Resources/buildInfo.asset", callToCreateAsset.GetArguments()[1]);

            assetProxy.ReceivedWithAnyArgs().SaveAssets();
        }

        [Test]
        public void HandlePackageListRequest_ResourcesFolderDoesNotExist_FolderCreatedAndAssetCreatedWithInfo()
        {
            // Arrange
            string requestedPackage = string.Empty;
            var assetProxy = Substitute.For<IAssetAccessProxy>();
            assetProxy.IsValidFolder("any args").ReturnsForAnyArgs(false);
            var versionInjection = new PreBuildVersionInjection(assetProxy);

            versionInjection.RetrieveCurrentDateTime = () => DateTime.Parse("1984-08-27 12:12:12.0000000-00:00", CultureInfo.InvariantCulture).ToUniversalTime();

            versionInjection.CheckForVersionName = delegate(string s, PackageRequestHandle packageRequestHandle) {
                packageRequestHandle.PackageVersion = "MyVersion";
                requestedPackage = s;
                packageRequestHandle.IsCompleted = true;
            };

            // Act
            versionInjection.OnPreprocessBuild(null);

            // Assert
            Assert.AreEqual(requestedPackage, "com.unity.live-capture");

            assetProxy.Received().CreateFolder("Assets", "Resources");

            var callToCreateAsset = assetProxy.ReceivedCalls().SingleOrDefault(x => x.GetMethodInfo().Name == "CreateAsset");
            Assert.IsNotNull(callToCreateAsset);
            Assert.AreEqual("MyVersion", ((BuildInfo)callToCreateAsset.GetArguments()[0]).m_LiveCaptureVersion);
            Assert.AreEqual("1984-08-27T12:12:12.0000000Z", ((BuildInfo)callToCreateAsset.GetArguments()[0]).m_BuildDate);
            Assert.AreEqual("Assets/Resources/buildInfo.asset", callToCreateAsset.GetArguments()[1]);

            assetProxy.ReceivedWithAnyArgs().SaveAssets();
        }

        [Test]
        public void HandlePackageListRequest_PackageClientTakesMoreThan5Seconds_ExceptionIsThrownAndAssetNotCreated()
        {
            // Arrange
            var assetProxy = Substitute.For<IAssetAccessProxy>();
            var versionInjection = new PreBuildVersionInjection(assetProxy);
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            versionInjection.CheckForVersionName = delegate(string s, PackageRequestHandle packageRequestHandle) { Thread.Sleep(6000); packageRequestHandle.IsCompleted = true; };
            Exception exceptionAfterReachingTimeout = null;

            // Act
            try
            {
                versionInjection.OnPreprocessBuild(null);
            }
            catch (TimeoutException e)
            {
                exceptionAfterReachingTimeout = e;
            }

            // Assert
            Assert.IsNotNull(exceptionAfterReachingTimeout);
            Assert.AreEqual(assetProxy.ReceivedCalls().Count(), 0);
        }
    }
}
