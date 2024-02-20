using System;
using System.Runtime.Serialization;

namespace gc.infraestructura.Core.Exceptions
{
    public class SecurityException : Exception
    {
        public SecurityException()
        {
        }

        public SecurityException(string message) : base(message)
        {
        }

        public SecurityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
