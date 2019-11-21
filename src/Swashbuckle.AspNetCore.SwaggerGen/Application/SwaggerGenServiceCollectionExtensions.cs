using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            // Add Mvc convention to ensure ApiExplorer is enabled for all actions
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            // Register generator and it's dependencies
            services.AddTransient(s => s.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value);
            services.AddTransient<ISwaggerProvider, SwaggerGenerator>();
            services.AddTransient(s => s.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value);
            services.AddTransient<ISchemaGenerator, SchemaGenerator>();
            services.AddTransient<IApiModelResolver>(s =>
            {
                var jsonSerializerOptions = s.GetJsonSerializerOptions() ?? new JsonSerializerOptions();

                return new JsonApiModelResolver(jsonSerializerOptions);
            });

            // Register custom configurators that assign values from SwaggerGenOptions (i.e. high level config)
            // to the service-specific options (i.e. lower-level config)
            services.AddTransient<IConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwaggerGeneratorOptions>();
            services.AddTransient<IConfigureOptions<SchemaGeneratorOptions>, ConfigureSchemaGeneratorOptions>();

            // Used by the <c>dotnet-getdocument</c> tool from the Microsoft.Extensions.ApiDescription.Server package.
            services.TryAddSingleton<IDocumentProvider, DocumentProvider>();

            if (setupAction != null) services.ConfigureSwaggerGen(setupAction);

            return services;
        }

        public static void ConfigureSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction)
        {
            services.Configure(setupAction);
        }

        private static JsonSerializerOptions GetJsonSerializerOptions(this IServiceProvider serviceProvider)
        {
#if NETCOREAPP3_0
            return serviceProvider.GetService<IOptions<JsonOptions>>()?.Value?.JsonSerializerOptions;
#else
            return null;
#endif
        }
    }
}
