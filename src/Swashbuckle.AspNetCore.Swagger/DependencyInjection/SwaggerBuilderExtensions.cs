using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

#if NETCOREAPP3_0

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
#endif

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerBuilderExtensions
    {
        public static IApplicationBuilder UseSwagger(
            this IApplicationBuilder app,
            Action<SwaggerOptions> setupAction = null)
        {
            SwaggerOptions options = new SwaggerOptions();
            if (setupAction != null)
            {
                setupAction(options);
            }
            else
            {
                options = app.ApplicationServices.GetRequiredService<IOptions<SwaggerOptions>>().Value;
            }

            app.UseMiddleware<SwaggerMiddleware>(options);

            return app;
        }
#if NETCOREAPP3_0
        public static IEndpointRouteBuilder MapSwagger(
            this IEndpointRouteBuilder endpoints,
            string pattern,
            Action<SwaggerOptions> setupAction = null)
        {
            if (!RoutePatternFactory.Parse(pattern).Parameters.Any(x => x.Name == "documentName"))
            {
                throw new ArgumentException("Pattern must contain '{documentName}' parameter", nameof(pattern));
            }

            Action<SwaggerOptions> endpointSetupAction = options =>
            {
                setupAction?.Invoke(options);

                options.RouteTemplate = pattern;
            };

            var pipeline = endpoints.CreateApplicationBuilder()
                .UseSwagger(endpointSetupAction)
                .Build();

            endpoints.MapGet(pattern, pipeline);

            return endpoints;
        }

        public static IEndpointRouteBuilder MapSwagger(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapSwagger("/swagger/{documentName}/swagger.json");
        }
#endif
    }
}