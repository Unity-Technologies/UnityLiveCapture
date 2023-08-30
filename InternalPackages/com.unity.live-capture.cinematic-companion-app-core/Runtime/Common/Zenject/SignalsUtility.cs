using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using Zenject;

namespace Unity.CompanionAppCommon
{
    // Inspired by SignalExtensions
    public static class FixedSignalExtensions
    {
        /// <summary>
        /// Inspired by <see cref="SignalExtensions.BindSignal{TSignal}(DiContainer)"/>.
        /// Equivalent to BindSignal(), but the handler object and method are resolved only once when
        /// the container initializes (instead of every time the signal is invoked).
        /// </summary>
        /// <remarks>
        /// TObject and TSignal cannot be value types because they would risk being stripped away by AOT (on iOS for example).
        /// https://github.com/modesttree/Zenject#does-this-work-on-aot-platforms-such-as-ios-and-webgl
        /// </remarks>
        public static FixedBindSignalToBinder<TSignal> FixedBindSignal<TSignal>(this DiContainer container)
            where TSignal : class
        {
            var signalBindInfo = new SignalBindingBindInfo(typeof(TSignal));

            return new FixedBindSignalToBinder<TSignal>(container, signalBindInfo);
        }
    }

    // Inspired by BindSignalToBinder
    public class FixedBindSignalToBinder<TSignal>
        where TSignal : class
    {
        DiContainer _container;
        BindStatement _bindStatement;
        SignalBindingBindInfo _signalBindInfo;

        public FixedBindSignalToBinder(DiContainer container, SignalBindingBindInfo signalBindInfo)
        {
            _container = container;

            _signalBindInfo = signalBindInfo;
            // This will ensure that they finish the binding
            _bindStatement = container.StartBinding();
        }

        public FixedBindSignalFromBinder<TObject, TSignal> ToMethod<TObject>(Action<TObject, TSignal> handler)
            where TObject : class
        {
            return ToMethod<TObject>(x => (Action<TSignal>)(s => handler(x, s)));
        }

        public FixedBindSignalFromBinder<TObject, TSignal> ToMethod<TObject>(Func<TObject, Action> handlerGetter)
            where TObject : class
        {
            return ToMethod<TObject>(x => (Action<TSignal>)(s => handlerGetter(x)()));
        }

        public FixedBindSignalFromBinder<TObject, TSignal> ToMethod<TObject>(Func<TObject, Action<TSignal>> handlerGetter)
            where TObject : class
        {
            return new FixedBindSignalFromBinder<TObject, TSignal>(_signalBindInfo, _bindStatement, handlerGetter, _container);
        }
    }

    // Inspired by BindSignalFromBinder
    public class FixedBindSignalFromBinder<TObject, TSignal>
        where TObject : class
        where TSignal : class
    {
        readonly BindStatement _bindStatement;
        readonly Func<TObject, Action<TSignal>> _methodGetter;
        readonly DiContainer _container;
        readonly SignalBindingBindInfo _signalBindInfo;

        public FixedBindSignalFromBinder(
            SignalBindingBindInfo signalBindInfo, BindStatement bindStatement, Func<TObject, Action<TSignal>> methodGetter,
            DiContainer container)
        {
            _signalBindInfo = signalBindInfo;
            _bindStatement = bindStatement;
            _methodGetter = methodGetter;
            _container = container;
        }

        public SignalCopyBinder FromResolve()
        {
            return From(x => x.FromResolve().AsCached());
        }

        public SignalCopyBinder FromResolveAll()
        {
            return From(x => x.FromResolveAll().AsCached());
        }

        public SignalCopyBinder FromNew()
        {
            return From(x => x.FromNew().AsCached());
        }

        public SignalCopyBinder From(Action<ConcreteBinderGeneric<TObject>> objectBindCallback)
        {
            Assert.That(!_bindStatement.HasFinalizer);
            _bindStatement.SetFinalizer(new NullBindingFinalizer());

            var objectLookupId = Guid.NewGuid();

            // Very important here that we use NoFlush otherwise the main binding will be finalized early
            var objectBinder = _container.BindNoFlush<TObject>().WithId(objectLookupId);
            objectBindCallback(objectBinder);

            var wrapperBinder = _container
                .BindInterfacesTo<FixedSignalCallbackWithLookupWrapper<TObject, TSignal>>()
                .AsCached()
                .WithArguments(_signalBindInfo, typeof(TObject), objectLookupId, _methodGetter)
                .NonLazy();

            var copyBinder = new SignalCopyBinder(wrapperBinder.BindInfo);
            // Make sure if they use one of the Copy/Move methods that it applies to both bindings
            copyBinder.AddCopyBindInfo(objectBinder.BindInfo);
            return copyBinder;
        }
    }

    // Inspired by SignalCallbackWithLookupWrapper
    public class FixedSignalCallbackWithLookupWrapper<TObject, TSignal> : IInitializable, IDisposable
        where TObject : class
        where TSignal : class
    {
        readonly DiContainer _container;
        readonly SignalBus _signalBus;
        readonly Guid _lookupId;
        readonly Func<TObject, Action<TSignal>> _methodGetter;
        readonly Type _objectType;
        readonly Type _signalType;
        readonly object _identifier;

        List<Action<TSignal>> _methods = new List<Action<TSignal>>();

        public FixedSignalCallbackWithLookupWrapper(
            SignalBindingBindInfo signalBindInfo,
            Type objectType,
            Guid lookupId,
            Func<TObject, Action<TSignal>> methodGetter,
            SignalBus signalBus,
            DiContainer container)
        {
            _objectType = objectType;
            _signalType = signalBindInfo.SignalType;
            _identifier = signalBindInfo.Identifier;
            _container = container;
            _methodGetter = methodGetter;
            _signalBus = signalBus;
            _lookupId = lookupId;

            signalBus.SubscribeId(signalBindInfo.SignalType, _identifier, OnSignalFired);
        }

        void OnSignalFired(object signal)
        {
            for (int i = 0; i < _methods.Count; i++)
            {
                var method = _methods[i];
                method(signal as TSignal);
            }
        }

        public void Initialize()
        {
            var objects = _container.ResolveIdAll(_objectType, _lookupId);

            for (int i = 0; i < objects.Count; i++)
            {
                var obj = objects[i] as TObject;
                var method = _methodGetter(obj);
                _methods.Add(method);
            }
        }

        public void Dispose()
        {
            _signalBus.UnsubscribeId(_signalType, _identifier, OnSignalFired);
        }
    }
}
