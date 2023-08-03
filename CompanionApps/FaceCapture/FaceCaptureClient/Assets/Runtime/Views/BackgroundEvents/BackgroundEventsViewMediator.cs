using System;
using Zenject;
using Unity.CompanionApps.FaceCapture.BackgroundEventsSignals;

namespace Unity.CompanionApps.FaceCapture
{
    class BackgroundEventsViewMediator : IInitializable, IDisposable
    {
        [Inject]
        IBackgroundEventsView m_BackgroundEventsView;

        [Inject]
        SignalBus m_SignalBus;

        public void Initialize()
        {
            m_BackgroundEventsView.OnClick += OnBackgroundClicked;
            m_BackgroundEventsView.OnTouch += OnBackgroundTouched;
        }

        public void Dispose()
        {
            m_BackgroundEventsView.OnClick -= OnBackgroundClicked;
            m_BackgroundEventsView.OnTouch -= OnBackgroundTouched;
        }

        void OnBackgroundClicked(int button, int clickCount)
        {
            m_SignalBus.Fire(new Clicked()
            {
                Button = button,
                ClickCount = clickCount
            });
        }

        void OnBackgroundTouched(int touchCount)
        {
             m_SignalBus.Fire(new Touched()
            {
                TouchCount = touchCount
            });
        }
    }
}
