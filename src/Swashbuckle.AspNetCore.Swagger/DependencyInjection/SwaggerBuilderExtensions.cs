using System;

#if (!NETSTANDARD2_0)
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerBuilderExtensions
    {
        /// <summary>
        /// Register the Swagger middleware with provided options
        /// </summary>
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, SwaggerOptions options)
        {
#if (!NETSTANDARD2_0)
            return app.UseMiddleware<SwaggerMiddleware>(options, app.ApplicationServices.GetRequiredService<TemplateBinderFactory>(), app.ApplicationServices.GetService<ISwaggerDocumentSerializer>());
#else
            return app.UseMiddleware<SwaggerMiddleware>(options, app.ApplicationServices.GetService<ISwaggerDocumentSerializer>());
#endif
        }

        /// <summary>
        /// Register the Swagger middleware with optional setup action for DI-injected options
        /// </summary>
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            Action<SwaggerOptions> setupAction = null)
        {
            SwaggerOptions options;
            using (var scope = app.ApplicationServices.CreateScope())
            {
                options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SwaggerOptions>>().Value;
                setupAction?.Invoke(options);
            }

            return app.UseSwagger(options);
        }

#if (!NETSTANDARD2_0)
        public static IEndpointConventionBuilder MapSwagger(
            this IEndpointRouteBuilder endpoints,
            string pattern = SwaggerOptions.DefaultRouteTemplate,
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

            return endpoints.MapGet(pattern, pipeline);
        }
#endif
    }
}
