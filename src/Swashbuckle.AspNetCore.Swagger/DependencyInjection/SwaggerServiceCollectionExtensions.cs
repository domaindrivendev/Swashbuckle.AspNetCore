using System;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions to configure swagger in the service collection
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Configure swagger options in the service collection.
    /// </summary>
    /// <param name="services">The service collection to set the swagger options on.</param>
    /// <param name="setupAction">The setup action for swagger.</param>
    public static void ConfigureSwagger(
        this IServiceCollection services,
        Action<SwaggerOptions> setupAction)
    {
        services.Configure(setupAction);
    }
}
