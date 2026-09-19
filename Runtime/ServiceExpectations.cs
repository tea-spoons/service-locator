
namespace TeaSpoons.ServiceLocator
{
    /// <summary>
    /// Class for deciding between multiple available implementations of the same interface.
    /// </summary>
    public static class ServiceExpectations
    {
        /// <summary>
        /// Adds an "expectation" for a binding.
        /// Once added, only instances of type <typeparamref name="TExpected"/> will be accepted for <typeparamref name="T"/>.
        /// All other attempts at binding an instance to <typeparamref name="T"/> will silently fail.
        /// </summary>
        /// <typeparam name="T">The interface type to expect a specific binding for.</typeparam>
        /// <typeparam name="TExpected">The expected type, which will become the only accepted type for binding to <typeparamref name="T"/>.</typeparam>
        public static void Add<T, TExpected>()
            where T : IService
            where TExpected : class, T
        {
            Services.AddExpectation<T, TExpected>();
        }
    }
}
