using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace NetCore21
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
            services.AddMvc()
                .AddJsonOptions(c =>
                {
                    c.SerializerSettings.Converters.Add(new StringEnumConverter(camelCaseText: true));
                    c.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test API", Version = "1" });

                c.UseAllOfToExtendReferenceSchemas();
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            // Expose v2 and v3 Swagger/OpenAPI formats
            app.UseSwagger(new SwaggerOptions
            {
                RouteTemplate = "/swagger/{documentName}/swagger.json",
                SerializeAsV2 = true
            });
            app.UseSwagger(new SwaggerOptions
            {
                RouteTemplate = "/swagger/{documentName}/openapi.json",
            });

            app.UseSwaggerUI();
        }
    }
}
