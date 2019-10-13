using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Basic.Swagger;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

#if NETCOREAPP3_0
using Microsoft.Extensions.Hosting;
#endif

namespace Basic
{
    public class Startup
    {
#if !NETCOREAPP3_0
        private readonly IHostingEnvironment _env;
        public Startup(IHostingEnvironment env)
        {
            _env = env;
        }
#endif

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
#if NETCOREAPP3_0
            services
                .AddMvc(options => options.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });
#else
            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });
#endif

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNetCore.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Swashbuckle Sample API",
                        Version = "v1",
                        Description = "A sample API for testing Swashbuckle",
                        TermsOfService = new Uri("http://tempuri.org/terms")
                    }
                );

                c.ParameterFilter<TestParameterFilter>();

                c.OperationFilter<AssignOperationVendorExtensions>();

                c.SchemaFilter<ExamplesSchemaFilter>();

                c.DescribeAllParametersInCamelCase();

                c.GeneratePolymorphicSchemas();

                c.DescribeAllEnumsAsStrings();
                //c.DescribeStringEnumsInCamelCase();

                c.EnableAnnotations();
            });

#if NETCOREAPP3_0
            services.AddOptions<SwaggerGenOptions>()
                    .Configure<IWebHostEnvironment>((options, env) =>
                    {
                        if (env.IsDevelopment())
                        {
                            var xmlCommentsPath = Path.Combine(AppContext.BaseDirectory, "Basic.xml");
                            options.IncludeXmlComments(xmlCommentsPath, true);
                        }
                    });
#else
            if (_env.IsDevelopment())
            {
                services.Configure<SwaggerGenOptions>(options =>
                {
                    var xmlCommentsPath = Path.Combine(AppContext.BaseDirectory, "Basic.xml");
                    options.IncludeXmlComments(xmlCommentsPath, true);
                });
            }
#endif
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
#if NETCOREAPP2_1
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
#endif
            // Add MVC to the request pipeline.
            app.UseDeveloperExceptionPage();
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
                });
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = ""; // serve the UI at root
                c.SwaggerEndpoint("swagger/v1/swagger.json", "V1 Docs");
                c.DisplayOperationId();
            });
        }
    }
}
