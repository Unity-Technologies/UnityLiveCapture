using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.LiveCapture.Editor
{
    class PreviewResizer : PointerManipulator
    {
        Vector3 m_Start;
        bool m_Active;

        public PreviewResizer()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            m_Active = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.parent.RegisterCallback<GeometryChangedEvent>(OnSizeChange);
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.parent.UnregisterCallback<GeometryChangedEvent>(OnSizeChange);
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        void OnSizeChange(GeometryChangedEvent evt)
        {
            if (m_Active)
            {
                return;
            }

            ApplyDelta(0f);
        }

        public void ApplyDelta(float delta)
        {
            var headerHeight = target.contentRect.height;
            var maxHeight = target.parent.parent.contentRect.height - 100f;
            var containerHeight = target.parent.contentRect.height;
            var height = Mathf.Min(containerHeight - delta, maxHeight);

            if (height < 100f)
            {
                height = height < 50f ? headerHeight : 100f;
            }

            target.parent.style.height = height;
        }

        protected void OnPointerDown(PointerDownEvent e)
        {
            if (m_Active)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                m_Start = e.localPosition;

                m_Active = true;
                target.CapturePointer(e.pointerId);
                e.StopPropagation();
            }
        }

        protected void OnPointerMove(PointerMoveEvent e)
        {
            if (!m_Active || !target.HasPointerCapture(e.pointerId))
                return;

            var delta = (e.localPosition - m_Start).y;

            ApplyDelta(delta);

            e.StopPropagation();
        }

        protected void OnPointerUp(PointerUpEvent e)
        {
            if (!m_Active || !target.HasPointerCapture(e.pointerId) || !CanStopManipulation(e))
                return;

            m_Active = false;
            target.ReleasePointer(e.pointerId);
            e.StopPropagation();
        }
    }
}
