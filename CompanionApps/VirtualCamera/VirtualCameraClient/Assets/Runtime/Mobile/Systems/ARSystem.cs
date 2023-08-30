using UnityEngine;
using Zenject;
using Unity.LiveCapture.VirtualCamera;
using Unity.CompanionAppCommon;

namespace Unity.CompanionApps.VirtualCamera
{
    public class ARSystem : ITickable
    {
        [Inject]
        ConnectionModel m_Connection;
        [Inject(Id = "AR camera")]
        Transform m_Transform;
        [Inject]
        IRemoteSystem m_Remote;
        [Inject]
        ITimeSystem m_TimeSystem;

        public Vector3 JoysticksValue { get; set; }
        public Vector3 GamepadMoveValue { get; set; }
        public Vector3 GamepadLookValue { get; set; }

        public void Tick()
        {
            if (m_Connection.State == ConnectionState.Connected)
            {
                SendInput();
                SendLegacy();
            }
        }

        void SendInput()
        {
            var host = m_Remote.Host as VirtualCameraHost;

            host.SendInput(new InputSample()
            {
                Time = m_TimeSystem.Time,
                ARPose = new Pose(m_Transform.position, m_Transform.rotation),
                VirtualJoysticks = JoysticksValue,
                GamepadMove = GamepadMoveValue,
                GamepadLook = GamepadLookValue,
            });
        }

        void SendLegacy()
        {
            var host = m_Remote.Host as VirtualCameraHost;

            if (UpateJoysticksData(m_TimeSystem.Time, JoysticksValue, out var joysticksSample))
            {
                host.SendJoysticks(joysticksSample);
            }

            if (UpateGamepadData(m_TimeSystem.Time, GamepadMoveValue, GamepadLookValue, out var gamepadSample))
            {
                host.SendGamepad(gamepadSample);
            }

            host.SendPose(new PoseSample()
            {
                Time = m_TimeSystem.Time,
                Pose = new Pose(m_Transform.position, m_Transform.rotation)
            });
        }

        bool UpateJoysticksData(double time, Vector3 movement, out JoysticksSample sample)
        {
            sample = new JoysticksSample()
            {
                Time = time,
                Joysticks = movement,
            };

            var changed = sample.Joysticks != Vector3.zero;

            return changed;
        }

        bool UpateGamepadData(double time, Vector3 movement, Vector3 rotation, out GamepadSample sample)
        {
            sample = new GamepadSample()
            {
                Time = time,
                Move = movement,
                Look = rotation
            };

            var changed = sample.Move != Vector3.zero || sample.Look != Vector3.zero;

            return changed;
        }
    }
}
