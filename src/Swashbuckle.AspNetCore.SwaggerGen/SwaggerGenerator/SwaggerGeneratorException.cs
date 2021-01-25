using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerGeneratorException : Exception
    {
        public SwaggerGeneratorException(string message) : base(message)
        { }

        public SwaggerGeneratorException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}