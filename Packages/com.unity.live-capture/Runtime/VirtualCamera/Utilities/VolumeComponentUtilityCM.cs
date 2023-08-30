#if SRP_CORE_14_0_OR_NEWER && CINEMACHINE_2_4_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
#if CINEMACHINE_3_0_0_OR_NEWER
using Unity.Cinemachine ;
#else
using Cinemachine;
using Cinemachine.PostFX;
using CinemachineCamera = Cinemachine.CinemachineVirtualCamera;
#endif

namespace Unity.LiveCapture.VirtualCamera
{
    static class VolumeComponentUtilityCM
    {
        internal static CinemachineVolumeSettings GetOrAddVolumeSettings(CinemachineCamera virtualCamera)
        {
            if (!virtualCamera.TryGetComponent<CinemachineVolumeSettings>(out var volumeSettings))
            {
                volumeSettings = virtualCamera.gameObject.AddComponent<CinemachineVolumeSettings>();
#if !CINEMACHINE_3_0_0_OR_NEWER
                virtualCamera.AddExtension(volumeSettings);
#endif
            }

            return volumeSettings;
        }
    }
}
#endif
