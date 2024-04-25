using System;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions to configure swagger in the ServiceCollection
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    public static void ConfigureSwagger(
        this IServiceCollection services,
        Action<SwaggerOptions> setupAction)
    {
        services.Configure(setupAction);
    }
}
