using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using Unity.LiveCapture.VirtualCamera.Editor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.Tests.Editor
{
    abstract class VirtualCameraAnchoringTests
    {
        protected readonly Vector3EqualityComparer m_PositionComparer = new Vector3EqualityComparer(0.001f);
        protected readonly QuaternionEqualityComparer m_RotationComparer = new QuaternionEqualityComparer(0.001f);

        GameObject m_Root;
        protected PlayableDirector m_Director;
        protected VirtualCameraDevice m_Device;
        protected VirtualCameraActor m_Actor;
        protected IVirtualCameraClientInternal m_Client;
        protected Transform m_Anchor;

        protected SnapshotLibrary m_SnapshotLibrary;
        protected IScreenshotImpl m_ScreenshotImpl;

        [SetUp]
        public void Setup()
        {
            TakeRecorder.IsLive = true;

            LoadFromPrefab("Anchoring/AnchoringEvaluateSetup");
        }

        [TearDown]
        public void TearDown()
        {
            Unload();
        }

        void LoadFromPrefab(string path)
        {
            Unload();

            var prefab = Resources.Load<GameObject>(path);

            m_Root = GameObject.Instantiate(prefab);
            m_Director = m_Root.GetComponentInChildren<PlayableDirector>();
            m_Device = m_Root.GetComponentInChildren<VirtualCameraDevice>();
            m_Actor = m_Root.GetComponentInChildren<VirtualCameraActor>();
            m_Anchor = m_Root.transform.Find("Cube");

            m_Client = Substitute.For<IVirtualCameraClientInternal>();

            m_SnapshotLibrary = ScriptableObject.CreateInstance<SnapshotLibrary>();
            m_Device.SnapshotLibrary = m_SnapshotLibrary;
            m_ScreenshotImpl = Substitute.For<IScreenshotImpl>();
            m_Device.SetScreenshotImpl(m_ScreenshotImpl);
        }

        void Unload()
        {
            if (m_SnapshotLibrary != null)
            {
                GameObject.DestroyImmediate(m_SnapshotLibrary);
            }

            if (m_Root != null)
                GameObject.DestroyImmediate(m_Root);
        }

        protected IEnumerator Step()
        {
            yield return TestUtils.WaitForPlayerLoopUpdates(1);
        }

        protected void ChangeSettings(bool enabled, Transform target = null, Func<AnchorSettings, AnchorSettings> modify = null)
        {
            m_Device.AnchorEnabled = enabled;
            m_Device.AnchorTarget = target;

            if (modify != null)
            {
                var settings = m_Device.AnchorSettings;
                settings = modify(settings);
                m_Device.AnchorSettings = settings;
            }
        }
    }

    class AnchoringLiveTests : VirtualCameraAnchoringTests
    {
        [UnityTest]
        public IEnumerator Parent()
        {
            m_Device.SetClient(m_Client, false);
            yield return Step();

            ChangeSettings(true, m_Anchor, settings =>
            {
                settings.PositionLock = settings.RotationLock = Axis.X | Axis.Y | Axis.Z;
                return settings;
            });
            yield return Step();

            m_Device.SetARPose(new Pose(new Vector3(5f, 5f, 5f), Quaternion.Euler(0f, 0f, 0f)));
            yield return Step();

            Assert.That(m_Actor.transform.position, Is.EqualTo(new Vector3(5f, 5f, 5f)).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(Quaternion.identity).Using(m_RotationComparer));

            m_Anchor.position = new Vector3(5f, 5f, 5f);
            m_Anchor.eulerAngles = new Vector3(0f, 90f, 0f);
            yield return Step();

            Assert.That(m_Actor.transform.position, Is.EqualTo(new Vector3(10f, 10f, 0f)).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(Quaternion.Euler(0f, 90f, 0f)).Using(m_RotationComparer));
        }

        [UnityTest]
        public IEnumerator ActorDoesNotTeleportAfterChangingSettings()
        {
            ChangeSettings(false);

            m_Device.SetClient(m_Client, false);

            m_Device.SetARPose(new Pose(new Vector3(5f, 5f, 5f), Quaternion.Euler(5f, 5f, 5f)));

            yield return Step();

            m_Anchor.position = new Vector3(10f, 10f, 10f);
            m_Anchor.eulerAngles = new Vector3(10f, 10f, 10f);

            var previousPosition = m_Actor.transform.position;
            var previousRotation = m_Actor.transform.rotation;
            ChangeSettings(true, m_Anchor);
            yield return Step();
            Assert.That(m_Actor.transform.position, Is.EqualTo(previousPosition).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(previousRotation).Using(m_RotationComparer));

            previousPosition = m_Actor.transform.position;
            previousRotation = m_Actor.transform.rotation;
            ChangeSettings(false, m_Anchor);
            yield return Step();
            Assert.That(m_Actor.transform.position, Is.EqualTo(previousPosition).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(previousRotation).Using(m_RotationComparer));

            previousPosition = m_Actor.transform.position;
            previousRotation = m_Actor.transform.rotation;
            ChangeSettings(true, m_Anchor, settings =>
            {
                settings.PositionLock = Axis.Y;
                settings.RotationLock = Axis.X | Axis.Z;
                return settings;
            });
            yield return Step();
            Assert.That(m_Actor.transform.position, Is.EqualTo(previousPosition).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(previousRotation).Using(m_RotationComparer));

            previousPosition = m_Actor.transform.position;
            previousRotation = m_Actor.transform.rotation;
            ChangeSettings(true, m_Anchor, settings =>
            {
                settings.PositionLock = Axis.None;
                settings.RotationLock = Axis.None;
                return settings;
            });
            yield return Step();
            Assert.That(m_Actor.transform.position, Is.EqualTo(previousPosition).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(previousRotation).Using(m_RotationComparer));
        }

        [UnityTest]
        public IEnumerator TakeSnapshotUsesWorldPose()
        {
            m_Anchor.position = new Vector3(10f, 10f, 10f);
            m_Anchor.eulerAngles = Vector3.zero;

            var initialARPose = new Pose()
            {
                position = new Vector3(5f, 5f, 5f),
                rotation = Quaternion.Euler(5f, 5f, 5f)
            };

            m_Device.SetClient(m_Client, false);
            yield return Step();

            m_Device.SetARPose(initialARPose);
            yield return Step();

            Assert.That(m_Actor.transform.position, Is.EqualTo(m_Anchor.position + initialARPose.position).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(initialARPose.rotation).Using(m_RotationComparer));

            m_Device.TakeSnapshot();
            var snapshot = m_SnapshotLibrary.Get(m_SnapshotLibrary.Count - 1);

            Assert.IsTrue(m_Device.AnchorDeviceSettings.IsActive);
            Assert.That(snapshot.Pose.position, Is.EqualTo(m_Actor.transform.position).Using(m_PositionComparer));
            Assert.That(snapshot.Pose.rotation, Is.EqualTo(m_Actor.transform.rotation).Using(m_RotationComparer));
        }

        [UnityTest]
        public IEnumerator GoToSnapshotDisablesAnchoring()
        {
            m_Device.SetClient(m_Client, false);
            yield return Step();

            ChangeSettings(true, m_Anchor);
            yield return Step();

            Assert.IsTrue(m_Device.AnchorEnabled);

            var snapshot = new Snapshot()
            {
                Pose = new Pose()
            };

            m_Device.GoToSnapshot(snapshot);

            Assert.IsFalse(m_Device.AnchorEnabled);
        }

        [UnityTest]
        public IEnumerator AlignWithActor()
        {
            m_Device.SetClient(m_Client, false);
            yield return Step();

            m_Anchor.position = new Vector3(10f, 10f, 10f);
            m_Anchor.eulerAngles = new Vector3(10f, 10f, 10f);

            ChangeSettings(true, m_Anchor);
            yield return Step();

            Assert.IsTrue(m_Device.AnchorDeviceSettings.IsActive);

            var targetPose = new Pose()
            {
                position = new Vector3(1f, 2f, 3f),
                rotation = Quaternion.Euler(new Vector3(10f, 20f, 30f))
            };
            m_Actor.transform.position = targetPose.position;
            m_Actor.transform.rotation = targetPose.rotation;
            m_Device.RequestAlignWithActor();
            yield return Step();

            Assert.IsTrue(m_Device.AnchorDeviceSettings.IsActive);
            Assert.That(m_Actor.transform.position, Is.EqualTo(targetPose.position).Using(m_PositionComparer));
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(targetPose.rotation).Using(m_RotationComparer));
        }
    }

    class AnchoringPlaybackTests : VirtualCameraAnchoringTests
    {
        // The actor plays the take: Resources/Anchoring/Takes/[001] New Shot [001].asset
        // Where actor position XYZ curves look like (0s to 3s):
        //        ______
        //      /
        // ____/
        //
        // The anchor plays the animation clip: Resources/Anchoring/MasterTimeline.playable [Recorded]
        // Where the anchor position XYZ curves look like (0s to 2s):
        //    ____
        // _/      \_
        //
        [UnityTest]
        public IEnumerator Parent()
        {
            m_Director.RebuildGraph();

            m_Director.time = 0d;
            m_Director.DeferredEvaluate();
            yield return Step();
            Assert.That(m_Actor.transform.position, Is.EqualTo(new Vector3(0f, 0f, 0f)).Using(m_PositionComparer), "Incorrect initial pose");
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(Quaternion.Euler(0f, 0f, 0f)).Using(m_RotationComparer), "Incorrect initial pose");

            m_Director.time = 0.5d;
            m_Director.DeferredEvaluate();
            yield return Step();
            Assert.That(m_Actor.transform.position, Is.EqualTo(new Vector3(5f, 5f, 5f)).Using(m_PositionComparer), "Anchor motion applied incorrectly");
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(Quaternion.Euler(5f, 5f, 5f)).Using(m_RotationComparer), "Anchor motion applied incorrectly");

            m_Director.time = 1.5d;
            m_Director.DeferredEvaluate();
            yield return Step();
            Assert.That(m_Actor.transform.position, Is.EqualTo(new Vector3(20.01f, 19.88f, 20.11f)).Using(m_PositionComparer), "AR combined incorrectly with anchor motion");
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(Quaternion.Euler(18.44924f, 21.63789f, 21.44154f)).Using(m_RotationComparer), "AR combined incorrectly with anchor motion");
        }
    }

    class AnchoringDriverTests
    {
        public class AnchoringCase
        {
            public string m_Name;
            public Pose m_AnchorPose;
            public Pose m_ARPose;
            public AnchorSettings m_Settings;
            public Pose m_ExpectedWorldPose;
            public float m_DeltaTime;
            public int? m_SessionID;

            public AnchoringCase(string name, Pose anchorPose, Pose ARPose, AnchorSettings settings, Pose expectedWorldPose, float deltaTime = 1f, int? sessionID = null)
            {
                m_Name = name;
                m_AnchorPose = anchorPose;
                m_ARPose = ARPose;
                m_Settings = settings;
                m_ExpectedWorldPose = expectedWorldPose;
                m_DeltaTime = deltaTime;
                m_SessionID = sessionID;
            }

            public override string ToString()
            {
                return m_Name;
            }
        }

        readonly Vector3EqualityComparer m_PositionComparer = new Vector3EqualityComparer(0.001f);
        readonly QuaternionEqualityComparer m_RotationComparer = new QuaternionEqualityComparer(0.001f);

        GameObject m_ActorGO;
        VirtualCameraActor m_Actor;
        BaseCameraDriver m_Driver;
        IAnchorable m_Anchorable;
        Transform m_Target;

        [SetUp]
        public void Setup()
        {
            m_ActorGO = VirtualCameraCreatorUtilities.CreateVirtualCameraActor();
            m_ActorGO.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); // Revert MatchToSceneView

            m_Actor = m_ActorGO.GetComponent<VirtualCameraActor>();
            m_Driver = m_ActorGO.GetComponent<BaseCameraDriver>();
            m_Anchorable = m_Driver;

            m_Target = new GameObject().transform;
            m_Target.name = "anchorTarget";
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_ActorGO);
            GameObject.DestroyImmediate(m_Target.gameObject);
        }

        void Step(float deltaTime)
        {
            m_Driver.PostLateUpdate(deltaTime);
        }

        void SetupAnchoringCase(in AnchoringCase anchoringCase)
        {
            m_Target.position = anchoringCase.m_AnchorPose.position;
            m_Target.rotation = anchoringCase.m_AnchorPose.rotation;

            m_Actor.LocalPosition = anchoringCase.m_ARPose.position;
            m_Actor.LocalEulerAngles = anchoringCase.m_ARPose.rotation.eulerAngles;
            m_Actor.LocalPositionEnabled = m_Actor.LocalEulerAnglesEnabled = true;

            m_Anchorable.AnchorTo(m_Target, anchoringCase.m_Settings, anchoringCase.m_SessionID);
        }

        void AssertAnchoringCase(in AnchoringCase anchoringCase)
        {
            Assert.That(m_Actor.transform.position, Is.EqualTo(anchoringCase.m_ExpectedWorldPose.position).Using(m_PositionComparer), "Incorrect actor world position");
            Assert.That(m_Actor.transform.rotation, Is.EqualTo(anchoringCase.m_ExpectedWorldPose.rotation).Using(m_RotationComparer), "Incorrect actor world rotation");
        }

        void DoAnchoringCase(in AnchoringCase anchoringCase)
        {
            SetupAnchoringCase(anchoringCase);
            Step(anchoringCase.m_DeltaTime);
            AssertAnchoringCase(anchoringCase);
        }

        [Test]
        public void AnchoringIsDisabledWhenFlagsAreDisabled()
        {
            var anchorPose = new Pose()
            {
                position = new Vector3(10f, 10f, 10f),
                rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
            };

            var settings = new AnchorSettings()
            {
                PositionLock = Axis.X | Axis.Y | Axis.Z,
                RotationLock = Axis.X | Axis.Y | Axis.Z,
                PositionOffset = Vector3.zero,
                Damping = Damping.Default
            };

            var anchoringCase = new AnchoringCase(
                "AnchoringIsDisabledWhenFlagsAreDisabled",
                anchorPose,
                Pose.identity,
                settings,
                Pose.identity
            );

            SetupAnchoringCase(anchoringCase);

            // Mimic VirtualCameraChannelFlags.Position and VirtualCameraChannelFlags.Rotation toggled off
            m_Actor.LocalPositionEnabled = m_Actor.LocalEulerAnglesEnabled = false;

            Step(anchoringCase.m_DeltaTime);
            AssertAnchoringCase(anchoringCase);
        }

        [Test]
        public void DampingAffectsAnchorAndNotAR()
        {
            var anchorPose = new Pose()
            {
                position = new Vector3(10f, 10f, 10f),
                rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f))
            };

            var settings = new AnchorSettings()
            {
                PositionLock = Axis.X | Axis.Y | Axis.Z,
                RotationLock = Axis.X | Axis.Y | Axis.Z,
                PositionOffset = Vector3.zero,
                Damping = Damping.Default
            };

            var initialCase = new AnchoringCase(
                "Initial",
                anchorPose,
                Pose.identity,
                settings,
                anchorPose
            );

            DoAnchoringCase(initialCase);

            // The driver's internal anchor pose will be the following after damping:
            // position: (19, 19, 19)
            // rotation: (0, 90, 0)
            //
            // Because damping at 1.0 strength and 0.5 deltaTime gives the following formula:
            // newValue = 0.9 * deltaValue

            settings.Damping = new Damping()
            {
                Enabled = true,
                Body = Vector3.one * 1f,
                Aim = 1f
            };

            var dampedMovementCase = new AnchoringCase(
                "DampedMovement",
                new Pose()
                {
                    position = new Vector3(20f, 20f, 20f),
                    rotation = Quaternion.Euler(new Vector3(0f, 100f, 0f))
                },
                new Pose()
                {
                    position = new Vector3(0f, 0f, 5f),
                    rotation = Quaternion.Euler(new Vector3(0f, 5f, 0f))
                },
                settings,
                new Pose()
                {
                    position = new Vector3(24f, 19f, 19f),
                    rotation = Quaternion.Euler(new Vector3(0f, 95f, 0f))
                },
                0.5f
            );

            DoAnchoringCase(dampedMovementCase);
        }

        static IEnumerable FollowCases
        {
            get
            {
                var anchorPose = new Pose()
                {
                    position = new Vector3(10f, 10f, 10f),
                    rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                };

                var settings = new AnchorSettings()
                {
                    PositionLock = Axis.X | Axis.Y | Axis.Z,
                    RotationLock = Axis.None,
                    PositionOffset = Vector3.zero,
                    Damping = Damping.Default
                };

                yield return new AnchoringCase(
                    "AnchorOnly",
                    anchorPose,
                    Pose.identity,
                    settings,
                    new Pose()
                    {
                        position = anchorPose.position,
                        rotation = Quaternion.identity
                    }
                );

                yield return new AnchoringCase(
                    "AR",
                    anchorPose,
                    new Pose()
                    {
                        position = new Vector3(0f, 0f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    },
                    settings,
                    new Pose()
                    {
                        position = new Vector3(10f, 10f, 15f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    }
                );

                var offsetSettings = settings;
                offsetSettings.PositionOffset = new Vector3(0f, 0f, 5f);

                yield return new AnchoringCase(
                    "ARAndPositionOffset",
                    anchorPose,
                    new Pose()
                    {
                        position = new Vector3(0f, 0f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    },
                    offsetSettings,
                    new Pose()
                    {
                        position = new Vector3(15f, 10f, 15f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    }
                );
            }
        }

        [Test, TestCaseSource("FollowCases")]
        public void Follow(AnchoringCase anchoringCase)
        {
            DoAnchoringCase(anchoringCase);
        }

        static IEnumerable FollowAndIgnoreYCases
        {
            get
            {
                var anchorPose = new Pose()
                {
                    position = new Vector3(10f, 10f, 10f),
                    rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                };

                var settings = new AnchorSettings()
                {
                    PositionLock = Axis.X | Axis.Z,
                    RotationLock = Axis.None,
                    PositionOffset = Vector3.zero,
                    Damping = Damping.Default
                };

                yield return new AnchoringCase(
                    "AnchorOnly",
                    anchorPose,
                    Pose.identity,
                    settings,
                    new Pose()
                    {
                        position = new Vector3(anchorPose.position.x, 0f, anchorPose.position.z),
                        rotation = Quaternion.identity
                    }
                );

                yield return new AnchoringCase(
                    "AR",
                    anchorPose,
                    new Pose()
                    {
                        position = new Vector3(0f, 5f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    },
                    settings,
                    new Pose()
                    {
                        position = new Vector3(10f, 5f, 15f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    }
                );

                var offsetSettings = settings;
                offsetSettings.PositionOffset = new Vector3(0f, 5f, 5f);

                yield return new AnchoringCase(
                    "ARAndPositionOffset",
                    anchorPose,
                    new Pose()
                    {
                        position = new Vector3(0f, 5f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    },
                    offsetSettings,
                    new Pose()
                    {
                        position = new Vector3(15f, 5f, 15f),
                        rotation = Quaternion.Euler(new Vector3(0f, 45f, 0f))
                    }
                );
            }
        }

        [Test, TestCaseSource("FollowAndIgnoreYCases")]
        public void FollowAndIgnoreY(AnchoringCase anchoringCase)
        {
            DoAnchoringCase(anchoringCase);
        }

        static IEnumerable ParentCases
        {
            get
            {
                var anchorPose = new Pose()
                {
                    position = new Vector3(10f, 10f, 10f),
                    rotation = Quaternion.Euler(new Vector3(90f, 90f, 0f))
                };

                var settings = new AnchorSettings()
                {
                    PositionLock = Axis.X | Axis.Y | Axis.Z,
                    RotationLock = Axis.X | Axis.Y | Axis.Z,
                    PositionOffset = Vector3.zero,
                    Damping = Damping.Default
                };

                yield return new AnchoringCase(
                    "AnchorOnly",
                    anchorPose,
                    Pose.identity,
                    settings,
                    anchorPose
                );

                yield return new AnchoringCase(
                    "AR",
                    anchorPose,
                    new Pose()
                    {
                        position = new Vector3(0f, 0f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                    },
                    settings,
                    new Pose()
                    {
                        position = new Vector3(10f, 5f, 10f),
                        rotation = Quaternion.Euler(new Vector3(0f, 180f, 90f))
                    }
                );

                var offsetSettings = settings;
                offsetSettings.PositionOffset = new Vector3(0f, 0f, 5f);

                yield return new AnchoringCase(
                    "ARAndPositionOffset",
                    anchorPose,
                    new Pose()
                    {
                        position = new Vector3(0f, 0f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                    },
                    offsetSettings,
                    new Pose()
                    {
                        position = new Vector3(10f, 0f, 10f),
                        rotation = Quaternion.Euler(new Vector3(0f, 180f, 90f))
                    }
                );
            }
        }

        [Test, TestCaseSource("ParentCases")]
        public void Parent(AnchoringCase anchoringCase)
        {
            DoAnchoringCase(anchoringCase);
        }

        static IEnumerable ParentAndIgnoreXZRotationCases
        {
            get
            {
                var anchorPose = new Pose()
                {
                    position = new Vector3(10f, 10f, 10f),
                    rotation = Quaternion.Euler(new Vector3(45f, 90f, 10f))
                };

                var settings = new AnchorSettings()
                {
                    PositionLock = Axis.X | Axis.Y | Axis.Z,
                    RotationLock = Axis.Y,
                    PositionOffset = Vector3.zero,
                    Damping = Damping.Default
                };

                yield return new AnchoringCase(
                    "AnchorOnly",
                    anchorPose,
                    Pose.identity,
                    settings,
                    new Pose()
                    {
                        position = anchorPose.position,
                        rotation = Quaternion.Euler(new Vector3(0f, anchorPose.rotation.eulerAngles.y, 0f))
                    }
                );

                yield return new AnchoringCase(
                    "AR",
                    anchorPose,
                    new Pose()
                    {
                        position = new Vector3(0f, 0f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                    },
                    settings,
                    new Pose()
                    {
                        position = new Vector3(15f, 10f, 10f),
                        rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f))
                    }
                );

                var offsetSettings = settings;
                offsetSettings.PositionOffset = new Vector3(0f, 0f, 5f);

                yield return new AnchoringCase(
                    "ARAndPositionOffset",
                    new Pose()
                    {
                        position = new Vector3(0f, 0f, 0f),
                        rotation = Quaternion.Euler(new Vector3(90f, 90f, 0f))
                    },
                    new Pose()
                    {
                        position = new Vector3(0f, 0f, 5f),
                        rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                    },
                    offsetSettings,
                    new Pose()
                    {
                        position = new Vector3(5f, -5f, 0f),
                        rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f))
                    }
                );
            }
        }

        [Test, TestCaseSource("ParentAndIgnoreXZRotationCases")]
        public void ParentAndIgnoreXZRotation(AnchoringCase anchoringCase)
        {
            DoAnchoringCase(anchoringCase);
        }

        [Test]
        public void NoDampingDuringTransition()
        {
            var dampedSettings = new AnchorSettings()
            {
                PositionLock = Axis.X | Axis.Y | Axis.Z,
                RotationLock = Axis.X | Axis.Y | Axis.Z,
                PositionOffset = Vector3.zero,
                Damping = new Damping()
                {
                    Enabled = true,
                    Body = Vector3.one,
                    Aim = 1f
                }
            };

            var firstShot = new AnchoringCase(
                "dampingCase1",
                new Pose()
                {
                    position = new Vector3(10f, 10f, 10f),
                    rotation = Quaternion.Euler(new Vector3(0f, 10f, 0f))
                },
                new Pose()
                {
                    position = new Vector3(0f, 0f, 5f),
                    rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f))
                },
                dampedSettings,
                Pose.identity, // Dummy value because we're not asserting the actor pose for this case
                1f,
                1
            );

            SetupAnchoringCase(firstShot);
            Step(1f);

            var nextShot = new AnchoringCase(
                "dampingCase1",
                new Pose()
                {
                    position = new Vector3(20f, 20f, 20f),
                    rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                },
                new Pose()
                {
                    position = new Vector3(0f, 0f, 5f),
                    rotation = Quaternion.Euler(new Vector3(0f, 90f, 0f))
                },
                dampedSettings,
                new Pose()
                {
                    position = new Vector3(25f, 20f, 20f),
                    rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f))
                },
                1f,
                firstShot.m_SessionID + 1
            );

            DoAnchoringCase(nextShot);
        }
    }

    class AnchoringTrackTests
    {
        class AnchoringState
        {
            public Transform Target;
            public AnchorSettings? Settings;

            public void Validate(IAnchorable anchorable)
            {
                Transform target;
                AnchorSettings? settings;
                int? sessionID;

                anchorable.GetConfiguration(out target, out settings, out sessionID);

                Assert.That(target, Is.EqualTo(Target), "Incorrect anchor target");
                Assert.That(settings, Is.EqualTo(Settings), "Incorrect anchor settings");
            }
        }

        GameObject m_Root;
        PlayableDirector m_Director;
        IAnchorable m_Anchorable;
        Transform m_Anchor;

        [SetUp]
        public void Setup()
        {
            LoadFromPrefab("AnchoringTrack/AnchoringEvaluateSetup");
        }

        [TearDown]
        public void TearDown()
        {
            Unload();
        }

        void LoadFromPrefab(string path)
        {
            Unload();

            var prefab = Resources.Load<GameObject>(path);

            m_Root = GameObject.Instantiate(prefab);
            m_Director = m_Root.GetComponentInChildren<PlayableDirector>();
            m_Anchor = m_Root.transform.Find("Cube");

            var actor = m_Root.GetComponentInChildren<VirtualCameraActor>();
            m_Anchorable = actor.GetComponent<BaseCameraDriver>();
        }

        void Unload()
        {
            if (m_Root != null)
                GameObject.DestroyImmediate(m_Root);
        }

        // Uses Resources/AnchoringTrack/MasterTimeline.playable which contains 3 anchoring clips, each 3 seconds long.
        [Test]
        public void PlayClipsOneByOne()
        {
            var followState = new AnchoringState()
            {
                Target = m_Anchor,
                Settings = new AnchorSettings()
                {
                    PositionLock = Axis.X | Axis.Y | Axis.Z,
                    RotationLock = Axis.None,
                    PositionOffset = Vector3.zero,
                    Damping = Damping.Default
                }
            };

            var parentState = new AnchoringState()
            {
                Target = m_Anchor,
                Settings = new AnchorSettings()
                {
                    PositionLock = Axis.X | Axis.Y | Axis.Z,
                    RotationLock = Axis.X | Axis.Y | Axis.Z,
                    PositionOffset = Vector3.zero,
                    Damping = Damping.Default
                }
            };

            var parentDampedState = new AnchoringState()
            {
                Target = m_Anchor,
                Settings = new AnchorSettings()
                {
                    PositionLock = Axis.X | Axis.Y | Axis.Z,
                    RotationLock = Axis.X | Axis.Y | Axis.Z,
                    PositionOffset = Vector3.zero,
                    Damping = new Damping()
                    {
                        Enabled = true,
                        Body = Vector3.one,
                        Aim = 1f
                    }
                }
            };

            var offState = new AnchoringState()
            {
                Target = null,
                Settings = null
            };

            m_Director.RebuildGraph();

            m_Director.time = 0d;
            m_Director.Evaluate();
            followState.Validate(m_Anchorable);

            m_Director.time = 3d;
            m_Director.Evaluate();
            parentState.Validate(m_Anchorable);

            m_Director.time = 6d;
            m_Director.Evaluate();
            parentDampedState.Validate(m_Anchorable);

            m_Director.time = 100d;
            m_Director.Evaluate();
            offState.Validate(m_Anchorable);

            m_Director.time = 0d;
            m_Director.Evaluate();
            followState.Validate(m_Anchorable);
        }
    }
}
