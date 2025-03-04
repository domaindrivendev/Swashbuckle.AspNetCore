namespace CustomDocumentSerializer;

public class Startup
{
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

    public void Configure(IApplicationBuilder app)
    {
        app.UseSwagger();

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapSwagger("swagger/{documentName}/swagger.json");
            endpoints.MapSwagger("swagger/{documentName}/swaggerv2.json", c => c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
        });
    }
}
