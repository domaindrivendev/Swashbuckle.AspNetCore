using System;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Serialization;
using Swashbuckle.SwaggerGen.Generator;
using Basic.Swagger;

namespace Basic
{
    public class Startup
    {
        private IApplicationEnvironment _appEnv;

        public Startup(IApplicationEnvironment appEnv)
        {
            _appEnv = appEnv;
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            services.AddSwaggerGen(c =>
            {
                c.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Swashbuckle Sample API",
                    Description = "A sample API for testing Swashbuckle",
                    TermsOfService = "Some terms ..."
                });

                c.DescribeAllEnumsAsStrings();

                c.OperationFilter<AssignOperationVendorExtensions>();

                c.IncludeXmlComments(string.Format(@"{0}\artifacts\bin\Basic\{1}\{2}{3}\Basic.xml",
                    GetSolutionBasePath(),
                    _appEnv.Configuration,
                    _appEnv.RuntimeFramework.Identifier,
                    _appEnv.RuntimeFramework.Version.ToString().Replace(".", string.Empty)));
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Add the platform handler to the request pipeline.
            app.UseIISPlatformHandler();

            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseDeveloperExceptionPage();
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

            app.UseSwaggerGen();
            app.UseSwaggerUi();
        }

        private string GetSolutionBasePath()
        {
            var dir = Directory.CreateDirectory(_appEnv.ApplicationBasePath);
            while (dir.Parent != null)
            {
                if (dir.GetFiles("global.json").Any())
                    return dir.FullName;

                dir = dir.Parent;
            }
            throw new InvalidOperationException("Failed to detect solution base path - global.json not found.");
        }
    }
}
