
namespace TeaSpoons.ServiceLocator
{
    using System;

    public class EarlyServiceRequestException : Exception
    {
        public override string Message => $"{nameof(Services)}.{nameof(Services.Get)} was called before service binding finished. " +
            $"Please subscribe to {nameof(ServiceBinder)}.{nameof(ServiceBinder.BindingFinished)} to perform initialization once all service bindings are complete.";
    }
}
