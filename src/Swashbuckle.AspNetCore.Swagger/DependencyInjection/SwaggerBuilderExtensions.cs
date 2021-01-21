using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

#if NETCOREAPP3_0
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
#endif

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerBuilderExtensions
    {
        /// <summary>
        /// Register the Swagger middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, SwaggerOptions options)
        {
            return app.UseMiddleware<SwaggerMiddleware>(options);
        }

        /// <summary>
        /// Register the Swagger middleware with optional setup action for DI-injected options
        /// </summary>
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            Action<SwaggerOptions> setupAction = null)
        {
            SwaggerOptions options = app.ApplicationServices.GetRequiredService<IOptions<SwaggerOptions>>().Value;
            setupAction?.Invoke(options);

            return app.UseSwagger(options);
        }

#if NETCOREAPP3_0
        public static IEndpointRouteBuilder MapSwagger(
            this IEndpointRouteBuilder endpoints,
            string pattern = "/swagger/{documentName}/swagger.json",
            Action<SwaggerEndpointOptions> setupAction = null)
        {
            if (!RoutePatternFactory.Parse(pattern).Parameters.Any(x => x.Name == "documentName"))
            {
                throw new ArgumentException("Pattern must contain '{documentName}' parameter", nameof(pattern));
            }

            Action<SwaggerOptions> endpointSetupAction = options =>
            {
                var endpointOptions = new SwaggerEndpointOptions();

                setupAction?.Invoke(endpointOptions);

                options.RouteTemplate = pattern;
                options.SerializeAsV2 = endpointOptions.SerializeAsV2;
                options.PreSerializeFilters.AddRange(endpointOptions.PreSerializeFilters);
            };

            var pipeline = endpoints.CreateApplicationBuilder()
                .UseSwagger(endpointSetupAction)
                .Build();

            endpoints.MapGet(pattern, pipeline);

            return endpoints;
        }
#endif
    }
}