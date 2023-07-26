using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera.Rigs;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.VirtualCamera.Tests
{
    public class VirtualCameraDampingTests : MonoBehaviour
    {
        const float k_CompareTolerance = 0.001f;

        public class DampingStep
        {
            internal Damping settings = Damping.Default;
            /// <summary>
            /// The raw pose input from the tracker in world space. Note: samples are not deltas.
            /// </summary>
            public Pose[] samples;
        }

        public class DampingCase
        {
            /// <summary>
            /// The name of the damping case. It appears in the Unity Editor and should describe the initial state, the steps, and the
            /// expected outcome
            /// </summary>
            public string name;
            internal Pose initialState = Pose.identity;
            internal float deltaTime = 0.1f;
            internal DampingStep[] steps;
            internal Pose expectedState;

            internal DampingCase(string name, Pose initialState, Pose expectedState, DampingStep[] steps, float deltaTime)
            {
                this.name = name;
                this.initialState = initialState;
                this.expectedState = expectedState;
                this.steps = steps;
                this.deltaTime = deltaTime;
            }

            public override string ToString()
            {
                return name;
            }
        }

        Vector3EqualityComparer m_Vector3Comparer = new Vector3EqualityComparer(k_CompareTolerance);
        QuaternionEqualityComparer m_QuaternionComparer = new QuaternionEqualityComparer(k_CompareTolerance);


        static Damping CreateSettings(Vector3 body, float aim, bool enabled)
        {
            var settings = new Damping()
            {
                Body = body,
                Aim = aim,
                Enabled = enabled
            };
            return settings;
        }

        static IEnumerable NoDampingApplied
        {
            get
            {
                yield return new DampingCase(
                    "set damping enabled at body 0, aim 0, enabled true: No damping applied",
                    new Pose()
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity
                    },
                    new Pose()
                    {
                        position = Vector3.one,
                        rotation = Quaternion.Euler(180, 0, 0)
                    },
                    new DampingStep[]
                    {
                        new DampingStep()
                        {
                            settings = CreateSettings(Vector3.zero, 0, true),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(180, 0, 0))}
                        }
                    },
                    0.5f);

                yield return new DampingCase(
                    "set damping enabled at body 10, aim 10, enabled false: No damping applied",
                    new Pose()
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity
                    },
                    new Pose()
                    {
                        position = Vector3.one,
                        rotation = Quaternion.Euler(180, 0, 0)
                    },
                    new DampingStep[]
                    {
                        new DampingStep()
                        {
                            settings = CreateSettings(new Vector3(10, 10, 10), 10, false),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(180, 0, 0))}
                        }
                    },
                    0.5f);
            }
        }

        [Test, TestCaseSource("NoDampingApplied")]
        public void DampingEnabledDisabled(DampingCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable DampingRegularCases
        {
            get
            {
                yield return new DampingCase("set damping enabled at body 1, aim 1, enabled true: Damping applied",
                    new Pose()
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity
                    },
                    new Pose()
                    {
                        position = new Vector3(0.9f, 0.9f, 0.9f),
                        rotation = Quaternion.Euler(180, 0, 0)
                    },
                    new DampingStep[]
                    {
                        new DampingStep()
                        {
                            settings = CreateSettings(Vector3.one, 0.1f, true),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(180, 0, 0))}
                        }
                    },
                    0.5f);

                yield return new DampingCase(
                    "set damping enabled at body 10, aim 10, enabled true: Damping applied",
                    new Pose()
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity
                    },
                    new Pose()
                    {
                        position = new Vector3(0.2056f, 0.2056f, 0.2056f),
                        rotation = Quaternion.Euler(0, 37, 37)
                    },
                    new DampingStep[]
                    {
                        new DampingStep()
                        {
                            settings = CreateSettings(new Vector3(10f, 10f, 10f), 10f, true),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(180, 0, 0))}
                        }
                    },
                    0.5f);
            }
        }

        [Test, TestCaseSource("DampingRegularCases")]
        public void DampingNormalCases(DampingCase rigCase)
        {
            DoRigCase(rigCase);
        }

        static IEnumerable DampingUnHandledCases
        {
            get
            {
                yield return new DampingCase(
                    "set damping enabled at body -1, aim -1, enabled true: No Damping applied",
                    new Pose()
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity
                    },
                    new Pose()
                    {
                        position = Vector3.one,
                        rotation = Quaternion.Euler(180, 0, 0)
                    },
                    new DampingStep[]
                    {
                        new DampingStep()
                        {
                            settings = CreateSettings(new Vector3(-1, -1, -1), -1f, true),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(180, 0, 0))}
                        }
                    },
                    0.5f);

                yield return new DampingCase(
                    "set damping enabled at body -1, aim 10, enabled true: Damping applied on rotation only",
                    new Pose()
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity
                    },
                    new Pose()
                    {
                        position = Vector3.one,
                        rotation = Quaternion.Euler(0, 37, 37)
                    },
                    new DampingStep[]
                    {
                        new DampingStep()
                        {
                            settings = CreateSettings(new Vector3(-1, -1, -1), 10f, true),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(180, 0, 0))}
                        }
                    },
                    0.5f);

                yield return new DampingCase(
                    "set damping enabled at body 1, aim -1, enabled true: Damping applied on position only",
                    new Pose()
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity
                    },
                    new Pose()
                    {
                        position = new Vector3(0.9f, 0.9f, 0.9f),
                        rotation = Quaternion.Euler(70, 0, 0),
                    },
                    new DampingStep[]
                    {
                        new DampingStep()
                        {
                            settings = CreateSettings(new Vector3(1, 1, 1), -1f, true),
                            samples =  new Pose[] {new Pose(Vector3.one, Quaternion.Euler(70, 0, 0))}
                        }
                    },
                    0.5f);
            }
        }

        [Test, TestCaseSource("DampingUnHandledCases")]
        public void DampingNotHandledCases(DampingCase rigCase)
        {
            DoRigCase(rigCase);
        }

        void DoRigCase(DampingCase rigCase)
        {
            var currentPose = rigCase.initialState;
            foreach (var step in rigCase.steps)
            {
                foreach (var sample in step.samples)
                {
                    currentPose = VirtualCameraDamping.Calculate(currentPose, sample, step.settings, rigCase.deltaTime);
                }
            }
            Assert.That(currentPose.rotation.eulerAngles, Is.EqualTo(rigCase.expectedState.rotation.eulerAngles).Using(m_Vector3Comparer), "Rotation is different");
            Assert.That(currentPose.position, Is.EqualTo(rigCase.expectedState.position).Using(m_Vector3Comparer), "Position is different");
        }
    }
}
