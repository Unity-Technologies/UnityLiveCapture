using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using Unity.Collections;

namespace Unity.CompanionApps.FaceCapture
{
    [RequireComponent(typeof(ARSession))]
    [RequireComponent(typeof(ARFaceManager))]
    class ARFaceManagerSystem : MonoBehaviour, IARFaceManagerSystem
    {
        public event Action<ARFace> FaceRemoved = delegate {};
        public event Action<ARFace> FaceUpdated = delegate {};
        public event Action<ARFace> FaceAdded = delegate {};

        ARSession m_ARSession;
        ARFaceManager m_FaceManager;
        bool m_IsSupported;

        public bool Enabled
        {
            get => m_FaceManager.enabled;
            set => m_FaceManager.enabled = value;
        }

        public bool IsSupported => m_IsSupported;

        void Awake()
        {
            m_ARSession = GetComponent<ARSession>();
            m_FaceManager = GetComponent<ARFaceManager>();

            m_IsSupported = m_FaceManager.subsystem != null;

            m_FaceManager.facesChanged += OnFacesChanged;
        }

        void OnDestroy()
        {
            m_FaceManager.facesChanged -= OnFacesChanged;
        }

        void OnFacesChanged(ARFacesChangedEventArgs eventArgs)
        {
            foreach (var arFace in eventArgs.removed)
            {
                FaceRemoved(arFace);
            }

            foreach (var arFace in eventArgs.updated)
            {
                FaceUpdated(arFace);
            }

            foreach (var arFace in eventArgs.added)
            {
                FaceAdded(arFace);
            }
        }

        public bool RequestWorldAlignment(ARWorldAlignment alignment)
        {
            var subsystem = m_ARSession.subsystem as ARKitSessionSubsystem;
            var isValid = subsystem != null;

            if (isValid)
            {
                subsystem.requestedWorldAlignment = alignment;
            }

            return isValid;
        }

        public NativeArray<ARKitBlendShapeCoefficient> GetBlendShapeCoefficients(ARFace face, Allocator allocator)
        {
            var subsystem = m_FaceManager.subsystem as ARKitFaceSubsystem;

            return subsystem.GetBlendShapeCoefficients(face.trackableId, allocator);
        }
    }
}
