using System.Collections;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera.Rigs;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.VirtualCamera.Tests
{
    public class VirtualCameraRigTranslateTests
    {
        const float k_CompareTolerance = 0.001f;

        public class RigCase
        {
            /// <summary>
            /// The name of the case. Will appear in the editor. Should describe the initial state, the steps, and the
            /// expected outcome
            /// </summary>
            public string name;
            internal VirtualCameraRigState initialState = VirtualCameraRigState.Identity;
            public Vector3 direction;
            public Vector3 speed;
            public Space pedestalSpace;
            public Space motionSpace;
            internal VirtualCameraRigState expectedState;

            internal RigCase(string name, VirtualCameraRigState initialState, VirtualCameraRigState expectedState, Vector3 direction, Vector3 speed, Space pedestalSpace, Space motionSpace = Space.Self)
            {
                this.name = name;
                this.initialState = initialState;
                this.expectedState = expectedState;
                this.direction = direction;
                this.speed = speed;
                this.pedestalSpace = pedestalSpace;
                this.motionSpace = motionSpace;
            }

            public override string ToString()
            {
                return name;
            }
        }

        Vector3EqualityComparer m_Vector3Comparer = new Vector3EqualityComparer(k_CompareTolerance);

        static VirtualCameraRigState CreateRigState(Quaternion? RebaseOffset = null, Pose? ARPose = null, Pose? LocalPose = null, Pose? Pose = null, Pose? Origin = null)
        {
            var state = VirtualCameraRigState.Identity;
            state.RebaseOffset = RebaseOffset ?? state.RebaseOffset;
            state.ARPose = ARPose ?? state.ARPose;
            state.LocalPose = LocalPose ?? state.LocalPose;
            state.Pose = Pose ?? state.Pose;
            state.Origin = Origin ?? state.Origin;

            return state;
        }

        static IEnumerable TranslateUsingNonUniformSpeedCase
        {
            get
            {
                var tilt45Pan45 = CreateRigState
                (
                    ARPose: new Pose(Vector3.zero, Quaternion.Euler(45f, 45f, 0)),
                    Pose: new Pose(Vector3.zero, Quaternion.Euler(45f, 45f, 0f)),
                    Origin: new Pose(new Vector3(4, 10, 3), Quaternion.Euler(new Vector3(10, 180, 280)))
                );

                var tilt315 = CreateRigState
                (
                    ARPose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 0)),
                    Pose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 0f))
                );

                var tilt315Dutch270 = CreateRigState
                (
                    ARPose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 270)),
                    Pose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 270f))
                );

                yield return new RigCase(
                    "tilt 45, pan 45, speed 1,2,1, origin position 4,10,3 rotation 10,180,280, translate forward: expect position x4.6 y9.3 z2.6 ",
                    tilt45Pan45,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(0.5f, -0.7071f, 0.5f), Quaternion.Euler(45f, 45f, 0)),
                        Pose: new Pose(new Vector3(4.609f, 9.307f, 2.614f), Quaternion.Euler(43.8f, 122.3f, 231.3f))
                    ),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 2, 1),
                    Space.Self);

                yield return new RigCase(
                    "tilt 45, pan 45, speed 1,2,1, Space world, origin position 4,10,3 rotation 10,180,280, translate forward: expect position x4.6 y9.3 z2.6 ",
                    tilt45Pan45,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(0.5f, -0.7071f, 0.5f), Quaternion.Euler(45f, 45f, 0)),
                        Pose: new Pose(new Vector3(4.609f, 9.307f, 2.614f), Quaternion.Euler(43.8f, 122.3f, 231.3f))
                    ),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 2, 1),
                    Space.World);

                yield return new RigCase(
                    "tilt 45, pan 45, speed 1,1,0, origin position 4,10,3 rotation 10,180,280, translate forward: expect position x0 y0 z0 ",
                    tilt45Pan45,
                    CreateRigState
                    (
                        LocalPose: new Pose(Vector3.zero, Quaternion.Euler(45f, 45f, 0)),
                        Pose: new Pose(new Vector3(4, 10, 3), Quaternion.Euler(43.8f, 122.3f, 231.3f))
                    ),
                    new Vector3(0, 0, 1),
                    new Vector3(1, 1, 0),
                    Space.Self);

                yield return new RigCase(
                    "tilt 315, speed 10,3,1, Space world, translate up: expect position x0 y3 z0 ",
                    tilt315,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(0, 3, 0), Quaternion.Euler(315, 0, 0)),
                        Pose: new Pose(new Vector3(0, 3, 0), Quaternion.Euler(315, 0, 0))
                    ),
                    new Vector3(0, 1, 0),
                    new Vector3(10, 3, 1),
                    Space.World);

                yield return new RigCase(
                    "tilt 315 dutch 270, pedestal 0, speed 10,3,1, translate up: expect position x3 y0 z0 ",
                    tilt315Dutch270,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(3, 0, 0), Quaternion.Euler(315, 0, 270)),
                        Pose: new Pose(new Vector3(3, 0, 0), Quaternion.Euler(315, 0, 270))
                    ),
                    new Vector3(0, 1, 0),
                    new Vector3(10, 3, 1),
                    Space.Self);
            }
        }

        static IEnumerable TranslateUsingUniformSpeedCase
        {
            get
            {
                var tilt45Pan45 = CreateRigState
                (
                    ARPose: new Pose(new Vector3(0f, 0, 0f), Quaternion.Euler(45f, 45f, 0)),
                    Pose: new Pose(Vector3.zero, Quaternion.Euler(45f, 45f, 0f))
                );

                var tilt315 = CreateRigState
                (
                    ARPose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 0)),
                    Pose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 0f))
                );

                yield return new RigCase(
                    "tilt 45, pan 45, speed 2 and translate forward: expect position x0.5 y-0.7 z0.5 ",
                    tilt45Pan45,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(1, -1.4142f, 1), Quaternion.Euler(45f, 45f, 0)),
                        Pose: new Pose(new Vector3(1, -1.4142f, 1), Quaternion.Euler(45f, 45f, 0f))
                    ),
                    new Vector3(0, 0, 1),
                    new Vector3(2, 2, 2),
                    Space.Self);

                yield return new RigCase(
                    "tilt 45, pan 45, speed 0, translate forward: expect position x0 y0 z0 ",
                    tilt45Pan45,
                    CreateRigState
                    (
                        LocalPose: new Pose(Vector3.zero, Quaternion.Euler(45f, 45f, 0)),
                        Pose: new Pose(Vector3.zero, Quaternion.Euler(45f, 45f, 0f))
                    ),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, 0),
                    Space.Self);

                yield return new RigCase(
                    "tilt 315, speed 3, Space world, translate up: expect position x0 y3 z0 ",
                    tilt315,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(0, 3, 0), Quaternion.Euler(315, 0, 0)),
                        Pose: new Pose(new Vector3(0, 3, 0), Quaternion.Euler(315, 0, 0))
                    ),
                    new Vector3(0, 1, 0),
                    new Vector3(3, 3, 3),
                    Space.World);

                yield return new RigCase(
                    "tilt 315, speed 3, translate up: expect position x0 y3 z0 ",
                    tilt315,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(0, 2.121f, -2.121f), Quaternion.Euler(315, 0, 0)),
                        Pose: new Pose(new Vector3(0, 2.121f, -2.121f), Quaternion.Euler(315, 0, 0))
                    ),
                    new Vector3(0, 1, 0),
                    new Vector3(3, 3, 3),
                    Space.Self);
            }
        }

        static IEnumerable TranslateInWorldSpaceCase
        {
            get
            {
                var tilt45Pan45 = CreateRigState
                (
                    ARPose: new Pose(new Vector3(0f, 0, 0f), Quaternion.Euler(45f, 45f, 0)),
                    Pose: new Pose(Vector3.zero, Quaternion.Euler(45f, 45f, 0f))
                );

                var tilt315 = CreateRigState
                (
                    ARPose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 0)),
                    Pose: new Pose(Vector3.zero, Quaternion.Euler(315f, 0f, 0f))
                );

                yield return new RigCase(
                    "tilt 45, pan 45, translate forward",
                    tilt45Pan45,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(1.4142f, 0f, 1.4142f), Quaternion.Euler(45f, 45f, 0)),
                        Pose: new Pose(new Vector3(1.4142f, 0f, 1.4142f), Quaternion.Euler(45f, 45f, 0f))
                    ),
                    new Vector3(0, 0, 1),
                    new Vector3(2, 2, 2),
                    Space.Self,
                    Space.World);

                yield return new RigCase(
                    "tilt 45, pan 45, translate right",
                    tilt45Pan45,
                    CreateRigState
                    (
                        LocalPose: new Pose(new Vector3(1.4142f, 0f, -1.4142f), Quaternion.Euler(45f, 45f, 0)),
                        Pose: new Pose(new Vector3(1.4142f, 0f, -1.4142f), Quaternion.Euler(45f, 45f, 0f))
                    ),
                    new Vector3(1, 0, 0),
                    new Vector3(2, 2, 2),
                    Space.Self,
                    Space.World);
            }
        }

        [Test, TestCaseSource("TranslateUsingNonUniformSpeedCase")]
        public void TranslateUsingNonUniformSpeed(RigCase rigCase)
        {
            DoRigCase(rigCase);
        }

        [Test, TestCaseSource("TranslateUsingUniformSpeedCase")]
        public void TranslateUsingUniformSpeed(RigCase rigCase)
        {
            DoRigCase(rigCase);
        }

        [Test, TestCaseSource("TranslateInWorldSpaceCase")]
        public void TranslateInWorldSpace(RigCase rigCase)
        {
            DoRigCase(rigCase);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void WorldSpaceSingularityTranslate(bool runUpdates)
        {
            var rigSettings = VirtualCameraRigSettings.Identity;

            var rig = CreateRigState
            (
                ARPose: new Pose(Vector3.zero, Quaternion.Euler(90f, 0f, 45f)),
                Pose: new Pose(Vector3.zero, Quaternion.Euler(90f, 0f, 45f))
            );
            rig.Refresh(rigSettings);

            rig.Translate(Vector3.forward, 1f, Vector3.one, Space.Self, Space.World, rigSettings);
            if (runUpdates)
                rig.Update(rig.ARPose, rigSettings);

            Assert.That(rig.LocalPose.position, Is.EqualTo(Vector3.zero).Using(m_Vector3Comparer), "Moved in singularity while rebasing was disabled");

            rigSettings.Rebasing = true;
            rig.InitializeJoystickValues();
            if (runUpdates)
                rig.Update(rig.ARPose, rigSettings);

            rig.Translate(Vector3.forward, 2f, Vector3.one, Space.Self, Space.World, rigSettings);
            if (runUpdates)
                rig.Update(rig.ARPose, rigSettings);
            Assert.That(rig.LocalPose.position, Is.EqualTo(new Vector3(-1.4142f, 0f, 1.4142f)).Using(m_Vector3Comparer), "Rotation wasn't adjusted for singularity after rebasing was enabled");
        }

        void DoRigCase(RigCase rigCase)
        {
            var rigSettings = new VirtualCameraRigSettings();

            var rig = rigCase.initialState;
            rig.Refresh(rigSettings);

            rig.Translate(rigCase.direction, 1, rigCase.speed, rigCase.pedestalSpace, rigCase.motionSpace, rigSettings);

            Assert.That(rig.RebaseOffset.eulerAngles, Is.EqualTo(rigCase.expectedState.RebaseOffset.eulerAngles).Using(m_Vector3Comparer), "Rebase offset is different");
            Assert.That(rig.LocalPose.position, Is.EqualTo(rigCase.expectedState.LocalPose.position).Using(m_Vector3Comparer), "Local position is different");
            Assert.That(rig.LocalPose.rotation.eulerAngles, Is.EqualTo(rigCase.expectedState.LocalPose.rotation.eulerAngles).Using(m_Vector3Comparer), "Local rotation is different");
            Assert.That(rig.Pose.position, Is.EqualTo(rigCase.expectedState.Pose.position).Using(m_Vector3Comparer), "Position is different");
            Assert.That(rig.Pose.rotation.eulerAngles, Is.EqualTo(rigCase.expectedState.Pose.rotation.eulerAngles).Using(m_Vector3Comparer), "Rotation is different");
        }
    }
}
