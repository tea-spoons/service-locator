
namespace TeaSpoons.ServiceLocator
{
#if UNITY_EDITOR
    /// <summary>
    /// Methods needed for using the service locator in unit tests.
    /// </summary>
    /// <remarks>
    /// Only available in the editor.
    /// </remarks>
    public static class ServicesTestMode
    {
        /// <summary>
        /// Enables "test mode", which allows binding services even after the regular service locator binding phase (see <see cref="ServiceBinder.BindingTime"/>).<br/>
        /// Deletes all existing bindings and expectations.<br/>
        /// In test mode, all actions added to <see cref="ServiceBinder.BindingFinished"/> will not be remembered, but instead be invoked immediately.
        /// </summary>
        public static void Start()
        {
            Services.testMode = true;
            Services.Clear();
        }
    }
#endif
}
