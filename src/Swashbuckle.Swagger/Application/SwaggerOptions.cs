using System;
using Microsoft.AspNet.Http;
using Swashbuckle.Swagger.XmlComments;
using Swashbuckle.Swagger.Annotations;
using System.Globalization;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerOptions
    {
        public Func<HttpRequest, string> RootUrlResolver { get; private set; }

        internal SchemaGeneratorOptions SchemaGeneratorOptions { get; private set; }

        internal SwaggerGeneratorOptions SwaggerGeneratorOptions { get; private set; }

        public SwaggerOptions()
        {
            RootUrlResolver = DefaultRootUrlResolver;

            SchemaGeneratorOptions = new SchemaGeneratorOptions();

            SwaggerGeneratorOptions = new SwaggerGeneratorOptions();
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerOperationAttributes());
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerResponseAttributes());
        }

        public void SetRootUrlResolver(Func<HttpRequest, string> rootUrlResolver)
        {
            RootUrlResolver = rootUrlResolver;
        }

        public void SwaggerGenerator(Action<SwaggerGeneratorOptions> configure)
        {
            configure(SwaggerGeneratorOptions);
        }

        public void SchemaGenerator(Action<SchemaGeneratorOptions> configure)
        {
            configure(SchemaGeneratorOptions);
        }

        public void IncludeXmlComments(string filePath)
        {
            SchemaGeneratorOptions.ModelFilters.Add(new ApplyXmlTypeComments(filePath));
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplyXmlActionComments(filePath));
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