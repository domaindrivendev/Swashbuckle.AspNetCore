using System;
using Microsoft.Framework.OptionsModel;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Description;
using Microsoft.AspNet.Http;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;
using Swashbuckle.Application;

namespace Microsoft.Framework.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwagger(
            this IServiceCollection serviceCollection,
            Action<SwaggerOptions> configure = null)
        {
            serviceCollection.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            serviceCollection.Configure<SwaggerOptions>(configure ?? ((options) => {}));

            serviceCollection.AddTransient(GetRootUrlResolver);
            serviceCollection.AddTransient(GetSchemaRegistry);
            serviceCollection.AddTransient(GetSwaggerProvider);
        }

        private static Func<HttpRequest, string> GetRootUrlResolver(IServiceProvider serviceProvider)
        {
            var swaggerOptions = serviceProvider.GetService<IOptions<SwaggerOptions>>();
            return swaggerOptions.Options.RootUrlResolver;
        }

        private static ISchemaRegistry GetSchemaRegistry(IServiceProvider serviceProvider)
        {
            var jsonContractResolver = GetJsonContractResolver(serviceProvider);
            var swaggerOptions = serviceProvider.GetService<IOptions<SwaggerOptions>>();
            return new SchemaGenerator(jsonContractResolver, swaggerOptions.Options.SchemaGeneratorOptions);
        }

        private static ISwaggerProvider GetSwaggerProvider(IServiceProvider serviceProvider)
        {
            var swaggerOptions = serviceProvider.GetService<IOptions<SwaggerOptions>>();
            return new SwaggerGenerator(
                serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>(),
                () => serviceProvider.GetService<ISchemaRegistry>(),
                swaggerOptions.Options.SwaggerGeneratorOptions);
        }

        private static IContractResolver GetJsonContractResolver(IServiceProvider serviceProvider)
        {
            var mvcOptions = serviceProvider.GetService<MvcOptions>();
            return new DefaultContractResolver();
        }
    }
}