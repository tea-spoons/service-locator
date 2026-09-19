
namespace TeaSpoons.ServiceLocator
{
    using System;

    public class DuplicateBindingException : Exception
    {
        public override string Message => $"An attempt was made to bind an object of type '{duplicateBinding.Name}' to '{key.Name}'.\nHowever, an object of type '{existingBinding.Name} is already bound to it.'";

        private Type key;
        private Type existingBinding;
        private Type duplicateBinding;

        public DuplicateBindingException(Type key, Type existingBinding, Type duplicateBinding)
        {
            this.key = key;
            this.existingBinding = existingBinding;
            this.duplicateBinding = duplicateBinding;
        }
    }
}
