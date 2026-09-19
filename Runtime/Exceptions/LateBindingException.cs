
namespace TeaSpoons.ServiceLocator
{
    using System;

    public class LateBindingException : Exception
    {
        public override string Message => $"An attempt was made to bind a service implementation after {nameof(ServiceBinder.Phase)}.\nPlease use the {nameof(ServiceBinder.Phase)} phase for correct service initialization.";
    }
}
