using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Zenject;

namespace Unity.CompanionApps.FaceCapture
{
    class FaceWireframeController : IInitializable, IDisposable, ITickable
    {
        [Inject]
        IARFaceManagerSystem m_FaceManager;
        [Inject]
        ISettings m_Settings;

        Dictionary<ARFace, MeshRenderer> m_Faces = new Dictionary<ARFace, MeshRenderer>();

        public void Initialize()
        {
            m_FaceManager.FaceAdded += FaceAdded;
            m_FaceManager.FaceRemoved += FaceRemoved;
        }

        public void Dispose()
        {
            m_FaceManager.FaceAdded -= FaceAdded;
            m_FaceManager.FaceRemoved -= FaceRemoved;
        }

        public void Tick()
        {
            UpdateVisibility();
        }

        void FaceAdded(ARFace face)
        {
            m_Faces.Add(face, face.GetComponent<MeshRenderer>());
        }

        void FaceRemoved(ARFace face)
        {
            m_Faces.Remove(face);
        }

        void UpdateVisibility()
        {
            foreach (var pair in m_Faces)
            {
                var face = pair.Key;
                var renderer = pair.Value;

                renderer.enabled = m_Settings.FaceWireframe && face.trackingState == TrackingState.Tracking;
            }
        }
    }
}
