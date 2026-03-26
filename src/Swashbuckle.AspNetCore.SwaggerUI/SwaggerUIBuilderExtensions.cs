using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Microsoft.AspNetCore.Builder;

public static class SwaggerUIBuilderExtensions
{
    /// <summary>
    /// Register the SwaggerUI middleware with provided options
    /// </summary>
    public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app, SwaggerUIOptions options)
        => app.UseMiddleware<SwaggerUIMiddleware>(options);

    /// <summary>
    /// Register the SwaggerUI middleware with optional setup action for DI-injected options
    /// </summary>
    public static IApplicationBuilder UseSwaggerUI(
        this IApplicationBuilder app,
        Action<SwaggerUIOptions> setupAction = null)
    {
        var options = ResolveOptions(app.ApplicationServices, setupAction);
        var hostingEnv = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        EnsureDefaultUrl(options, hostingEnv.ApplicationName);

        return app.UseSwaggerUI(options);
    }

    /// <summary>
    /// Maps the SwaggerUI middleware to the specified endpoint route.
    /// </summary>
    /// <remarks>
    /// This is a convenience extension method that combines the registration of the SwaggerUI middleware with endpoint
    /// routing. It allows you to specify the route pattern for the SwaggerUI and optionally configure the <see cref="SwaggerUIOptions"/>.
    /// </remarks>
    /// <param name="endpoints">Endpoint route builder to which the SwaggerUI middleware will be mapped.</param>
    /// <param name="routePrefix">Optional: The route to register SwaggerUI on. Defaults to the <see cref="SwaggerUIOptions.RoutePrefix"/> value.</param>
    /// <param name="setupAction">Optional setup action to configure the <see cref="SwaggerUIOptions"/>.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> that can be used to further configure the endpoint.</returns>
    public static IEndpointConventionBuilder MapSwaggerUI(
        this IEndpointRouteBuilder endpoints,
        string routePrefix = null,
        Action<SwaggerUIOptions> setupAction = null)
    {
        var options = ResolveOptions(endpoints.ServiceProvider, setupAction);

        if (routePrefix != null)
        {
            options.RoutePrefix = routePrefix;
        }

        var hostingEnv = endpoints.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        EnsureDefaultUrl(options, hostingEnv.ApplicationName);

        var pipeline = endpoints.CreateApplicationBuilder()
            .UseSwaggerUI(options)
            .Build();

        return endpoints.Map(GetRoutePattern(options.RoutePrefix), pipeline);
    }

    private static SwaggerUIOptions ResolveOptions(IServiceProvider serviceProvider, Action<SwaggerUIOptions> setupAction)
    {
        using var scope = serviceProvider.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SwaggerUIOptions>>().Value;

        setupAction?.Invoke(options);

        return options;
    }

    private static void EnsureDefaultUrl(SwaggerUIOptions options, string applicationName)
    {
        // To simplify the common case, use a default that will work with the SwaggerMiddleware defaults
        options.ConfigObject.Urls ??= [new UrlDescriptor { Name = $"{applicationName} v1", Url = "v1/swagger.json" }];
    }

    private static string GetRoutePattern(string routePrefix)
    {
        var sanitizedRoutePrefix = routePrefix?.Trim('/');
        return string.IsNullOrEmpty(sanitizedRoutePrefix)
            ? "{**path}"
            : $"{sanitizedRoutePrefix}/{{**path}}";
    }
}
