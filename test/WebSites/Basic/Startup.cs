using System.Globalization;
using System.Reflection;
using Basic.Swagger;
using Microsoft.AspNetCore.Localization;
using Microsoft.OpenApi;

namespace Basic;

public class Startup : StartupBase
{

    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        base.Configure(app, env);
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = ""; // serve the UI at root
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
        });

        // Expose the UI for other versions of the OpenAPI specification for the UI tests
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger-v3_1";
            c.SwaggerEndpoint("/swagger/v1/swaggerv3_1.json", "V1 Docs (OpenAPI 3.1)");
        });

        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger-v3_2";
            c.SwaggerEndpoint("/swagger/v1/swaggerv3_2.json", "V1 Docs (OpenAPI 3.2)");
        });
    }
}
