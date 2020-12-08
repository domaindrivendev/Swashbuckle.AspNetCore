using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Filters.Swagger;

namespace Filters
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
                        Title = "Test Filters API V1",
                        Version = "v1",
                        Description = "A sample API for testing Swashbuckle filters"
                    }
                );

                c.SwaggerDoc("v2",
                    new OpenApiInfo
                    {
                        Title = "Test Filters API V2",
                        Version = "v2",
                        Description = "A sample API for testing Swashbuckle filters"
                    }
                );

                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Filters.xml"));
                c.EnableAnnotations();

                c.DocumentFilter<V1DocumentFilter>();
                c.OperationFilter<V1OperationFilter>();
                c.ParameterFilter<V1ParameterFilter>();
                c.RequestBodyFilter<V1RequestBodyFilter>();
                c.SchemaFilter<V1SchemaFilter>();
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
                endpoints.MapSwagger("swagger/{documentName}/swagger.json");
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = ""; // serve the UI at root
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
            });
        }
    }
}
