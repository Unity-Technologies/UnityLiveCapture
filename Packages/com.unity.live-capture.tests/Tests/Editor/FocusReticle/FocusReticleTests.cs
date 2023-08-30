using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;

namespace Unity.LiveCapture.Tests.Editor
{
    [TestFixture]
    class FocusReticleTests
    {
        // Nothing fancy, just make sure normalized and screen coordinates are not the same.
        class CoordinatesTransformStub : BaseFocusReticleControllerImplementation.ICoordinatesTransform
        {
            public static readonly Vector2 k_Resolution = new Vector2(1200, 800);

            public Vector2 NormalizedToScreen(Vector2 normalizedPosition)
            {
                return normalizedPosition * k_Resolution;
            }

            public Vector2 NormalizeScreenPoint(Vector2 screenPosition)
            {
                return screenPosition / k_Resolution;
            }
        }

        static readonly FocusMode[] k_AllFocusModes = {FocusMode.Clear, FocusMode.Manual, FocusMode.ReticleAF, FocusMode.TrackingAF};

        readonly CoordinatesTransformStub m_CoordinatesTransformStub = new CoordinatesTransformStub();
        readonly BaseFocusReticleControllerImplementation m_Implementation = new BaseFocusReticleControllerImplementation();
        readonly IFocusReticle m_FocusReticle = Substitute.For<IFocusReticle>();

        [SetUp]
        public void Setup()
        {
            m_Implementation.FocusReticle = m_FocusReticle;
            m_Implementation.CoordinatesTransform = m_CoordinatesTransformStub;
            m_Implementation.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            m_Implementation.Dispose();
        }

