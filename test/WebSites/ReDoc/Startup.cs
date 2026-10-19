using Swashbuckle.AspNetCore.ReDoc;

namespace ReDoc;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", () => Results.Redirect("api-docs"));

            // Expose the OpenAPI document in other versions of the specification for the UI tests
            endpoints.MapSwagger("swagger/{documentName}/swaggerv3_1.json", c => c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1);
            endpoints.MapSwagger("swagger/{documentName}/swaggerv3_2.json", c => c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_2);
        });

        app.UseSwagger(c =>
        {
            c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
        });

        app.UseReDoc(c =>
        {
            c.RoutePrefix = "api-docs";

            c.ConfigObject = new ConfigObject
            {
                HideDownloadButton = true,
                HideLoading = true
            };
        });

        app.UseReDoc(c =>
        {
            c.RoutePrefix = "api-docs-v3_1";
            c.SpecUrl = "../swagger/v1/swaggerv3_1.json";
        });

        app.UseReDoc(c =>
        {
            c.RoutePrefix = "api-docs-v3_2";
            c.SpecUrl = "../swagger/v1/swaggerv3_2.json";
        });
    }
}
