using System;

namespace Swashbuckle.Swagger
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwagger(
            string apiVersion,
            string defaultHost = null,
            string defaultBasePath = null,
            string[] defaultSchemes = null);
    }

    public class UnknownApiVersion : Exception
    {
        public UnknownApiVersion(string apiVersion)
            : base(string.Format("Unknown API version - {0}", apiVersion))
        {}
    }
}