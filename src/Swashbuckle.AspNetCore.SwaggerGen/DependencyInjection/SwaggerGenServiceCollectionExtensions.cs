using Microsoft.Extensions.ApiDescriptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

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
        services.ConfigureOptions<ConfigureSwaggerGenJsonOptions>();
        services.TryAddSingleton<ISerializerDataContractResolver>(s =>
        {
            var serializerOptions = s.GetRequiredService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions;
            return new JsonSerializerDataContractResolver(serializerOptions);
        });

        // Used by the <c>dotnet-getdocument</c> tool from the Microsoft.Extensions.ApiDescription.Server package.
        services.TryAddSingleton<IDocumentProvider, DocumentProvider>();

        if (setupAction != null) services.ConfigureSwaggerGen(setupAction);

        return services;
    }

    public static IServiceCollection AddSwaggerGenMinimalApisJsonOptions(this IServiceCollection services)
    {
        return services.ConfigureOptions<ConfigureMinimalApiSwaggerGenJsonOptions>();
    }

    public static IServiceCollection AddSwaggerGenMvcJsonOptions(this IServiceCollection services)
    {
        return services.ConfigureOptions<ConfigureMvcSwaggerGenJsonOptions>();
    }

    public static void ConfigureSwaggerGen(
        this IServiceCollection services,
        Action<SwaggerGenOptions> setupAction)
    {
        services.Configure(setupAction);
    }
}
