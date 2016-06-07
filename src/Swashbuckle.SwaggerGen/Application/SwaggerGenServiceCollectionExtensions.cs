using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Application;
using Swashbuckle.SwaggerGen.Generator;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static void AddSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            services.Configure(setupAction ?? (opts => { }));

            services.AddSingleton(CreateSwaggerProvider);
        }

        public static void ConfigureSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction)
        {
            services.Configure(setupAction);
        }

        private static ISwaggerProvider CreateSwaggerProvider(IServiceProvider serviceProvider)
        {
            var swaggerGenOptions = serviceProvider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
            var mvcJsonOptions = serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value;
            var apiDescriptionsProvider = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            var schemaRegistryFactory = new SchemaRegistryFactory(
                mvcJsonOptions.SerializerSettings,
                swaggerGenOptions.GetSchemaRegistryOptions(serviceProvider)
            );

            return new SwaggerGenerator(
                apiDescriptionsProvider,
                schemaRegistryFactory,
                swaggerGenOptions.GetSwaggerGeneratorOptions(serviceProvider)
            );
        }
    }
}
