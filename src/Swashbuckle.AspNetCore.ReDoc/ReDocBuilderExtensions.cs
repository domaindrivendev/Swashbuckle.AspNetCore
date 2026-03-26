using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.ReDoc;

namespace Microsoft.AspNetCore.Builder;

public static class ReDocBuilderExtensions
{
    /// <summary>
    /// Register the Redoc middleware with provided options
    /// </summary>
    public static IApplicationBuilder UseReDoc(this IApplicationBuilder app, ReDocOptions options)
        => app.UseMiddleware<ReDocMiddleware>(options);

    /// <summary>
    /// Register the Redoc middleware with optional setup action for DI-injected options
    /// </summary>
    public static IApplicationBuilder UseReDoc(
        this IApplicationBuilder app,
        Action<ReDocOptions> setupAction = null)
    {
        var options = ResolveOptions(app.ApplicationServices, setupAction);

        EnsureDefaultSpecUrl(options);

        return app.UseReDoc(options);
    }

    /// <summary>
    /// Maps the Redoc middleware to the specified endpoint route.
    /// </summary>
    /// <param name="endpoints">Endpoint route builder to which the Redoc middleware will be mapped.</param>
    /// <param name="routePrefix">Optional route prefix for the Redoc endpoint. If not provided, the <see cref="ReDocOptions.RoutePrefix"/> value is used.</param>
    /// <param name="setupAction">Optional setup action to configure the Redoc options.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> that can be used to further configure the endpoint.</returns>
    public static IEndpointConventionBuilder MapReDoc(
        this IEndpointRouteBuilder endpoints,
        string routePrefix = null,
        Action<ReDocOptions> setupAction = null)
    {
        var options = ResolveOptions(endpoints.ServiceProvider, setupAction);

        if (routePrefix != null)
        {
            options.RoutePrefix = routePrefix;
        }

        EnsureDefaultSpecUrl(options);

        var pipeline = endpoints.CreateApplicationBuilder()
            .UseReDoc(options)
            .Build();

        return endpoints.Map(GetRoutePattern(options.RoutePrefix), pipeline);
    }

    private static ReDocOptions ResolveOptions(IServiceProvider serviceProvider, Action<ReDocOptions> setupAction)
    {
        using var scope = serviceProvider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ReDocOptions>>().Value;
        setupAction?.Invoke(options);
        return options;
    }

    private static void EnsureDefaultSpecUrl(ReDocOptions options)
    {
        // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
        options.SpecUrl ??= "../swagger/v1/swagger.json";
    }

    private static string GetRoutePattern(string routePrefix)
    {
        var sanitizedRoutePrefix = routePrefix?.Trim('/');
        return string.IsNullOrEmpty(sanitizedRoutePrefix)
            ? "{**path}"
            : $"{sanitizedRoutePrefix}/{{**path}}";
    }
}
