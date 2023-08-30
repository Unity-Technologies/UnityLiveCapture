using System.Collections;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera.Rigs;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.VirtualCamera.Tests
{
    public class VirtualCameraRotateTests : MonoBehaviour
    {
        const float k_CompareTolerance = 0.001f;

        public class RotateStep
        {
            internal VirtualCameraRigSettings settings = VirtualCameraRigSettings.Identity;

            /// <summary>
            /// The raw pose input from the tracker in world space. Note: samples are not deltas.
            /// </summary>
            public Pose ARSample;

            /// <summary>
            /// The rotation speed received from the joystick/gamepad.
            /// </summary>
            public Vector3 RotationSample;
        }

        public class RotateCase
        {
            /// <summary>
            /// The name of the damping case. It appears in the Unity Editor and should describe the initial state, the steps, and the
            /// expected outcome
            /// </summary>
            public string name;
            internal VirtualCameraRigState initialState = VirtualCameraRigState.Identity;
            internal RotateStep[] steps;
            internal VirtualCameraRigState expectedState;

            internal RotateCase(string name, VirtualCameraRigState initialState, VirtualCameraRigState expectedState, RotateStep[] steps)
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

        static VirtualCameraRigSettings CreateSettings(bool rebasing)
        {
            var settings = VirtualCameraRigSettings.Identity;
            settings.Rebasing = rebasing;
            return settings;
        }

        static VirtualCameraRigState CreateInitialRigState(Pose ARPose, Pose? Origin = null)
        {
            var state = VirtualCameraRigState.Identity;

            state.Origin = Origin ?? state.Origin;

            state.ARPose = ARPose;
            state.LastInput = state.ARPose;
            state.LocalPose = ARPose;

            state.Refresh(new VirtualCameraRigSettings());

            return state;
        }

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

        static IEnumerable PanCases
        {
            get
            {
                yield return new RotateCase(
                    "AR on",
                    CreateRigState(),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 30, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, -10, 0)
                        }
                    });

                yield return new RotateCase(
                    "AR off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 30, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, -10, 0)
                        }
                    });

                yield return new RotateCase(
                    "AR toggled on and off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.zero, Quaternion.identity)
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.zero, Quaternion.Euler(0, 99, 0))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.zero, Quaternion.Euler(0, 1, 0)),
                            RotationSample = new Vector3(0, 1, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.zero, Quaternion.Euler(0, 10, 0)),
                            RotationSample = new Vector3(0, 3, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.zero, Quaternion.Euler(0, 100, 0)),
                            RotationSample = new Vector3(0, 4, 0)
                        }
                    });
            }
        }

        [Test, TestCaseSource("PanCases")]
        public void Pan(RotateCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable TiltCases
        {
            get
            {
                yield return new RotateCase(
                    "AR on",
                    CreateRigState(),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 30, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample = new Pose(Vector3.one, Quaternion.Euler(20, 30, 10)),
                            RotationSample = new Vector3(10, 0, 0)
                        }
                    });

                yield return new RotateCase(
                    "AR off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(30, 40, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 50)),
                            RotationSample = new Vector3(10, 0, 0)
                        }
                    });

                yield return new RotateCase(
                    "AR toggled off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(25, 40, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(2, 0, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(3, 0, 0)
                        }
                    });

                yield return new RotateCase(
                    "AR toggled on",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(33, 40, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(2, 0, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(3, 0, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(33, 40, 10)),
                            RotationSample = new Vector3(4, 0, 0)
                        }
                    });
            }
        }

        [Test, TestCaseSource("TiltCases")]
        public void Tilt(RotateCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable RollCases
        {
            get
            {
                yield return new RotateCase(
                    "AR on",
                    CreateRigState(),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 30, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample = new Pose(Vector3.one, Quaternion.Euler(20, 30, 10)),
                            RotationSample = new Vector3(0, 0, 10)
                        }
                    });

                yield return new RotateCase(
                    "AR off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 40, 15))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 50)),
                            RotationSample = new Vector3(0, 0, 5)
                        }
                    });

                yield return new RotateCase(
                    "AR toggled off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 40, 15))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 2)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 3)
                        }
                    });

                yield return new RotateCase(
                    "AR toggled on",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 40, 33))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 2)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 3)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 33)),
                            RotationSample = new Vector3(0, 0, 4)
                        }
                    });
            }
        }

        [Test, TestCaseSource("RollCases")]
        public void Roll(RotateCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable GeneralCases
        {
            get
            {
                yield return new RotateCase(
                    "AR on",
                    CreateRigState(),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample = new Pose(Vector3.one, Quaternion.Euler(20, 30, 10)),
                            RotationSample = new Vector3(10, 10, 10)
                        }
                    });

                yield return new RotateCase(
                    "AR off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(25, 45, 15))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 50)),
                            RotationSample = new Vector3(5, 5, 5)
                        }
                    });

                yield return new RotateCase(
                    "AR toggled off",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 0)
                        }
                    });

                yield return new RotateCase(
                    "AR toggled off and on",
                    CreateInitialRigState
                    (
                        new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    CreateRigState
                    (
                        Pose: new Pose(Vector3.one, Quaternion.Euler(20, 40, 10))
                    ),
                    new[]
                    {
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(true),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 0)
                        },
                        new RotateStep()
                        {
                            settings = CreateSettings(false),
                            ARSample =  new Pose(Vector3.one, Quaternion.Euler(20, 40, 10)),
                            RotationSample = new Vector3(0, 0, 0)
                        }
                    });
            }
        }

        [Test, TestCaseSource("GeneralCases")]
        public void RotateGeneral(RotateCase rigCase)
        {
            DoRigCase(rigCase);
        }

        [Test]
        public void PanAffectsARTranslation()
        {
            var rigCase = new RotateCase(
                "Pan affects AR translation",
                CreateInitialRigState
                (
                    new Pose(Vector3.one, Quaternion.Euler(0, 0, 0))
                ),
                CreateRigState
                (
                    Pose: new Pose(new Vector3(2, 1, 1), Quaternion.Euler(0, 90, 0))
                ),
                new[]
                {
                    new RotateStep()
                    {
                        settings = CreateSettings(false),
                        ARSample =  new Pose(Vector3.one, Quaternion.Euler(0, 0, 0)),
                        RotationSample = new Vector3(0, 90, 0)
                    },
                    new RotateStep()
                    {
                        settings = CreateSettings(false),
                        ARSample =  new Pose(new Vector3(1, 1, 2), Quaternion.Euler(0, 0, 0)),
                        RotationSample = new Vector3(0, 0, 0)
                    }
                });

            DoRigCase(rigCase);
        }

        [Test]
        public void WorldSpaceSingularityRotate()
        {
            var rigSettings = VirtualCameraRigSettings.Identity;

            var rig = CreateRigState
            (
                ARPose: new Pose(Vector3.zero, Quaternion.Euler(90f, 0f, 45f)),
                Pose: new Pose(Vector3.zero, Quaternion.Euler(90f, 0f, 45f))
            );
            rig.Refresh(rigSettings);

            rigSettings.Rebasing = true;
            rig.InitializeJoystickValues();
            rig.Update(rig.ARPose, rigSettings);
            Assert.That(rig.LocalPose.rotation.eulerAngles, Is.EqualTo(new Vector3(90f, 315f, 0f)).Using(m_Vector3Comparer), "Rotation wasn't adjusted for singularity after rebasing was enabled");

            rigSettings.Rebasing = false;
            rig.Update(rig.ARPose, rigSettings);
            Assert.That(rig.LocalPose.rotation.eulerAngles, Is.EqualTo(new Vector3(90f, 315f, 0f)).Using(m_Vector3Comparer), "Rotation jumped after rebasing was disabled");
        }

        void DoRigCase(RotateCase rigCase)
        {
            var rig = rigCase.initialState;

            foreach (var step in rigCase.steps)
            {
                if (step.ARSample != null)
                {
                    rig.Update(step.ARSample, step.settings);
                }

                if (step.RotationSample != null)
                {
                    rig.Rotate(step.RotationSample, 1f, Vector3.one, step.settings);
                }
            }

            Assert.That(rig.Pose.position, Is.EqualTo(rigCase.expectedState.Pose.position).Using(m_Vector3Comparer), "Position is different");
            Assert.That(rig.Pose.rotation.eulerAngles, Is.EqualTo(rigCase.expectedState.Pose.rotation.eulerAngles).Using(m_Vector3Comparer), "Rotation is different");
        }
    }
}
