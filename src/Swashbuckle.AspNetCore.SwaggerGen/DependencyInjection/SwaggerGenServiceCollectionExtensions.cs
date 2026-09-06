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
            var generatorOptions = s.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value;
            return new JsonSerializerDataContractResolver(serializerOptions, generatorOptions);
        });

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
             * First try to get the options configured for MVC, but only when MVC is actually
             * registered: IOptions<Mvc.JsonOptions> resolves in any application, so in a Minimal
             * APIs application it hands back MVC's own defaults and hides whatever the application
             * configured through ConfigureHttpJsonOptions,
             * then try to get the options configured for Minimal APIs if available,
             * then try the default JsonSerializerOptions if available,
             * otherwise create a new instance as a last resort as this is an expensive operation.
             */
            serializerOptions =
                (IsMvcRegistered() ? _serviceProvider.GetService<IOptions<AspNetCore.Mvc.JsonOptions>>()?.Value?.JsonSerializerOptions : null)
                ?? _serviceProvider.GetService<IOptions<AspNetCore.Http.Json.JsonOptions>>()?.Value?.SerializerOptions
                ?? JsonSerializerOptions.Default;

            return serializerOptions;
        }

        /// <summary>
        /// Whether the application registered MVC, through <c>AddControllers()</c>, <c>AddMvcCore()</c>
        /// or <c>AddMvc()</c>. <c>IControllerFactory</c> is registered by all three, and by nothing a
        /// Minimal APIs application uses, including <c>AddEndpointsApiExplorer()</c>.
        /// </summary>
        private bool IsMvcRegistered()
            => _serviceProvider.GetService<AspNetCore.Mvc.Controllers.IControllerFactory>() is not null;
    }
}
