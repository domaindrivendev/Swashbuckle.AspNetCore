using Microsoft.OpenApi;

namespace QueryOperations;

/// <summary>
/// A web site using the HTTP QUERY method, which is a first-class operation in OpenAPI 3.2.
/// The QUERY action is only available when targeting .NET 11 or later, as earlier versions
/// of Microsoft.OpenApi cannot serialize it correctly for OpenAPI 3.1 and earlier.
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
#if NET11_0_OR_GREATER
            endpoints.MapSwagger("swagger/{documentName}/swaggerv3_2.json", c => c.OpenApiVersion = OpenApiSpecVersion.OpenApi3_2);
#endif
        });
    }
}
