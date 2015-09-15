using System;
using Microsoft.Framework.OptionsModel;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Newtonsoft.Json;
using Swashbuckle.Swagger;
using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Application;

namespace Microsoft.Framework.DependencyInjection
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            services.AddSingleton(SchemaProviderFactory);
            services.Configure<SwaggerSchemaOptions>(options =>
            {
                options.ModelFilter<ApplySwaggerModelFilterAttributes>();
            });

            services.AddSingleton(SwaggerProviderFactory);
            services.Configure<SwaggerDocumentOptions>(options =>
            {
                options.OperationFilter<ApplySwaggerOperationAttributes>();
                options.OperationFilter<ApplySwaggerOperationFilterAttributes>();
                options.OperationFilter<ApplySwaggerResponseAttributes>();
            });

            services.AddSingleton<SwaggerPathHelper>();
        }

        public static void ConfigureSwaggerSchema(
            this IServiceCollection services,
            Action<SwaggerSchemaOptions> options)
        {
            services.Configure(options);
        }

        public static void ConfigureSwaggerDocument(
            this IServiceCollection services,
            Action<SwaggerDocumentOptions> options)
        {
            services.Configure(options);
        }

        private static ISchemaProvider SchemaProviderFactory(IServiceProvider serviceProvider)
        {
            var jsonSerializerSettings = GetJsonSerializerSettings(serviceProvider);
            var optionsAccessor = serviceProvider.GetService<IOptions<SwaggerSchemaOptions>>();
            return new DefaultSchemaProvider(jsonSerializerSettings, optionsAccessor.Options);
        }

        private static ISwaggerProvider SwaggerProviderFactory(IServiceProvider serviceProvider)
        {
            var optionsAccessor = serviceProvider.GetService<IOptions<SwaggerDocumentOptions>>();
            return new DefaultSwaggerProvider(
                serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>(),
                serviceProvider.GetRequiredService<ISchemaProvider>(),
                optionsAccessor.Options);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(IServiceProvider serviceProvider)
        {
            var jsonOptions = serviceProvider.GetService<IOptions<MvcJsonOptions>>();
            return jsonOptions.Options.SerializerSettings;
        }
    }
}