        [TestCase(FocusMode.Clear)]
        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.ReticleAF)]
        [TestCase(FocusMode.TrackingAF)]
        public void ReticleIsVisible_NoPendingEvent_PositionIsNotSentInAnyMode(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, true);

            m_Implementation.PendingDrag = false;
            m_Implementation.PendingTap = false;

            Assert.False(m_Implementation.ShouldSendPosition(out _));
        }

        [TestCase(FocusMode.Clear)]
        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.ReticleAF)]
        [TestCase(FocusMode.TrackingAF)]
        public void ReticleIsInvisible_ReticleIsTappedAndOrDragged_PositionIsNotSentInAnyMode(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, false);

            m_Implementation.PendingDrag = true;
            m_Implementation.PendingTap = true;

            Assert.False(m_Implementation.ShouldSendPosition(out _));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FocusModeIsClear_ReticleIsTappedAndOrDragged_PositionIsNotSent(bool tapped, bool dragged)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, FocusMode.Clear, true);

            m_Implementation.PendingDrag = false;
            m_Implementation.PendingTap = true;

            Assert.False(m_Implementation.ShouldSendPosition(out _));
        }

        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.ReticleAF)]
        [TestCase(FocusMode.TrackingAF)]
        public void FocusModeIsNotClear_ReticleIsTapped_PositionIsSent(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, true);

            m_Implementation.PendingDrag = false;
            m_Implementation.PendingTap = true;

            Assert.IsTrue(m_Implementation.ShouldSendPosition(out _));
        }

        [Test]
        public void FocusModeIsReticleAF_ReticleIsDragged_PositionIsSent()
        {
            m_Implementation.UpdateView(Vector2.one * .5f, FocusMode.ReticleAF, true);

            m_Implementation.PendingDrag = true;
            m_Implementation.PendingTap = false;

            Assert.IsTrue(m_Implementation.ShouldSendPosition(out _));
        }

        [TestCase(FocusMode.Clear)]
        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.TrackingAF)]
        public void FocusModeIsNotReticleAF_ReticleIsDragged_PositionIsNotSent(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, true);

            m_Implementation.PendingDrag = true;
            m_Implementation.PendingTap = false;

            Assert.IsFalse(m_Implementation.ShouldSendPosition(out _));
        }

        static IEnumerable<(FocusMode focusMode, bool drag, bool tap)> CombineFocusModeAndEventsOptions()
        {
            var tapModes = new[] {false, true};
            var dragModes = new[] {false, true};

            foreach (var focusMode in k_AllFocusModes)
            {
                foreach (var isTap in tapModes)
                {
                    foreach (var isDrag in dragModes)
                    {
                        yield return (focusMode, isTap, isDrag);
                    }
                }
            }
        }

        [Test]
        public void ReticleIsNotVisible_ReticleIsTappedAndOrDragged_PositionIsNotSentInAnyMode(
            [ValueSource(nameof(CombineFocusModeAndEventsOptions))]
                (FocusMode focusMode, bool drag, bool tap) input)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, input.focusMode, false);
            m_Implementation.PendingDrag = input.drag;
            m_Implementation.PendingTap = input.tap;

            Assert.False(m_Implementation.ShouldSendPosition(out _));
        }

        [Test]
        public void PositionIsNotValid_ReticleIsTappedAndOrDragged_PositionIsNotSentInAnyMode(
            [ValueSource(nameof(CombineFocusModeAndEventsOptions))]
                (FocusMode focusMode, bool drag, bool tap) input)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, input.focusMode, true);
            m_Implementation.PendingDrag = input.drag;
            m_Implementation.PendingTap = input.tap;
            m_Implementation.LastPointerPosition = Vector2.one * -1024;
            Assert.False(m_Implementation.ShouldSendPosition(out _));
        }

        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.ReticleAF)]
        [TestCase(FocusMode.TrackingAF)]
        public void FocusModeIsNotClear_ReticleIsTappedAndDragged_PositionIsNotSentTwice(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, true);
            m_Implementation.PendingDrag = true;
            m_Implementation.PendingTap = true;

            Assert.True(m_Implementation.ShouldSendPosition(out _));
            Assert.False(m_Implementation.ShouldSendPosition(out _));
        }

        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.TrackingAF)]
        public void FocusModeIsManualOrTrackingAF_ReticleMoved_ReticleIsAnimatedAndHidden(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .2f, focusMode, true);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .4f, focusMode, true);
            m_FocusReticle.Received().Animate(true);
        }

        [Test]
        public void FocusModeIsClear_ReticleMoved_ReticleIsNotAnimated()
        {
            m_Implementation.UpdateView(Vector2.one * .2f, FocusMode.Clear, true);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .4f, FocusMode.Clear, true);
            m_FocusReticle.DidNotReceive().Animate(Arg.Any<bool>());
        }

        [Test]
        public void FocusModeIsReticleAF_ReticleMovedExternal_ReticleIsNotAnimated()
        {
            m_Implementation.UpdateView(Vector2.one * .2f, FocusMode.ReticleAF, true);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .4f, FocusMode.ReticleAF, true);
            m_FocusReticle.DidNotReceive().Animate(Arg.Any<bool>());
        }

        [Test]
        public void FocusModeIsReticleAF_ReticleMovedInternal_ReticleIsAnimated()
        {
            m_Implementation.UpdateView(Vector2.one * .2f, FocusMode.ReticleAF, true);
            m_FocusReticle.ClearReceivedCalls();
            var newPosition = Vector2.one * .4f;
            UpdateLastSentPosition(newPosition);
            m_Implementation.UpdateView(newPosition, FocusMode.ReticleAF, true);
            m_FocusReticle.Received().Animate(false);
        }

        [TestCase(FocusMode.Clear)]
        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.ReticleAF)]
        [TestCase(FocusMode.TrackingAF)]
        public void ReticleIsVisible_ReticleIsDragged_ReticleIsNotAnimatedInAnyMode(FocusMode focusMode)
        {
            m_Implementation.IsDragging = true;
            m_Implementation.UpdateView(Vector2.one * .2f, focusMode, true);
            m_FocusReticle.ClearReceivedCalls();
            var newPosition = Vector2.one * .4f;
            UpdateLastSentPosition(newPosition);
            m_Implementation.UpdateView(newPosition, focusMode, true);
            m_FocusReticle.DidNotReceive().Animate(Arg.Any<bool>());
        }

        static IEnumerable<(FocusMode focusModeA, FocusMode focusModeB)> CombineFocusModes()
        {
            foreach (var a in k_AllFocusModes)
            {
                foreach (var b in k_AllFocusModes)
                {
                    if (a != b)
                    {
                        yield return (a, b);
                    }
                }
            }
        }

        [Test]
        public void ReticleIsVisible_FocusModeChanges_AnimationIsStopped(
            [ValueSource(nameof(CombineFocusModes))]
                (FocusMode focusModeA, FocusMode focusModeB) input)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, input.focusModeA, true);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .5f, input.focusModeB, true);
            m_FocusReticle.Received().StopAnimationIfNeeded();
        }

        [Test]
        public void ReticleIsVisible_FocusModeChangesToReticleAF_ReticleIsActivatedAndNotAnimatedAndAnimationIsReset()
        {
            m_Implementation.UpdateView(Vector2.one * .5f, FocusMode.Clear, true);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .5f, FocusMode.ReticleAF, true);
            m_FocusReticle.Received().SetActive(true);
            m_FocusReticle.Received().ResetAnimation();
            m_FocusReticle.DidNotReceive().Animate(Arg.Any<bool>());
        }

        [TestCase(FocusMode.Clear, FocusMode.Manual)]
        [TestCase(FocusMode.Manual, FocusMode.TrackingAF)]
        [TestCase(FocusMode.TrackingAF, FocusMode.Clear)]
        public void ReticleIsVisible_FocusModeChangesToClearOrManualOrTrackingAF_ReticleIsDeactivatedAndNotAnimatedAndAnimationIsReset(FocusMode from, FocusMode to)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, from, true);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .5f, to, true);
            m_FocusReticle.Received().SetActive(false);
            m_FocusReticle.DidNotReceive().ResetAnimation();
            m_FocusReticle.DidNotReceive().Animate(Arg.Any<bool>());
        }

        [TestCase(FocusMode.Clear)]
        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.ReticleAF)]
        [TestCase(FocusMode.TrackingAF)]
        public void ReticleBecomesVisible_ReticleIsNotAnimatedInAnyMode(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, false);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, true);
            m_FocusReticle.DidNotReceive().Animate(Arg.Any<bool>());
        }

        [Test]
        public void ReticleBecomesVisible_ReticleIsOnlyActivatedInReticleAFMode(
            [ValueSource(nameof(CombineFocusModes))]
                (FocusMode focusModeA, FocusMode focusModeB) input)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, input.focusModeA, false);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .5f, input.focusModeB, true);
            m_FocusReticle.Received().SetActive(input.focusModeB == FocusMode.ReticleAF);
        }

        [TestCase(FocusMode.Clear)]
        [TestCase(FocusMode.Manual)]
        [TestCase(FocusMode.ReticleAF)]
        [TestCase(FocusMode.TrackingAF)]
        public void ReticleBecomesInvisible_ReticleIsDeactivatedInAllModes(FocusMode focusMode)
        {
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, true);
            m_FocusReticle.ClearReceivedCalls();
            m_Implementation.UpdateView(Vector2.one * .5f, focusMode, false);
            m_FocusReticle.Received().SetActive(false);
        }

        void UpdateLastSentPosition(Vector2 position)
        {
            m_Implementation.LastPointerPosition = m_CoordinatesTransformStub.NormalizedToScreen(position);
            m_Implementation.PendingTap = true;
            m_Implementation.ShouldSendPosition(out _);
        }
    }
}
