using UnityEditor;
using UnityEngine;

namespace Unity.LiveCapture.VirtualCamera.Editor
{
    /// <summary>
    /// Applies any of the user's manual modifications to an actor's transform
    /// onto its associated device's rig. (While the device is not recording.)
    /// </summary>
    [InitializeOnLoad]
    class ActorManualMoveDetector
    {
        static ActorManualMoveDetector()
        {
            Undo.postprocessModifications += OnPostProcessModification;
        }

        static UndoPropertyModification[] OnPostProcessModification(UndoPropertyModification[] modifications)
        {
            foreach (var modification in modifications)
            {
                var transform = modification.currentValue.target as Transform;
                if (transform == null)
                {
                    continue;
                }

                foreach (var device in VirtualCameraDevice.instances)
                {
                    if (device.Actor != null && transform == device.Actor.transform)
                    {
                        device.RequestAlignWithActor();
                        break;
                    }
                }
            }

            return modifications;
        }
    }
}
