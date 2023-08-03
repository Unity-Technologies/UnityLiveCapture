using Unity.CompanionAppCommon;
using UnityEngine.InputSystem;
using Zenject;

namespace Unity.CompanionApps.VirtualCamera
{
    // Brings back the Information Bar if the user taps the screen with 3 fingers.
    class DisplayInformationBarCommand
    {
        [Inject]
        SignalBus m_SignalBus;

        public void Execute(OrphanTouchSignal signal)
        {
            if (signal.type == OrphanTouchType.PointerDown && HasPerformedGesture())
            {
                m_SignalBus.Fire(new SetMainViewOptionSignal { value = (MainViewOptions.InformationBar, true) });
            }
        }

        bool HasPerformedGesture()
        {
            var count = 0;
            var touchScreen = Touchscreen.current;
            if (touchScreen != null)
            {
                foreach (var touch in touchScreen.touches)
                {
                    if (touch.ReadValue().phase == TouchPhase.Began)
                    {
                        ++count;
                    }
                }
            }

            // 3 fingers gesture, UX decision.
            return count >= 3;
        }
    }
}
