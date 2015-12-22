using System;

namespace Swashbuckle.SwaggerGen.Generator
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwagger(
            string apiVersion,
            string host = null,
            string basePath = null,
            string[] schemes = null);
    }

    public class UnknownApiVersion : Exception
    {
        public UnknownApiVersion(string apiVersion)
            : base(string.Format("Unknown API version - {0}", apiVersion))
        {}
    }
}