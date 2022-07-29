using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

#if NETSTANDARD2_0
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#endif

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        /// <summary>
        /// Register the SwaggerUI middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseSwaggerUI(this IApplicationBuilder app, SwaggerUIOptions options)
        {
            return app.UseMiddleware<SwaggerUIMiddleware>(options);
        }

        /// <summary>
        /// Register the SwaggerUI middleware with DI-injected options
        /// </summary>
        /// <param name="setupAction">Optional action to modifiy the DI-injected options</param>
        public static IApplicationBuilder UseSwaggerUI(
            this IApplicationBuilder app,
            Action<SwaggerUIOptions> setupAction = null)
        {
            SwaggerUIOptions options = FetchAndConfigureOptions(endpoints.ServiceProvider, setupAction);

            return app.UseSwaggerUI(options);
        }

#if (!NETSTANDARD2_0)
        /// <summary>
        /// Register the SwaggerUI middleware with DI-injected options. Uses the endpoint routing syntax.
        /// </summary>
        /// <param name="setupAction">Optional action to modifiy the DI-injected options</param>
        public static IEndpointConventionBuilder MapSwaggerUI(
            this IEndpointRouteBuilder endpoints,
            Action<SwaggerUIOptions>? setupAction = null)
        {
            SwaggerUIOptions options = FetchAndConfigureOptions(endpoints.ServiceProvider, setupAction);

            var swaggerUiDelegate = endpoints.CreateApplicationBuilder()
                .Use((context, next) =>
                {
                        // workaround for https://github.com/dotnet/aspnetcore/issues/24252
                    context.SetEndpoint(null);
                    return next();
                })
                .UseSwaggerUI(options)
                .Build();

            return endpoints.MapGet(options.RoutePrefix + "/{*wildcard}", swaggerUiDelegate);
        }
#endif

        /// <summary>
        /// Fetches the options instance registered with the DI, applies the local setup action and configures some defaults.
        /// </summary>
        private static SwaggerUIOptions FetchAndConfigureOptions(IServiceProvider serviceProvider, Action<SwaggerUIOptions>? setupAction = null)
        {
            // if a preconfigured instance of SwaggerUIOptions is registered with the DI, 
            //      use it as a basis and then apply the local setup to it
            SwaggerUIOptions options;
            using (var scope = serviceProvider.CreateScope())
            {
                options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SwaggerUIOptions>>().Value;
                setupAction?.Invoke(options);
            }

            // To simplify the common case, use a default that will work with the SwaggerOptions and SwaggerGenOptions defaults
            if (options.ConfigObject.Urls == null)
            {
                var hostingEnv = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                options.ConfigObject.Urls = new[] { new UrlDescriptor { Name = $"{hostingEnv.ApplicationName} v1", Url = "v1/swagger.json" } };
            }

            return options;
        }
    }
}
