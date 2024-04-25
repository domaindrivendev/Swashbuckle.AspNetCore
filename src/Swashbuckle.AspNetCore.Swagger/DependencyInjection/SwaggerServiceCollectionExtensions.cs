using System;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.DependencyInjection;

public static class SwaggerServiceCollectionExtensions
{
    public static void ConfigureSwagger(
        this IServiceCollection services,
        Action<SwaggerOptions> setupAction)
    {
        services.Configure(setupAction);
    }
}
