
namespace TeaSpoons.ServiceLocator
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Class for acquiring references to bound services.
    /// </summary>
    public static class Services
    {
        private readonly struct ScheduledBinding
        {
            private readonly Type serviceType;
            private readonly Type instanceType;
            private readonly Func<IService> createFunction;

            public ScheduledBinding(Type type, Type instanceType, Func<IService> createFunction)
            {
                this.serviceType = type;
                this.instanceType = instanceType;
                this.createFunction = createFunction;
            }

            public void Bind()
            {
                if (HasAnotherExpected(serviceType, instanceType))
                {
                    return;
                }

                var success = bindings.TryAdd(serviceType, createFunction());

                if (!success)
                {
                    throw new DuplicateBindingException(serviceType, bindings[serviceType].GetType(), instanceType);
                }
            }
        }

        private static List<ScheduledBinding> scheduledBindings = new();
        private static readonly Dictionary<Type, IService> bindings = new();
        private static readonly Dictionary<Type, Type> expectations = new();

        internal static event Action bindingFinished;
        internal static event Action initialSceneLoaded;
        internal static bool hasBindingStarted => scheduledBindings == null;
        internal static bool hasBindingFinished { get; private set; } = false;
        internal static bool hasInitialSceneLoadedEventStarted { get; private set; } = false;

#if UNITY_EDITOR
        /// <summary>
        /// When enabled, test mode allows to freely override bindings, for the purpose of replacing existing bindings with mocks/substitutes in unit tests.
        /// </summary>
        /// <remarks>
        /// Only available in the editor.
        /// </remarks>
        internal static bool testMode = false;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            bindings.Clear();
            expectations.Clear();
            bindingFinished = null;
            initialSceneLoaded = null;
            hasBindingFinished = false;
            hasInitialSceneLoadedEventStarted = false;
            scheduledBindings = new();
#if UNITY_EDITOR
            testMode = false;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitializeBindings()
        {
            var scheduledBindings = Services.scheduledBindings;
            Services.scheduledBindings = null;

            foreach (var binding in scheduledBindings)
            {
                binding.Bind();
            }

            hasBindingFinished = true;

            bindingFinished?.Invoke();
            bindingFinished = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void DoLateInitialization()
        {
            hasInitialSceneLoadedEventStarted = true;
            if (initialSceneLoaded != null)
            {
                foreach (var response in initialSceneLoaded.GetInvocationList())
                {
                    try
                    {
                        response.Method.Invoke(response.Target, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e.InnerException);
                    }
                }
            }
            initialSceneLoaded = null;
        }

        /// <summary>
        /// Returns reference to the instance bound to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of which to get the bound service instance.</typeparam>
        /// <exception cref="MissingServiceException">Thrown if <typeparamref name="T"/> does not have a service bound to it.</exception>
        public static T Get<T>()
            where T : IService
        {
#if UNITY_EDITOR
            if (!testMode && !hasBindingFinished)
#else
            if (!hasBindingFinished)
#endif
            {
                throw new EarlyServiceRequestException();
            }

            if (bindings.TryGetValue(typeof(T), out var result))
            {
                return (T)result;
            }

            throw new MissingServiceException(typeof(T));
        }

        internal static void ScheduleBinding<T, TInstance>(Func<TInstance> createFunction)
            where TInstance : T
            where T : IService
        {
#if UNITY_EDITOR
            if (testMode)
            {
                new ScheduledBinding(typeof(T), typeof(TInstance), () => createFunction()).Bind();
                return;
            }
#endif
            if (hasBindingStarted) throw new LateBindingException();

            scheduledBindings.Add(new ScheduledBinding(typeof(T), typeof(TInstance), () => createFunction()));
        }

        internal static void AddExpectation<T, TExpected>()
            where T : IService
            where TExpected : class, T
        {
            if (!expectations.TryAdd(typeof(T), typeof(TExpected)))
            {
                throw new DuplicateExpectationException(typeof(T));
            }
        }

        internal static void Clear()
        {
            bindings.Clear();
            expectations.Clear();
            bindingFinished = null;
        }

        /// <summary>
        /// Returns <c>true</c> exactly when an expectation exists for <paramref name="target"/>, and it is NOT <paramref name="proposed"/>.
        /// </summary>
        private static bool HasAnotherExpected(Type target, Type proposed)
        {
            return expectations.TryGetValue(target, out var expectation) && !expectation.IsAssignableFrom(proposed);
        }
    }
}
