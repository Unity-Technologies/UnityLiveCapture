using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera.Rigs;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.VirtualCamera.Tests
{
    public class VirtualCameraRigTests
    {
        const float k_CompareTolerance = 0.001f;

        public class RigStep
        {
            internal VirtualCameraRigSettings settings = VirtualCameraRigSettings.Identity;
            /// <summary>
            /// The raw pose input from the tracker in world space. Note: samples are not deltas.
            /// </summary>
            public Pose[] samples;
        }

        public class RigCase
        {
            /// <summary>
            /// The name of the case. Will appear in the editor. Should describe the initial state, the steps, and the
            /// expected outcome
            /// </summary>
            public string name;
            internal VirtualCameraRigState initialState = VirtualCameraRigState.Identity;
            public RigStep[] steps;
            internal VirtualCameraRigState expectedState;

            internal RigCase(string name, VirtualCameraRigState expectedState, RigStep[] steps)
            {
                this.name = name;
                this.expectedState = expectedState;
                this.steps = steps;
            }

            internal RigCase(string name, VirtualCameraRigState initialState, VirtualCameraRigState expectedState, RigStep[] steps)
            {
                this.name = name;
                this.initialState = initialState;
                this.expectedState = expectedState;
                this.steps = steps;
            }

            public override string ToString()
            {
                return name;
            }
        }

        Vector3EqualityComparer m_Vector3Comparer = new Vector3EqualityComparer(k_CompareTolerance);
        QuaternionEqualityComparer m_QuaternionComparer = new QuaternionEqualityComparer(k_CompareTolerance);

        static VirtualCameraRigSettings CreateSettings(bool rebasing)
        {
            var settings = VirtualCameraRigSettings.Identity;
            settings.Rebasing = rebasing;
            return settings;
        }

        static VirtualCameraRigSettings CreateSettings(bool rebasing, float ergonomicTilt)
        {
            var settings = CreateSettings(rebasing);
            settings.ErgonomicTilt = ergonomicTilt;
            return settings;
        }

        static VirtualCameraRigSettings CreateSettings(bool rebasing, RotationAxis rotationAxis)
        {
            var settings = CreateSettings(rebasing);
            settings.RotationLock = rotationAxis;
            return settings;
        }

        static VirtualCameraRigSettings CreateSettings(bool rebasing, float ergonomicTilt, RotationAxis rotationAxis)
        {
            var settings = CreateSettings(rebasing, ergonomicTilt);
            settings.ErgonomicTilt = ergonomicTilt;
            settings.RotationLock = rotationAxis;
            return settings;
        }

        static VirtualCameraRigState CreateRigState(Quaternion? RebaseOffset = null, Pose? ARPose = null, Pose? Pose = null, Pose? Origin = null)
        {
            var state = VirtualCameraRigState.Identity;
            state.RebaseOffset = RebaseOffset ?? state.RebaseOffset;
            state.ARPose = ARPose ?? state.ARPose;
            state.Pose = Pose ?? state.Pose;
            state.Origin = Origin ?? state.Origin;

            return state;
        }

        static IEnumerable ErgonomicTiltCases
        {
            get
            {
                yield return new RigCase(
                    "set ergonomic tilt 30, pan 30, expect rotation x 30 y 30",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, Quaternion.Euler(30f, 30f, 0)),
                        Pose: new Pose(Vector3.zero, Quaternion.Euler(30f, 30f, 0f))
                    ),
                    new RigStep[]
                    {
                        new RigStep()
                        {
                            settings = CreateSettings(false, 30f),
                            samples =  new Pose[] {new Pose(Vector3.zero, Quaternion.Euler(0f, 30f, 0))}
                        }
                    });

                yield return new RigCase(
                    "pan 30; set ergonomic tilt 30, pan 30; set ergo tilt 30, pan 30 with rotation lock pan, expect rotation x 30 y 30",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, Quaternion.Euler(30f, 30f, 0f)),
                        Pose: new Pose(Vector3.zero, Quaternion.Euler(30f, 30f, 0f))
                    ),
                    new RigStep[]
                    {
                        new RigStep()
                        {
                            settings = CreateSettings(false),
                            samples =  new Pose[] {new Pose(Vector3.zero, Quaternion.Euler(0f, 30f, 0f))}
                        },
                        new RigStep()
                        {
                            settings = CreateSettings(true, 30f),
                            samples =  new Pose[] {new Pose(Vector3.zero, Quaternion.Euler(0f, 30f, 0f))}
                        },
                        new RigStep()
                        {
                            settings = CreateSettings(false, 30f, RotationAxis.Pan),
                            samples =  new Pose[] {new Pose(Vector3.zero, Quaternion.Euler(0f, 30f, 0f))}
                        }
                    });

                yield return new RigCase(
                    "Test ergonomic tilt should affect rotation on tilt locked",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.one, Quaternion.Euler(20, 30, 0)),
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 30, 0))
                    ),
                    new RigStep[]
                    {
                        new RigStep()
                        {
                            settings = CreateSettings(false, 0f),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(0, 30, 0))}
                        },
                        new RigStep()
                        {
                            settings = CreateSettings(false, 80f, RotationAxis.Tilt),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(0, 30, 0))}
                        },
                        new RigStep()
                        {
                            settings = CreateSettings(false, 10f, RotationAxis.Tilt),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(0, 30, 0))}
                        },
                        new RigStep()
                        {
                            settings = CreateSettings(false, 20f, RotationAxis.Tilt),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(0, 30, 0))}
                        }
                    });

                yield return new RigCase(
                    "set ergonomic tilt 30 tilt -30, move forward 10, expect position x 0 y 0 z 10",
                    CreateRigState
                    (
                        /// tilt -30 ergonomic tilt 30 should cancel the rotation therefore it is like moving with a rotation of 0
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(new Vector3(0, 0, 10), Quaternion.Euler(0, 0, 0)),
                        Pose: new Pose(new Vector3(0, 0, 10), Quaternion.Euler(0, 0, 0))
                    ),
                    new RigStep[]
                    {
                        new RigStep()
                        {
                            settings = CreateSettings(false, 30f),
                            samples =  new Pose[] {new Pose(new Vector3(0, 0, 10), Quaternion.Euler(-30f, 0f, 0))}
                        }
                    });

                yield return new RigCase(
                    "set ergonomic tilt 30 tilt -30, move forward x 10 y 2 z 5, expect position x 10 y 2 z 5",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(new Vector3(10, 2, 5), Quaternion.Euler(0, 180, 0)),
                        Pose: new Pose(new Vector3(10, 2, 5), Quaternion.Euler(0, 180, 0))
                    ),
                    new RigStep[]
                    {
                        new RigStep()
                        {
                            settings = CreateSettings(false, 30f),
                            samples =  new Pose[] {new Pose(new Vector3(10, 2, 5), Quaternion.Euler(-30f, 180f, 0))}
                        }
                    });
            }
        }

        [Test, TestCaseSource("ErgonomicTiltCases")]
        public void ErgonomicTilt(RigCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable RotationLockCases
        {
            get
            {
                var rot_45x = Quaternion.Euler(45f, 0f, 0f);
                var rot_90x = Quaternion.Euler(90f, 0f, 0f);
                var rot_91x = Quaternion.Euler(91f, 0f, 0f);
                var rot_45y = Quaternion.Euler(0f, 45f, 0f);
                var rot_90y = Quaternion.Euler(0f, 90f, 0f);

                yield return new RigCase(
                    "tilt 45, with tilt lock, doesn't rotate",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, Quaternion.identity),
                        Pose: new Pose(Vector3.zero, Quaternion.identity)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(false, RotationAxis.Tilt),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45x) }
                        },
                    });

                yield return new RigCase(
                    "pan 45, with pan lock, doesn't rotate",
                    CreateRigState
                    (
                        RebaseOffset: rot_45y, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, Quaternion.identity),
                        Pose: new Pose(Vector3.zero, Quaternion.identity)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(false, RotationAxis.Pan),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45y) }
                        },
                    });

                yield return new RigCase(
                    "pan 45, with pan lock, doesn't rotate, unlock pan keeps the Y rotation",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.Euler(new Vector3(0, 90, 0)), // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, Quaternion.Euler(0, 315, 0)),
                        Pose: new Pose(Vector3.zero, Quaternion.Euler(0, 315, 0))
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(false, RotationAxis.Pan),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45y) }
                        },
                        new RigStep() {
                            settings = CreateSettings(false, RotationAxis.Pan),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_90y) }
                        },
                        new RigStep() {
                            settings = CreateSettings(false),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45y) }
                        }
                    });

                yield return new RigCase(
                    "tilt 90, with tilt lock, doesn't rotate",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, Quaternion.identity),
                        Pose: new Pose(Vector3.zero, Quaternion.identity)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(false, RotationAxis.Tilt),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_90x) }
                        },
                    });

                // yield return new RigCase(
                //     "tilt 91, with tilt lock, doesn't rotate",
                //     CreateRigState (
                //         rebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                //         arPose: new Pose(Vector3.zero, Quaternion.identity),
                //         pose: new Pose(Vector3.zero, Quaternion.identity)
                //     ),
                //     new RigStep[]
                //     {
                //         new RigStep() {
                //             settings = CreateSettings(false, RotationAxis.Tilt),
                //             samples = new Pose[] { new Pose(Vector3.zero, rot_91x) }
                //         },
                //     });
            }
        }

        [Test, TestCaseSource("RotationLockCases")]
        public void RotationLock(RigCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable RebaseCases
        {
            get
            {
                var rot_45x = Quaternion.Euler(45f, 0f, 0f);
                var rot_45y = Quaternion.Euler(0f, 45f, 0f);
                var rot_45z = Quaternion.Euler(0f, 0f, 45f);
                var rot_45xy = rot_45y * rot_45x;
                var rot_45xz = rot_45x * rot_45z;
                var rot_45yz = rot_45y * rot_45z;
                var pos_1xyz_rot_15x45y10z = new Pose(Vector3.one, Quaternion.Euler(15f, 45f, 10f));

                yield return new RigCase(
                    "from origin, with no movement, remains at origin",
                    VirtualCameraRigState.Identity,
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { Pose.identity }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with position, remains at origin",
                    VirtualCameraRigState.Identity,
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.right, Quaternion.identity) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation X, rotates on X axis",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, rot_45x),
                        Pose: new Pose(Vector3.zero, rot_45x)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45x) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation Y, does not rotate",
                    CreateRigState
                    (
                        RebaseOffset: rot_45y,
                        ARPose: Pose.identity,
                        Pose: Pose.identity
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45y) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation Z, rotates on Z axis",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity, // Rebase only applies on the Y axis
                        ARPose: new Pose(Vector3.zero, rot_45z),
                        Pose: new Pose(Vector3.zero, rot_45z)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45z) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation XY, rotates on X axis",
                    CreateRigState
                    (
                        RebaseOffset: rot_45y,
                        ARPose: new Pose(Vector3.zero, rot_45x),
                        Pose: new Pose(Vector3.zero, rot_45x)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45xy) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation XZ, rotates on XZ axis",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity,
                        ARPose: new Pose(Vector3.zero, rot_45xz),
                        Pose: new Pose(Vector3.zero, rot_45xz)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45xz) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation YZ, rotates on Z axis",
                    CreateRigState
                    (
                        RebaseOffset: rot_45y,
                        ARPose: new Pose(Vector3.zero, rot_45z),
                        Pose: new Pose(Vector3.zero, rot_45z)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45yz) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with no movement, remains at origin",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z,
                        RebaseOffset: Quaternion.identity,
                        ARPose: Pose.identity,
                        Pose: pos_1xyz_rot_15x45y10z
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { Pose.identity }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with position, remains at origin",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity,
                        ARPose: Pose.identity,
                        Pose: pos_1xyz_rot_15x45y10z
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.right, Quaternion.identity) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation X, remains at origin and rotates on X axis",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity,
                        ARPose: new Pose(Vector3.zero, rot_45x),
                        Pose: new Pose(pos_1xyz_rot_15x45y10z.position, pos_1xyz_rot_15x45y10z.rotation * rot_45x)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45x) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation Y, remains at origin and does not rotate",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        RebaseOffset: rot_45y,
                        ARPose: Pose.identity,
                        Pose: pos_1xyz_rot_15x45y10z
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45y) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation Z, remains at origin and rotates on Z axis",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity,
                        ARPose: new Pose(Vector3.zero, rot_45z),
                        Pose: new Pose(pos_1xyz_rot_15x45y10z.position, pos_1xyz_rot_15x45y10z.rotation * rot_45z)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45z) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation XY, remains at origin and rotates on X axis",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        RebaseOffset: rot_45y,
                        ARPose: new Pose(Vector3.zero, rot_45x),
                        Pose: new Pose(pos_1xyz_rot_15x45y10z.position, pos_1xyz_rot_15x45y10z.rotation * rot_45x)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45xy) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation XZ, remains at origin and rotates on XZ axis",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity,
                        ARPose: new Pose(Vector3.zero, rot_45xz),
                        Pose: new Pose(pos_1xyz_rot_15x45y10z.position, pos_1xyz_rot_15x45y10z.rotation * rot_45xz)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45xz) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "from origin, with rotation YZ, remains at origin and rotates on Z axis",
                    CreateRigState
                    (
                        Origin: pos_1xyz_rot_15x45y10z
                    ),
                    CreateRigState
                    (
                        RebaseOffset: rot_45y,
                        ARPose: new Pose(Vector3.zero, rot_45z),
                        Pose: new Pose(pos_1xyz_rot_15x45y10z.position, pos_1xyz_rot_15x45y10z.rotation * rot_45z)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45yz) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });

                yield return new RigCase(
                    "Sequence rotation 45X, rotation 45XY, rotation 45X: should have rebase offset zero and rotation 45 on the X axis",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity,
                        ARPose: new Pose(Vector3.zero, rot_45x),
                        Pose: new Pose(Vector3.zero, rot_45x)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(false),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45x) }
                        },
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45xy) }
                        },
                        new RigStep() { settings = CreateSettings(false) },
                        new RigStep() {
                            settings = CreateSettings(true),
                            samples = new Pose[] { new Pose(Vector3.zero, rot_45x) }
                        },
                        new RigStep() { settings = CreateSettings(false) }
                    });
            }
        }

        [Test, TestCaseSource("RebaseCases")]
        public void Rebase(RigCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable MotionCases
        {
            get
            {
                var rot_45y = Quaternion.Euler(0f, 45f, 0f);

                yield return new RigCase(
                    "move forward, rotate 45Y, move right: position is 1,0,1 rotation is 45Y",
                    CreateRigState
                    (
                        RebaseOffset: Quaternion.identity,
                        ARPose: new Pose(new Vector3(1f, 0f, 1f), rot_45y),
                        Pose: new Pose(new Vector3(1f, 0f, 1f), rot_45y)
                    ),
                    new RigStep[]
                    {
                        new RigStep() {
                            settings = CreateSettings(false),
                            samples = new Pose[] { new Pose(new Vector3(0f, 0f, 1f), Quaternion.identity) }
                        },
                        new RigStep() {
                            settings = CreateSettings(false),
                            samples = new Pose[] { new Pose(new Vector3(1f, 0f, 1f), rot_45y) }
                        }
                    });
            }
        }

        [Test, TestCaseSource("MotionCases")]
        public void Motion(RigCase rigCase)
        {
            DoRigCase(rigCase);
        }

        void DoRigCase(RigCase rigCase)
        {
            var rig = rigCase.initialState;

            foreach (var step in rigCase.steps)
            {
                if (step.samples != null)
                {
                    foreach (var sample in step.samples)
                    {
                        rig.Update(sample, step.settings);
                    }
                }
                else
                {
                    rig.Update(rig.LastInput, step.settings);
                }
            }

            Assert.That(rig.RebaseOffset.eulerAngles, Is.EqualTo(rigCase.expectedState.RebaseOffset.eulerAngles).Using(m_Vector3Comparer), "Rebase offset is different");
            Assert.That(rig.ARPose.position, Is.EqualTo(rigCase.expectedState.ARPose.position).Using(m_Vector3Comparer), "Local position is different");
            Assert.That(rig.ARPose.rotation.eulerAngles, Is.EqualTo(rigCase.expectedState.ARPose.rotation.eulerAngles).Using(m_Vector3Comparer), "Local rotation is different");
            Assert.That(rig.Pose.position, Is.EqualTo(rigCase.expectedState.Pose.position).Using(m_Vector3Comparer), "Position is different");
            Assert.That(rig.Pose.rotation.eulerAngles, Is.EqualTo(rigCase.expectedState.Pose.rotation.eulerAngles).Using(m_Vector3Comparer), "Rotation is different");
        }
    }
}
