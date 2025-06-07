using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MultipleVersions;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddApiVersioning();
        services.AddVersionedApiExplorer();

        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseSwagger();

        // A common endpoint that contains both versions
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger";
            foreach (var description in provider.ApiVersionDescriptions)
            {
                c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Version {description.GroupName}");
            }
            c.EnableSwaggerDocumentUrlsEndpoint();
        });

        // Separate endpoints that contain only one version
        foreach (var description in provider.ApiVersionDescriptions)
        {
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = $"swagger/{description.GroupName}";
                c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Version {description.GroupName}");
            });
            app.UseReDoc(c =>
            {
                c.RoutePrefix = $"redoc/{description.GroupName}";
                c.SpecUrl($"/swagger/{description.GroupName}/swagger.json");
            });
        }
    }
}
