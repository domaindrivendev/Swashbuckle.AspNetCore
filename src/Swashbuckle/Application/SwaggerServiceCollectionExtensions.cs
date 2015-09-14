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

            serviceCollection.AddTransient(GetSchemaProvider);
            serviceCollection.AddTransient(GetSwaggerProvider);

            serviceCollection.AddSingleton<SwaggerPathHelper>();
        }

        private static ISchemaProvider GetSchemaProvider(IServiceProvider serviceProvider)
        {
            var jsonSerializerSettings = GetJsonSerializerSettings(serviceProvider);
            var swaggerOptions = serviceProvider.GetService<IOptions<SwaggerOptions>>();
            return new DefaultSchemaProvider(jsonSerializerSettings, swaggerOptions.Options.SchemaGeneratorOptions);
        }

        private static ISwaggerProvider GetSwaggerProvider(IServiceProvider serviceProvider)
        {
            var optionAccessor = serviceProvider.GetService<IOptions<SwaggerOptions>>();

            return new SwaggerGenerator(
                serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>(),
                serviceProvider.GetService<ISchemaProvider>(),
                optionAccessor.Options.SwaggerGeneratorOptions);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(IServiceProvider serviceProvider)
        {
            var mvcOptions = serviceProvider.GetService<MvcOptions>();
            
            // get serialization settings if available
            var formatter = serviceProvider.GetService<JsonOutputFormatter>();
            if (formatter != null) return formatter.SerializerSettings;
            
            //otherwise create new serialization settings
            return new JsonSerializerSettings();
        }
    }
}
