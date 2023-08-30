using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class GamepadLayoutViewMediator : IInitializable
    {
        [Inject]
        IGamepadLayoutView m_View;

        public void Initialize()
        {
            m_View.onCloseClicked += OnCloseClicked;
        }

        void OnCloseClicked()
        {
            m_View.Hide();
        }
    }
}
