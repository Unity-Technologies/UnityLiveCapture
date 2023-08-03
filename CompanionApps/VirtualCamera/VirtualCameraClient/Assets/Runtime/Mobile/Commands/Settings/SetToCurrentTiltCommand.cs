using Zenject;
using UnityEngine;

namespace Unity.CompanionApps.VirtualCamera
{
    class SetToCurrentTiltCommand
    {
        [Inject]
        SignalBus m_SignalBus;

        [Inject(Id = "AR camera")]
        Transform m_Camera;

        public void Execute(SetToCurrentTiltSignal setToCurrentTiltSignal)
        {
            // TODO move this to the mediator and remove the command if the mediator is reused?
            if (m_Camera.transform.localEulerAngles.x > 0 && m_Camera.transform.localEulerAngles.x < 90)
            {
                m_SignalBus.Fire(new SendHostSignal()
                {
                    Type = HostMessageType.ErgonomicTilt,
                    FloatValue = m_Camera.localEulerAngles.x
                });
            }
        }
    }
}
