using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MultipleUrls
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

            services.AddApiVersioning();
            services.AddVersionedApiExplorer();

            services.AddSwaggerGen();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            foreach (var description in provider.ApiVersionDescriptions) {
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = $"api-docs/{description.GroupName}";
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());

                    foreach (var innerDescription in provider.ApiVersionDescriptions)
                    {
                        if (description == innerDescription)
                            continue;

                        c.SwaggerEndpoint($"/swagger/{innerDescription.GroupName}/swagger.json", innerDescription.GroupName.ToUpperInvariant());
                    }
                });

                app.UseReDoc(c =>
                {
                    c.RoutePrefix = $"api-docs/{description.GroupName}/redoc";
                    c.SpecUrl = $"/swagger/{description.GroupName}/swagger.json";
                });
            }
        }
    }
}
