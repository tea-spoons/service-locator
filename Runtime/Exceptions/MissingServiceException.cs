
namespace TeaSpoons.ServiceLocator
{
    using System;

    public class MissingServiceException : Exception
    {
        public override string Message => $"A service of type {type} was requested, but no instance is bound to it.";

        private readonly Type type;

        public MissingServiceException(Type type)
        {
            this.type = type;
        }
    }
}
