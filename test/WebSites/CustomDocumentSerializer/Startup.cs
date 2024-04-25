using Swashbuckle.AspNetCore.Swagger;

namespace CustomDocumentSerializer;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "CustomDocumentSerializer", Version = "v1" });
        });
        services.ConfigureSwagger(c =>
        {
            c.SetCustomDocumentSerializer<DocumentSerializerTest>();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapSwagger("swagger/{documentName}/swagger.json");
            endpoints.MapSwagger("swagger/{documentName}/swaggerv2.json", c => c.SerializeAsV2 = true);
        });
    }
}
