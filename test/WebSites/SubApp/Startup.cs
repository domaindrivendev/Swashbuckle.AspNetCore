using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace SubApp
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
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Test API V1",}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder rootApp, IWebHostEnvironment env)
        {
            rootApp.Map("/subapp", app =>
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseRouting();
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = ""; // serve the UI at root
                    c.SwaggerEndpoint("swagger/v1/swagger.json", "V1 Docs");
                });
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapSwagger();
                    endpoints.MapControllers();
                });
            });
        }
    }
}