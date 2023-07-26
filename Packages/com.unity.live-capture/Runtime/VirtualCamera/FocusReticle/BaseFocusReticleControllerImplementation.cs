using System;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera
{
    /// <summary>
    /// A class implementing the core logic of the focus reticle management.
    /// </summary>
    /// <remarks>
    /// It is event system agnostic since different event systems will be used depending on the context (editor or clients).
    /// </remarks>
    class BaseFocusReticleControllerImplementation : IDisposable
    {
        /// <summary>
        /// Conversion between normalized and screen coordinates.
        /// The reticle position is stored as a normalized value in the app state,
        /// but its view uses screen coordinates.
        /// </summary>
        public interface ICoordinatesTransform
        {
            Vector2 NormalizedToScreen(Vector2 normalizedPosition);
            Vector2 NormalizeScreenPoint(Vector2 screenPosition);
        }

        ICoordinatesTransform m_CoordinatesTransform;
        IFocusReticle m_FocusReticle;
        FocusMode m_CurrentFocusMode = FocusMode.Clear;
        bool m_PendingTap;
        bool m_PendingDrag;
        bool m_IsDragging;
        bool m_IsVisible;
        Vector2 m_LastPointerPosition;
        Vector2 m_CachedReticlePosition;
        Vector2 m_LastSentPosition;

        public ICoordinatesTransform CoordinatesTransform
        {
            set => m_CoordinatesTransform = value;
            private get =>
                m_CoordinatesTransform == null
                ? throw new ArgumentNullException(nameof(CoordinatesTransform))
                : m_CoordinatesTransform;
        }

        public IFocusReticle FocusReticle
        {
            set => m_FocusReticle = value;
            private get =>
                m_FocusReticle == null
                ? throw new ArgumentNullException(nameof(FocusReticle))
                : m_FocusReticle;
        }

        public bool PendingTap
        {
            set => m_PendingTap = value;
        }

        public bool PendingDrag
        {
            set => m_PendingDrag = value;
        }

        public bool IsDragging
        {
            set => m_IsDragging = value;
        }

        public Vector2 LastPointerPosition
        {
            set => m_LastPointerPosition = value;
        }

        public virtual void Initialize()
        {
            FocusReticle.AnimationComplete += OnAnimationComplete;
        }

        public virtual void Dispose()
        {
            var focusReticle = FocusReticle;
            focusReticle.AnimationComplete -= OnAnimationComplete;
            focusReticle.StopAnimationIfNeeded();
        }

        /// <summary>
        /// Updates the reticle view, including its animation and visibility.
        /// </summary>
        /// <param name="position">The reticle position (normalized).</param>
        /// <param name="focusMode">The focus mode.</param>
        /// <param name="visible">The reticle visibility based on the device mode.</param>
        public void UpdateView(Vector2 position, FocusMode focusMode, bool visible)
        {
            m_PendingTap = false;
            m_PendingDrag = false;

            // We do not consider the reticle has moved when it had its default value.
            var reticleMoved = m_CachedReticlePosition != Vector2.zero && m_CachedReticlePosition != position;
            var modeChanged = m_CurrentFocusMode != focusMode;
            var visibilityChanged = visible != m_IsVisible;
            var isReticlePersistent = focusMode == FocusMode.ReticleAF;

            m_CachedReticlePosition = position;
            m_CurrentFocusMode = focusMode;
            m_IsVisible = visible;

            var focusReticle = FocusReticle;

            // At all times reticle position should match received data.
            focusReticle.SetPosition(CoordinatesTransform.NormalizedToScreen(m_CachedReticlePosition));

            var activationChanged = modeChanged || visibilityChanged;

            if (activationChanged)
            {
                focusReticle.StopAnimationIfNeeded();
                if (isReticlePersistent)
                {
                    focusReticle.ResetAnimation();
                }
            }

            var shouldAnimate =
                m_IsVisible && m_CurrentFocusMode != FocusMode.Clear &&
                reticleMoved && !m_IsDragging;

            // Special case: when using reticle auto focus,
            // we only animate in response to local changes,
            // since the reticle may be dragged remotely and we do not want a stuttering animation.
            // The alternative would be to add state to the reticle besides its position, which is fairly invasive.
            // Since in Reticle AF mode the reticle is visible at all times,
            // giving up on animation when it is controlled remotely is an acceptable tradeoff.
            if (m_CurrentFocusMode == FocusMode.ReticleAF)
            {
                shouldAnimate &= m_LastSentPosition == position;
            }

            if (activationChanged || shouldAnimate)
            {
                SetReticleActive(isReticlePersistent && m_IsVisible || shouldAnimate);
            }

            if (shouldAnimate)
            {
                focusReticle.Animate(!isReticlePersistent);
            }
        }

        // In some cases we may want to control more than the reticle activation.
        protected virtual void SetReticleActive(bool value)
        {
            FocusReticle.SetActive(value);
        }

        /// <summary>
        /// Determines whether or not a new reticle position should be sent and evaluates its value.
        /// </summary>
        /// <param name="normalizedReticlePosition">The normalized reticle position.</param>
        /// <returns>True if the reticle position should be sent.</returns>
        public bool ShouldSendPosition(out Vector2 normalizedReticlePosition)
        {
            var sendReticlePosition = m_IsVisible && (
                m_CurrentFocusMode == FocusMode.Manual && m_PendingTap ||
                m_CurrentFocusMode == FocusMode.TrackingAF && m_PendingTap ||
                m_CurrentFocusMode == FocusMode.ReticleAF && (m_PendingDrag || m_PendingTap));

            m_PendingDrag = false;
            m_PendingTap = false;

            if (sendReticlePosition)
            {
                normalizedReticlePosition = CoordinatesTransform.NormalizeScreenPoint(m_LastPointerPosition);

                var normalizedPositionIsValid =
                    normalizedReticlePosition.x >= 0 && normalizedReticlePosition.x <= 1 &&
                    normalizedReticlePosition.y >= 0 && normalizedReticlePosition.y <= 1;

                if (normalizedPositionIsValid)
                {
                    // Subtly nudge the position so that a state change is detected
                    // triggering a raycast and animation,
                    // even when the reticle position is actually the same.
                    // This is a marginal case mostly encountered in the editor with a static cursor.
                    if (normalizedReticlePosition == m_CachedReticlePosition)
                    {
                        var nudge = (normalizedReticlePosition.x < 0.5f ? 1 : -1) * .00001f;
                        normalizedReticlePosition.x += nudge;
                    }

                    m_LastSentPosition = normalizedReticlePosition;

                    return true;
                }
            }

            normalizedReticlePosition = Vector2.zero;
            return false;
        }

        void OnAnimationComplete()
        {
            if (m_CurrentFocusMode != FocusMode.ReticleAF)
            {
                SetReticleActive(false);
            }
        }
    }
}
