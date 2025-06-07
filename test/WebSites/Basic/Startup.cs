using System.Globalization;
using System.Reflection;
using Basic.Swagger;
using Microsoft.AspNetCore.Localization;
using Microsoft.OpenApi.Models;

namespace Basic;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "Test API V1",
                    Version = "v1",
                    Description = "A sample API for testing Swashbuckle",
                    TermsOfService = new Uri("http://tempuri.org/terms")
                }
            );

            c.RequestBodyFilter<AssignRequestBodyVendorExtensions>();

            c.OperationFilter<AssignOperationVendorExtensions>();

            c.SchemaFilter<ExamplesSchemaFilter>();

            c.DescribeAllParametersInCamelCase();

            c.UseOneOfForPolymorphism();
            c.UseAllOfForInheritance();

            c.SelectDiscriminatorNameUsing((baseType) => "TypeName");
            c.SelectDiscriminatorValueUsing((subType) => subType.Name);

            c.IncludeXmlComments(Assembly.GetExecutingAssembly());

            c.EnableAnnotations();
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

            // Expose Swagger/OpenAPI JSON in different formats
            endpoints.MapSwagger("swagger/{documentName}/swagger.json");
            endpoints.MapSwagger("swagger/{documentName}/swaggerv2.json", c =>
            {
                c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
            });
        });

        var supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("fr"),
            new CultureInfo("sv-SE"),
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-US"),
            // Formatting numbers, dates, etc.
            SupportedCultures = supportedCultures,
            // UI strings that we have localized.
            SupportedUICultures = supportedCultures
        });

        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = ""; // serve the UI at root
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
        });
    }
}
