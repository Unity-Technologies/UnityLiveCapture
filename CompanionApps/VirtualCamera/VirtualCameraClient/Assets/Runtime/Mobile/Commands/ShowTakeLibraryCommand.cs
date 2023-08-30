using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    class ShowTakeLibraryCommand
    {
        [Inject]
        ITakeLibraryView m_View;

        public void Execute(ShowTakeLibrarySignal signal)
        {
            if (signal.value)
            {
                m_View.Show();
            }
            else
            {
                m_View.Hide();
            }
        }
    }
}
