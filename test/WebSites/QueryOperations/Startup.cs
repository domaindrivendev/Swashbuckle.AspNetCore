using Microsoft.OpenApi;

namespace QueryOperations;

/// <summary>
/// A web site using the HTTP QUERY method, which is a first-class operation in OpenAPI 3.2.
/// Earlier versions of the OpenAPI specification describe it using the
/// <c>x-oai-additionalOperations</c> extension instead.
/// </summary>
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "QueryOperations", Version = "v1" });
            c.IncludeXmlComments(typeof(Startup).Assembly);
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapSwagger("swagger/{documentName}/swagger.json");
            endpoints.MapSwagger("swagger/{documentName}/swaggerv3_2.json", c => c.OpenApiVersion = OpenApiSpecVersion.OpenApi3_2);
        });
    }
}
