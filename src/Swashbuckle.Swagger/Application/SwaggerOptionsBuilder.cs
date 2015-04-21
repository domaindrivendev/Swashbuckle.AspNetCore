using System;
using Microsoft.AspNet.Http;
using Swashbuckle.Swagger.XmlComments;
using Swashbuckle.Swagger.Annotations;
using System.Globalization;

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
            SwaggerGeneratorOptionsBuilder.OperationFilter<ApplySwaggerOperationAttributes>();
            SwaggerGeneratorOptionsBuilder.OperationFilter<ApplySwaggerResponseAttributes>();
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

        public static string DefaultRootUrlResolver(HttpRequest request)
        {
            var requestUri = new Uri(string.Format("{0}://{1}{2}", request.Scheme, request.Host, request.PathBase));

            var scheme = request.Headers["X-Forwarded-Proto"] ?? requestUri.Scheme;
            var host = request.Headers["X-Forwarded-Host"] ?? requestUri.Host;
            var port = request.Headers["X-Forwarded-Port"] ?? requestUri.Port.ToString(CultureInfo.InvariantCulture);

            return string.Format("{0}://{1}:{2}{3}", scheme, host, port, request.PathBase);
        }
    }
}