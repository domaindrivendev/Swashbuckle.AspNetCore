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
    }
}
