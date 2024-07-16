using System;
using System.Runtime.Serialization;

namespace Messenger.Extensions.Exceptions
{
    [Serializable]
    public class SecretsNotFoundException : Exception
    {
        public SecretsNotFoundException() : base($"Não foram encontradas configurações de ambiente!")
        {
        }
        public SecretsNotFoundException(Exception ex) : base($"Não foram encontradas configurações de ambiente!", ex)
        {
        }
    }
}