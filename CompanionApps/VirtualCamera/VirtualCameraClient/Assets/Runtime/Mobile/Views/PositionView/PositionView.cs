using System;
using System.Collections.Generic;
using Unity.LiveCapture.VirtualCamera;
using UnityEngine;
using Unity.TouchFramework;
using UnityEngine.UI;

namespace Unity.CompanionApps.VirtualCamera
{
    class PositionView : LinkedScroll, IPositionView
    {
        [SerializeField]
        SlideToggle m_LockX;
        [SerializeField]
        SlideToggle m_LockY;
        [SerializeField]
        SlideToggle m_LockZ;
        [SerializeField]
        Button m_ResetToOne;

        PositionAxis m_CachedPosition;

        public event Action<Vector3> onMotionScaleChanged = delegate {};
        public event Action<PositionAxis> onAxisLockChanged = delegate {};

        static List<float> GetEntries()
        {
            var list = new List<float>();

            list.Add(0.1f);
            list.Add(0.25f);
            list.Add(0.5f);
            list.Add(0.75f);

            for (var i = 1; i < 25; ++i)
                list.Add(i);

            for (var i = 5; i < 10; ++i)
                list.Add(i * 5);

            for (var i = 5; i < 10; ++i)
                list.Add(i * 10);

            for (var i = 1; i <= 10; ++i)
                list.Add(i * 100);

            return list;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_ResetToOne.onClick.AddListener(OnResetClicked);
            m_LockX.onValueChanged.AddListener(LockTruck);
            m_LockY.onValueChanged.AddListener(LockPedestal);
            m_LockZ.onValueChanged.AddListener(LockDolly);
            m_Vector3Scroller.onValueChanged += OnMotionScaleValueChanged;
        }

        protected override void OnDisable()
        {
            m_ResetToOne.onClick.RemoveListener(OnResetClicked);
            m_LockX.onValueChanged.RemoveListener(LockTruck);
            m_LockY.onValueChanged.RemoveListener(LockPedestal);
            m_LockZ.onValueChanged.RemoveListener(LockDolly);
            m_Vector3Scroller.onValueChanged -= OnMotionScaleValueChanged;
            base.OnDisable();
        }

        void Start()
        {
            m_Vector3Scroller.SetEntries(GetEntries());
        }

        void LockPedestal(bool on)
        {
            SetAxisLock(PositionAxis.Pedestal, on);
        }

        void LockTruck(bool on)
        {
            SetAxisLock(PositionAxis.Truck, on);
        }

        void LockDolly(bool on)
        {
            SetAxisLock(PositionAxis.Dolly, on);
        }

        void SetAxisLock(PositionAxis axis, bool isEnabled)
        {
            var positionLock = m_CachedPosition;

            if (isEnabled)
                positionLock |= axis;
            else
                positionLock &= ~axis;

            m_CachedPosition = positionLock;
            onAxisLockChanged.Invoke(m_CachedPosition);
        }

        void OnMotionScaleValueChanged(Vector3 motionScale)
        {
            onMotionScaleChanged.Invoke(motionScale);
        }

        void OnResetClicked()
        {
            m_Vector3Scroller.SetValue(Vector3.one);
        }

        public void SetMotionScale(Vector3 motionScale)
        {
            m_Vector3Scroller.SetValue(motionScale, gameObject.activeInHierarchy);
        }

        public void SetPositionLock(PositionAxis positionAxis)
        {
            m_LockX.on = positionAxis.HasFlag(PositionAxis.Truck);
            m_LockY.on = positionAxis.HasFlag(PositionAxis.Pedestal);
            m_LockZ.on = positionAxis.HasFlag(PositionAxis.Dolly);
            m_CachedPosition = positionAxis;
        }
    }
}
