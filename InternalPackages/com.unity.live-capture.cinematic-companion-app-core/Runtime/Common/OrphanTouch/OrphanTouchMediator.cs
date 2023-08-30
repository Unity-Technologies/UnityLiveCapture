using System;
using UnityEngine;
using Zenject;

namespace Unity.CompanionAppCommon
{
    enum OrphanTouchType
    {
        PointerDown,
        BeginDrag,
        Drag,
        EndDrag
    }

    class OrphanTouchMediator : IInitializable, IDisposable
    {
        [Inject]
        IOrphanTouch m_View;

        SignalBus m_SignalBus;

        public OrphanTouchMediator(SignalBus signalBus)
        {
            m_SignalBus = signalBus;
        }

        public void Initialize()
        {
            m_View.onBeginDrag += OnBeginDrag;
            m_View.onDrag += OnDrag;
            m_View.onEndDrag += OnEndDrag;
            m_View.onPointerDown += OnPointerDown;
        }

        public void Dispose()
        {
            m_View.onBeginDrag -= OnBeginDrag;
            m_View.onDrag -= OnDrag;
            m_View.onEndDrag -= OnEndDrag;
            m_View.onPointerDown -= OnPointerDown;
        }

        void OnBeginDrag(Vector2 position)
        {
            m_SignalBus.Fire(new OrphanTouchSignal()
            {
                position = position,
                type = OrphanTouchType.BeginDrag
            });
        }

        void OnDrag(Vector2 position)
        {
            m_SignalBus.Fire(new OrphanTouchSignal()
            {
                position = position,
                type = OrphanTouchType.Drag
            });
        }

        void OnEndDrag(Vector2 position)
        {
            m_SignalBus.Fire(new OrphanTouchSignal()
            {
                position = position,
                type = OrphanTouchType.EndDrag
            });
        }

        void OnPointerDown(Vector2 position)
        {
            m_SignalBus.Fire(new OrphanTouchSignal()
            {
                position = position,
                type = OrphanTouchType.PointerDown
            });
        }
    }
}
