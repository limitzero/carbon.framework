using System;
namespace Kharbon.Core.Exceptions
{
    public class MessageBusConfigurationException : ApplicationException
    {
        public MessageBusConfigurationException()
        {
            
        }

        public MessageBusConfigurationException(string message)
            :base(message)
        {
        }

        public MessageBusConfigurationException(string message, Exception inner)
            :base(message, inner)
        {
        }
    }
}