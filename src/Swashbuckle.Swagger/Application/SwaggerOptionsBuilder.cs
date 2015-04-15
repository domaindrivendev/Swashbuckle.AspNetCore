using System;
using Microsoft.AspNet.Http;
using Swashbuckle.Swagger.XmlComments;
using Swashbuckle.Swagger.Annotations;

namespace Swashbuckle.Application
{
    public class SwaggerOptionsBuilder
    {
        internal Func<HttpRequest, string> RootUrlResolver { get; private set; }
        internal SchemaGeneratorOptionsBuilder SchemaGeneratorOptionsBuilder { get; private set; }
        internal SwaggerGeneratorOptionsBuilder SwaggerGeneratorOptionsBuilder { get; private set; }

        public SwaggerOptionsBuilder()
        {
            RootUrlResolver = DefaultRootUrlResolver;

            SchemaGeneratorOptionsBuilder = new SchemaGeneratorOptionsBuilder();

            SwaggerGeneratorOptionsBuilder = new SwaggerGeneratorOptionsBuilder();
            SwaggerGeneratorOptionsBuilder.OperationFilter<AddResponsesFromAttributes>();
        }

        public void RootUrl(Func<HttpRequest, string> rootUrlResolver)
        {
            RootUrlResolver = rootUrlResolver;
        }

        public void SchemaGenerator(Action<SchemaGeneratorOptionsBuilder> configure)
        {
            configure(SchemaGeneratorOptionsBuilder);
        }

        public void SwaggerGenerator(Action<SwaggerGeneratorOptionsBuilder> configure)
        {
            configure(SwaggerGeneratorOptionsBuilder);
        }

        public void IncludeXmlComments(string filePath)
        {
            SchemaGeneratorOptionsBuilder.ModelFilter(() => new ApplyXmlTypeComments(filePath));
            SwaggerGeneratorOptionsBuilder.OperationFilter(() => new ApplyXmlActionComments(filePath));
        }

        private static string DefaultRootUrlResolver(HttpRequest request)
        {
            var scheme = request.Headers["X-Forwarded-Proto"] ?? request.Scheme;

            //var host = request.Headers["X-Forwarded-Host"] ?? hostAndPort[0];
            //var port = request.Headers["X-Forwarded-Port"] ?? hostAndPort[1];

            return string.Format("{0}://{1}{2}", scheme, request.Host, request.PathBase);
        }
    }
}