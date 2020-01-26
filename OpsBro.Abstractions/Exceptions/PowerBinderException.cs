using System.Resources;

namespace OpsBro.Abstractions.Exceptions
{
    public abstract class PowerBinderException : EbrainsException
    {
        public override ResourceManager ExceptionMessagesManager => ExceptionMessages.ResourceManager;
        public override ResourceManager ExceptionCodesManager => ExceptionCodes.ResourceManager;
    }
}