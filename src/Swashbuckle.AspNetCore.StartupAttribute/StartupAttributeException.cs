using System;
using System.Runtime.Serialization;

namespace Swashbuckle.AspNetCore.StartupAttribute
{
    [Serializable]
    public class StartupAttributeException : Exception
    {
        public StartupAttributeException() : base() { }

        public StartupAttributeException(string message) : base(message) { }

        public StartupAttributeException(string message, Exception innerException) : base(message, innerException) { }

        protected StartupAttributeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
