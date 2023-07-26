using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.LiveCapture.VirtualCamera.Rigs;
using UnityEngine.TestTools.Utils;

namespace Unity.LiveCapture.VirtualCamera.Tests
{
    class PositionLockTests
    {
        const float k_CompareTolerance = 0.001f;

        Vector3EqualityComparer m_Vector3Comparer = new Vector3EqualityComparer(k_CompareTolerance);
        VirtualCameraRigState m_State;
        VirtualCameraRigSettings m_Settings;

        [SetUp]
        public void Setup()
        {
            m_State = VirtualCameraRigState.Identity;
            m_Settings = VirtualCameraRigSettings.Identity;
        }

        static IEnumerable PositionConstraintCases
        {
            get
            {
                yield return new TestCaseData(PositionAxis.None, Vector3.zero, Vector3.zero, Vector3.forward, Vector3.forward);
                yield return new TestCaseData(PositionAxis.None, Vector3.zero, Vector3.zero, Vector3.right, Vector3.right);
                yield return new TestCaseData(PositionAxis.None, Vector3.zero, Vector3.zero, Vector3.up, Vector3.up);
                yield return new TestCaseData(PositionAxis.None, Vector3.zero, new Vector3(0f, 90f, 0f), Vector3.back, Vector3.back);
                yield return new TestCaseData(PositionAxis.None, Vector3.zero, new Vector3(0f, 90f, 0f), Vector3.left, Vector3.left);
                yield return new TestCaseData(PositionAxis.None, Vector3.zero, new Vector3(-90f, 0f, 0f), Vector3.down, Vector3.down);
                yield return new TestCaseData(PositionAxis.Dolly, Vector3.zero, Vector3.zero, Vector3.forward, Vector3.zero);
                yield return new TestCaseData(PositionAxis.Truck, Vector3.zero, Vector3.zero, Vector3.right, Vector3.zero);
                yield return new TestCaseData(PositionAxis.Pedestal, Vector3.zero, Vector3.zero, Vector3.up, Vector3.zero);
                yield return new TestCaseData(PositionAxis.Dolly, Vector3.zero, new Vector3(0f, 90f, 0f), Vector3.right, Vector3.zero);
                yield return new TestCaseData(PositionAxis.Truck, Vector3.zero, new Vector3(0f, 90f, 0f), Vector3.back, Vector3.zero);
                yield return new TestCaseData(PositionAxis.Pedestal, Vector3.zero, new Vector3(-90f, 0f, 0f), Vector3.forward, Vector3.zero);
                yield return new TestCaseData(PositionAxis.Dolly | PositionAxis.Truck, Vector3.zero, Vector3.zero, Vector3.forward + Vector3.right, Vector3.zero);
                yield return new TestCaseData(PositionAxis.Dolly | PositionAxis.Truck, Vector3.zero, Vector3.zero, Vector3.one, Vector3.up);
                yield return new TestCaseData(PositionAxis.Dolly | PositionAxis.Truck, Vector3.zero, new Vector3(0f, 90f, 0f), Vector3.one, Vector3.up);
            }
        }

        [Test, TestCaseSource("PositionConstraintCases")]
        public void PositionConstraint(
            PositionAxis positionLock,
            Vector3 position,
            Vector3 rotation,
            Vector3 deltaPosition,
            Vector3 expectedConstrainedPosition)
        {
            var pose = new Pose()
            {
                position = position,
                rotation = Quaternion.Euler(rotation),
            };

            m_State.LastInput = pose;
            m_State.ARPose = pose;
            m_State.Pose = pose;
            m_Settings.PositionLock = positionLock;

            pose.position += deltaPosition;
            m_State.Update(pose, m_Settings);

            Assert.That(m_State.Pose.position, Is.EqualTo(expectedConstrainedPosition).Using(m_Vector3Comparer));
        }

        [Test]
        public void DeactivateAxisLockKeepsPosition()
        {
            m_State.LastInput = Pose.identity;
            m_State.ARPose = Pose.identity;
            m_State.Pose = Pose.identity;
            m_State.Origin = Pose.identity;
            m_Settings.PositionLock = PositionAxis.Dolly;

            var pose = new Pose(Vector3.forward, Quaternion.identity);

            m_State.Update(pose, m_Settings);

            Assert.That(m_State.Pose.position, Is.EqualTo(Vector3.zero).Using(m_Vector3Comparer));

            m_Settings.PositionLock = PositionAxis.None;

            m_State.Update(pose, m_Settings);

            Assert.That(m_State.Pose.position, Is.EqualTo(Vector3.zero).Using(m_Vector3Comparer));
        }
    }
}
