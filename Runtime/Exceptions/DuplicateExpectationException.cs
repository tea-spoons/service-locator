
namespace TeaSpoons.ServiceLocator
{
    using System;

    public class DuplicateExpectationException : Exception
    {
        public override string Message => $"Multiple expectations were set for Type '{type}'.";

        private Type type;

        public DuplicateExpectationException(Type type)
        {
            this.type = type;
        }
    }
}
