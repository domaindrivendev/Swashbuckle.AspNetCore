using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions to configure dependencies for Swagger.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Configures Swagger options in the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to configure the Swagger options for.</param>
    /// <param name="setupAction">A delegate to a method to use to configure the Swagger options.</param>
    public static void ConfigureSwagger(
        this IServiceCollection services,
        Action<SwaggerOptions> setupAction)
    {
        services.Configure(setupAction);
    }
}
