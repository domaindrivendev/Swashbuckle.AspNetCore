using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGeneratorException : Exception
    {
        public SchemaGeneratorException(string message) : base(message)
        { }

        public SchemaGeneratorException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}