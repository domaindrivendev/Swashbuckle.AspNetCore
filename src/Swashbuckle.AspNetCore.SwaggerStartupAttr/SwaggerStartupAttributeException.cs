using System;
using System.Runtime.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerStartupAttr
{
    [Serializable]
    public class SwaggerStartupAttributeException : Exception
    {
        public SwaggerStartupAttributeException() : base() { }

        public SwaggerStartupAttributeException(string message) : base(message) { }

        public SwaggerStartupAttributeException(string message, Exception innerException) : base(message, innerException) { }

        protected SwaggerStartupAttributeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
