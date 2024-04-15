using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Localization;
using Basic.Swagger;

namespace Basic
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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

                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Basic.xml"));

                c.EnableAnnotations();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

                // Expose Swagger/OpenAPI JSON in new (v3) and old (v2) formats
                endpoints.MapSwagger("swagger/{documentName}/swagger.json");
                endpoints.MapSwagger("swagger/{documentName}/swaggerv2.json", c =>
                {
                    c.SerializeAsV2 = true;
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
}
