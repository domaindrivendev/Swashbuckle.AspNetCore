using System.Text.Json;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection;

public static class SwaggerGenServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerGen(
        this IServiceCollection services,
        Action<SwaggerGenOptions> setupAction = null)
    {
        // Add Mvc convention to ensure ApiExplorer is enabled for all actions
        services.Configure<AspNetCore.Mvc.MvcOptions>(c =>
            c.Conventions.Add(new SwaggerApplicationConvention()));

        // Register custom configurators that takes values from SwaggerGenOptions (i.e. high level config)
        // and applies them to SwaggerGeneratorOptions and SchemaGeneratorOptions (i.e. lower-level config)
        services.AddTransient<IConfigureOptions<SwaggerGeneratorOptions>, ConfigureSwaggerGeneratorOptions>();
        services.AddTransient<IConfigureOptions<SchemaGeneratorOptions>, ConfigureSchemaGeneratorOptions>();

        // Register generator and its dependencies
        services.TryAddTransient<SwaggerGenerator>();
        services.TryAddTransient<ISwaggerProvider>(s => s.GetRequiredService<SwaggerGenerator>());
        services.TryAddTransient<IAsyncSwaggerProvider>(s => s.GetRequiredService<SwaggerGenerator>());
        services.TryAddTransient(s => s.GetRequiredService<IOptions<SwaggerGeneratorOptions>>().Value);
        services.TryAddTransient<ISchemaGenerator, SchemaGenerator>();
        services.TryAddTransient(s => s.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value);
        services.AddSingleton<JsonSerializerOptionsProvider>();
        services.TryAddSingleton<ISerializerDataContractResolver>(s =>
        {
            var serializerOptions = s.GetRequiredService<JsonSerializerOptionsProvider>().Options;
            return new JsonSerializerDataContractResolver(serializerOptions);
        });

        // Used by the <c>dotnet-getdocument</c> tool from the Microsoft.Extensions.ApiDescription.Server package.
        services.TryAddSingleton<IDocumentProvider, DocumentProvider>();

        if (setupAction != null) services.ConfigureSwaggerGen(setupAction);

        return services;
    }

    public static IServiceCollection AddSwaggerGenMinimalApisJsonOptions(this IServiceCollection services)
    {
        return services.Replace(
            ServiceDescriptor.Transient<ISerializerDataContractResolver>((s) =>
            {
                var options = s.GetRequiredService<IOptionsSnapshot<AspNetCore.Http.Json.JsonOptions>>().Value.SerializerOptions;

                return new JsonSerializerDataContractResolver(options);
            }));
    }

    public static IServiceCollection AddSwaggerGenMvcJsonOptions(this IServiceCollection services)
    {
        return services.Replace(
            ServiceDescriptor.Transient<ISerializerDataContractResolver>((s) =>
            {
                var options = s.GetRequiredService<IOptionsSnapshot<AspNetCore.Mvc.JsonOptions>>().Value.JsonSerializerOptions;

                return new JsonSerializerDataContractResolver(options);
            }));
    }

    public static void ConfigureSwaggerGen(
        this IServiceCollection services,
        Action<SwaggerGenOptions> setupAction)
    {
        services.Configure(setupAction);
    }

    private sealed class JsonSerializerOptionsProvider
    {
        private JsonSerializerOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public JsonSerializerOptionsProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public JsonSerializerOptions Options => _options ??= ResolveOptions();

        private JsonSerializerOptions ResolveOptions()
        {
            JsonSerializerOptions serializerOptions;

            /*
             * There is no surefire way to do this.
             * However, both JsonOptions are defaulted in the same way.
             * If neither is configured it makes no difference which one is chosen.
             * If both are configured, then we just need to make a choice.
             * As Minimal APIs are newer if someone is configuring them
             * it's probably more likely that is what they're using.
             * 
             * If either JsonOptions is null we will try to create a new instance as
             * a last resort as this is an expensive operation.
             */
            serializerOptions =
                _serviceProvider.GetService<IOptions<AspNetCore.Http.Json.JsonOptions>>()?.Value?.SerializerOptions
                ?? JsonSerializerOptions.Default;

            if (HasConfiguredMinimalApiJsonOptions())
            {
                serializerOptions ??= _serviceProvider.GetService<IOptions<AspNetCore.Http.Json.JsonOptions>>()?.Value?.SerializerOptions;
            }

            return serializerOptions;
        }

        private bool HasConfiguredMinimalApiJsonOptions()
        {
            if (_serviceProvider.GetService<IEnumerable<IConfigureOptions<AspNetCore.Http.Json.JsonOptions>>>().Any())
                return true;

            return _serviceProvider.GetService<IEnumerable<IPostConfigureOptions<AspNetCore.Http.Json.JsonOptions>>>().Any();
        }
    }
}
