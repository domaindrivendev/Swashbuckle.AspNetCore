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

namespace Basic
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnv;

        public Startup(IHostingEnvironment hostingEnv)
        {
            _hostingEnv = hostingEnv;
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

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

                c.DescribeAllEnumsAsStrings();

                c.SchemaFilter<ExamplesSchemaFilter>();

                //c.DescribeAllParametersInCamelCase();

                c.GeneratePolymorphicSchemas();

                c.EnableAnnotations();
            });

            if (_hostingEnv.IsDevelopment())
            {
                services.ConfigureSwaggerGen(c =>
                {
                    var xmlCommentsPath = Path.Combine(System.AppContext.BaseDirectory, "Basic.xml");
                    c.IncludeXmlComments(xmlCommentsPath, true);
                });
            }
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
                c.DisplayOperationId();
            });
        }
    }
}
