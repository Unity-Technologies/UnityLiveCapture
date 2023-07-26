using UnityEngine;
using Zenject;

namespace Unity.CompanionAppCommon
{
    public class ApplicationEventsSystem : MonoBehaviour
    {
        [Inject]
        SignalBus m_SignalBus;

        void OnApplicationFocus(bool hasFocus)
        {
            m_SignalBus.Fire(new ApplicationFocusChangedSignal() { value = !hasFocus });
        }

        void OnApplicationPause(bool pauseStatus)
        {
            m_SignalBus.Fire(new ApplicationPauseChangedSignal() { value = pauseStatus });
        }
    }
}
