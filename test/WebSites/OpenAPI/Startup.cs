using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace OpenAPI
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnv;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnv)
        {
            Configuration = configuration;
            _hostingEnv = hostingEnv;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                }); ;

            services.AddSwaggerGen(c =>
            {
                
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Version = "v1",
                        Title = "Swashbuckle Sample API",
                        Description = "A sample API for testing Swashbuckle",
                        TermsOfService = "Some terms ..."
                    }
                );


                c.DescribeAllEnumsAsStrings();
                c.EnableAnnotations();
            });

            if (_hostingEnv.IsDevelopment())
            {
                services.ConfigureSwaggerGen(c =>
                {
                    var xmlCommentsPath = Path.Combine(System.AppContext.BaseDirectory, "OpenAPI.xml");
                    c.IncludeXmlComments(xmlCommentsPath, true);
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Add MVC to the request pipeline.
            app.UseDeveloperExceptionPage();

            app.UseMvc();

            app.UseSwaggerWithOpenAPILayer(swaggerOptions =>
            {
                swaggerOptions.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
            },
            openAPIOptions =>
            {
                openAPIOptions.CompabilityLayerActive = true;
                openAPIOptions.Format = Microsoft.OpenApi.OpenApiFormat.Yaml;
                openAPIOptions.Version = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = ""; // serve the UI at root
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                c.DisplayOperationId();
            });
        }
    }
}
