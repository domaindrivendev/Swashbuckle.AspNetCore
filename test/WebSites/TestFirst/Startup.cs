using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#if NETCOREAPP3_0
using Microsoft.Extensions.Hosting;
#endif

namespace TestFirst
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if NETCOREAPP3_0
            services.AddMvc(options => options.EnableEndpointRouting = false);
#else
            services.AddMvc();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#if NETCOREAPP3_0
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
#else
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
#endif
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Serve Swagger JSON from static file (i.e. pre-generated via "dotnet swagger")
            app.UseStaticFiles();

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api-docs";
                c.SwaggerEndpoint("v1-imported/openapi.json", "V1 Docs (Imported)");
                c.SwaggerEndpoint("v1-generated/openapi.json", "V1 Docs (Generated)");
            });

            app.UseMvc();
        }
    }
}
