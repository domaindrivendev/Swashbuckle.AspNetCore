using System;
using Microsoft.AspNet.Http;
using Swashbuckle.Swagger.Application;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Application
{
    public class SwaggerOptions
    {
        public SwaggerOptions()
        {
            RouteTemplate = "swagger/docs/{apiVersion}";
            RootUrlResolver = DefaultRootUrlResolver;
            SwaggerGeneratorOptions= new SwaggerGeneratorOptions();
            SchemaGeneratorOptions= new SchemaGeneratorOptions();
        }

        public  string RouteTemplate { get; set; }

        public Func<HttpRequest, string> RootUrlResolver { get; set; }

        internal SwaggerGeneratorOptions SwaggerGeneratorOptions { get; private set; }

        internal SchemaGeneratorOptions SchemaGeneratorOptions { get; private set; }

        public void SwaggerGenerator(Action<SwaggerGeneratorOptionsBuilder> configure)
        {
            var builder = new SwaggerGeneratorOptionsBuilder(); 
            configure(builder);
            SwaggerGeneratorOptions = builder.Build();
        }

        public void SchemaGenerator(Action<SchemaGeneratorOptionsBuilder> configure)
        {
            var builder = new SchemaGeneratorOptionsBuilder();
            configure(builder);
            SchemaGeneratorOptions = builder.Build();
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