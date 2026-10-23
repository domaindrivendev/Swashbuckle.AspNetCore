using Microsoft.OpenApi;

namespace Unions;

/// <summary>
/// A web site using C# union types, which System.Text.Json serializes as the value of the case they contain.
/// </summary>
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Unions", Version = "v1" });
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
            endpoints.MapSwagger("swagger/{documentName}/swaggerv3_1.json", c => c.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
            endpoints.MapSwagger("swagger/{documentName}/swaggerv3_2.json", c => c.OpenApiVersion = OpenApiSpecVersion.OpenApi3_2);
        });
    }
}
