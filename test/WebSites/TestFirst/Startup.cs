namespace TestFirst;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
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

        // Serve Swagger JSON from static file (i.e. pre-generated via "dotnet swagger")
        app.UseStaticFiles();

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1-imported/openapi.json", "V1 Docs (Imported)");
            c.SwaggerEndpoint("v1-generated/openapi.json", "V1 Docs (Generated)");
        });
    }
}
