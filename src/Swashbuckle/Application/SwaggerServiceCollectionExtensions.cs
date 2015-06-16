using System;
using Microsoft.Framework.OptionsModel;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Newtonsoft.Json;
using Swashbuckle.Swagger;
using Swashbuckle.Application;

namespace Microsoft.Framework.DependencyInjection
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static void AddSwagger(
            this IServiceCollection serviceCollection,
            Action<SwaggerOptions> configure = null)
        {
            serviceCollection.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            serviceCollection.Configure(configure ?? ((options) => {}));

            serviceCollection.AddTransient(GetSchemaRegistry);
            serviceCollection.AddTransient(GetSwaggerProvider);
        }

        private static ISchemaRegistry GetSchemaRegistry(IServiceProvider serviceProvider)
        {
            var jsonSerializerSettings = GetJsonSerializerSettings(serviceProvider);
            var swaggerOptions = serviceProvider.GetService<IOptions<SwaggerOptions>>();
            return new SchemaGenerator(jsonSerializerSettings, swaggerOptions.Options.SchemaGeneratorOptions);
        }

        private static ISwaggerProvider GetSwaggerProvider(IServiceProvider serviceProvider)
        {
            var optionAccessor = serviceProvider.GetService<IOptions<SwaggerOptions>>();

            return new SwaggerGenerator(
                serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>(),
                () => serviceProvider.GetService<ISchemaRegistry>(),
                optionAccessor.Options.SwaggerGeneratorOptions);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(IServiceProvider serviceProvider)
        {
            var mvcOptions = serviceProvider.GetService<MvcOptions>();
            // TODO: Get from mvcOptions
            return new JsonSerializerSettings();
        }
    }
}