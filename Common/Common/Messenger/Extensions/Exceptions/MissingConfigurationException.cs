using System;

namespace Messenger.Extensions.Exceptions
{
    public class MissingConfigurationException : Exception
    {
        public MissingConfigurationException(string message) : base(message)
        {
        }
    }
}