
namespace TeaSpoons.ServiceLocator
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Class for adding and removing bindings to the Service Locator.
    /// </summary>
    public static class ServiceBinder
    {
        /// <summary>
        /// Use this phase in a [<see cref="RuntimeInitializeOnLoadMethodAttribute"/>]
        /// to define the static method in which you bind services.
        /// </summary>
        public const RuntimeInitializeLoadType Phase = RuntimeInitializeLoadType.AfterAssembliesLoaded;

        #region Binding Target Interface and Implementation
        /// <summary>
        /// A binding target representing a type to bind an implementation to.
        /// </summary>
        public interface IBindingTarget<T>
            where T : IService
        {
            /// <summary>
            /// Binds <paramref name="instance"/> to <typeparamref name="T"/>.
            /// </summary>
            /// <exception cref="DuplicateBindingException">Thrown when there was already a binding for <typeparamref name="T"/>.</exception>
            void Bind<TInstance>(Func<TInstance> createFunction)
                where TInstance : T;
        }

        private struct BindingTarget<T> : IBindingTarget<T>
            where T : IService
        {
            void IBindingTarget<T>.Bind<TInstance>(Func<TInstance> createFunction)
            {
                Services.ScheduleBinding<T, TInstance>(createFunction);
            }
        }
        #endregion

        /// <summary>
        /// During service initialization (including the callback passed to <c>Bind</c>),
        /// use this event to run code after all services have been bound.
        /// </summary>
        /// <remarks>
        /// This can be used to have services interact with one another during game startup.\n
        /// Adding a response after service binding has finished causes the response to immediately be invoked once.
        /// </remarks>
        public static event Action BindingFinished
        {
            add
            {
#if UNITY_EDITOR
                if (Services.testMode)
                {
                    value();
                    return;
                }
#endif

                if (Services.hasBindingFinished)
                {
                    value();
                }
                else
                {
                    Services.bindingFinished += value;
                }
            }
            remove
            {
                Services.bindingFinished -= value;
            }
        }

        /// <summary>
        /// During service initialization (including the callback passed to <c>Bind</c>),
        /// use this event to run code after all services have been bound.
        /// </summary>
        /// <remarks>
        /// This can be used to have services interact with one another during game startup.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if accessed after service binding has completed.</exception>
        [Obsolete("OnAfterBindingFinished is deprecated. Please use BindingFinished instead.")]
        public static event Action OnAfterBindingFinished
        {
            add => BindingFinished += value;
            remove => BindingFinished -= value;
        }

        /// <summary>
        /// During service initialization (including the callback passed to <c>Bind</c>),
        /// use this event to register initialization that should happen during <see cref="RuntimeInitializeLoadType.AfterSceneLoad"/>.
        /// </summary>
        /// <remarks>
        /// This can be used to run initial code during this late phase on a per-service basis,
        /// while avoiding static code.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the event already started or finished invocation.</exception>
        public static event Action InitialSceneLoaded
        {
            add
            {
                if (Services.hasInitialSceneLoadedEventStarted)
                {
                    throw new InvalidOperationException($"Cannot add a response to {nameof(InitialSceneLoaded)}, late service initialization has already started.");
                }
                Services.initialSceneLoaded += value;
            }
            remove
            {
                if (Services.hasInitialSceneLoadedEventStarted)
                {
                    throw new InvalidOperationException($"Cannot remove a response from {nameof(InitialSceneLoaded)}, late service initialization has already started.");
                }
                Services.initialSceneLoaded -= value;
            }
        }

        /// <summary>
        /// During service initialization (including the callback passed to <c>Bind</c>),
        /// use this event to register initialization that should happen during <see cref="RuntimeInitializeLoadType.AfterSceneLoad"/>.
        /// </summary>
        /// <remarks>
        /// This can be used to run initial code during this late phase on a per-service basis,
        /// while avoiding static code.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the event already started or finished invocation.</exception>
        [Obsolete("OnLateInitialize is deprecated. Please use InitialSceneLoaded instead.")]
        public static event Action OnLateInitialize
        {
            add => InitialSceneLoaded += value;
            remove => InitialSceneLoaded -= value;
        }

        /// <summary>
        /// Returns an <see cref="IBindingTarget{T}"/> for binding something to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type (usually an interface) to bind to.</typeparam>
        public static IBindingTarget<T> For<T>()
            where T : IService
        {
            return new BindingTarget<T>();
        }
    }
}
