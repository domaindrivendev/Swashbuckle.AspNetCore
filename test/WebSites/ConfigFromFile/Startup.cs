using Swashbuckle.AspNetCore.ReDoc;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ConfigFromFile;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen();

        services.Configure<SwaggerGenOptions>(Configuration.GetSection("SwaggerGen"));

        services.Configure<SwaggerOptions>(Configuration.GetSection("Swagger"));

        services.Configure<SwaggerUIOptions>(c =>
        {
            Configuration.Bind("SwaggerUI", c);

            c.ConfigObject.SupportedSubmitMethods = new SubmitMethod[]{ };
            c.ConfigObject.AdditionalItems.Add("swaggerUIFoo", "bar");
        });

        services.Configure<ReDocOptions>(c =>
        {
            Configuration.Bind("ReDoc", c);

            c.ConfigObject.AdditionalItems.Add("redocFoo", "bar");
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseSwagger();

        app.UseSwaggerUI();

        app.UseReDoc();
    }
}
