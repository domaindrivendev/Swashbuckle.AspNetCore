using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ApiDescription;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
#if NETCOREAPP3_0
            services.AddTransient<ISerializerSettingsAccessor, MvcNewtonsoftJsonOptionsAccessor>();
#else
            services.AddTransient<ISerializerSettingsAccessor, MvcJsonOptionsAccessor>();
#endif

            services.AddTransient<ISwaggerProvider, SwaggerGenerator>();
            services.AddTransient<ISchemaGenerator, SchemaGenerator>(p =>
                new SchemaGenerator(
                    p.GetRequiredService<IOptions<SchemaGeneratorOptions>>(),
                    p.GetRequiredService<ISerializerSettingsAccessor>()));

            // Register custom configurators that assign values from SwaggerGenOptions (i.e. high level config)
            // to the service-specific options (i.e. lower-level config)
            services.AddTransient<IConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwaggerGeneratorOptions>();
            services.AddTransient<IConfigureOptions<SchemaGeneratorOptions>, ConfigureSchemaGeneratorOptions>();

            // Used by the Microsoft.Extensions.ApiDescription tool
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

#if NETCOREAPP3_0
        private sealed class MvcNewtonsoftJsonOptionsAccessor : ISerializerSettingsAccessor
        {
            public MvcNewtonsoftJsonOptionsAccessor(IOptions<MvcNewtonsoftJsonOptions> options)
            {
                Options = options;
            }

            public JsonSerializerSettings SerializerSettings => Options.Value?.SerializerSettings;

            private IOptions<MvcNewtonsoftJsonOptions> Options { get; }
        }
#else
        private sealed class MvcJsonOptionsAccessor : ISerializerSettingsAccessor
        {
            public MvcJsonOptionsAccessor(IOptions<MvcJsonOptions> options)
            {
                Options = options;
            }

            public JsonSerializerSettings SerializerSettings => Options.Value?.SerializerSettings;

            private IOptions<MvcJsonOptions> Options { get; }
        }
#endif
    }
}
