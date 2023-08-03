using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    public class JoysticksController : IInitializable, ITickable
    {
        [Inject]
        IJoysticksView m_JoysticksView;

        [Inject]
        Gamepad.IGamepadDriver m_Gamepad;

        public void Initialize()
        {

        }

        public void Tick()
        {
            m_JoysticksView.SetGamepadIsValid(m_Gamepad.HasDevice);
        }
    }
}
