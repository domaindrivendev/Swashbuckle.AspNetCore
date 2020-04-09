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
        public static IEndpointRouteBuilder MapSwagger(this IEndpointRouteBuilder endpoints, string pattern)
        {
            if (!RoutePatternFactory.Parse(pattern).Parameters.Any(x => x.Name == "documentName"))
            {
                throw new ArgumentException("Pattern must contains '{documentName}' parameter", nameof(pattern));
            }

            endpoints.MapGet(pattern, InvokeAsync);

            return endpoints;
        }

        public static IEndpointRouteBuilder MapSwagger(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapSwagger("/swagger/{documentName}/swagger.json");
        }

        private async static Task InvokeAsync(HttpContext context)
        {
            var response = context.Response;
            if (context.Request.RouteValues.TryGetValue("documentName", out var documentName))
            {
                var provider = context.RequestServices.GetRequiredService<ISwaggerProvider>();
                var basePath = string.IsNullOrEmpty(context.Request.PathBase)
                    ? null
                    : context.Request.PathBase.ToString();

                try
                {
                    var swagger = provider.GetSwagger(documentName.ToString(), null, basePath);

                    var options = context.RequestServices.GetRequiredService<IOptions<SwaggerOptions>>().Value;

                    foreach (var filter in options.PreSerializeFilters)
                    {
                        filter(swagger, context.Request);
                    }

                    response.StatusCode = 200;
                    response.ContentType = "application/json;charset=utf-8";

                    using (var textWriter = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        var jsonWriter = new OpenApiJsonWriter(textWriter);
                        if (options.SerializeAsV2) swagger.SerializeAsV2(jsonWriter);
                        else
                        {
                            swagger.SerializeAsV3(jsonWriter);
                        }

                        await response.WriteAsync(textWriter.ToString(), new UTF8Encoding(false));
                    }

                    return;
                }
                catch (UnknownSwaggerDocument)
                {

                }
            }

            response.StatusCode = StatusCodes.Status404NotFound;
        }
#endif
    }
}