using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger;
using MultipleVersions.Versioning;
using MultipleVersions.Swagger;

namespace MultipleVersions
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            services.AddSwagger();
            services.ConfigureSwaggerDocument(options =>
            {
                options.MultipleApiVersions(
                    new[]
                    {
                        new Info { Version = "v1", Title = "API V1" },
                        new Info { Version = "v2", Title = "API V2" }
                    },
                    ResolveVersionSupportByVersionsConstraint
                );

                options.DocumentFilter<SetVersionInPaths>();
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
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

            app.UseSwagger();
            app.UseSwaggerUi();
        }

        private static bool ResolveVersionSupportByVersionsConstraint(ApiDescription apiDesc, string version)
        {
            var versionAttribute = apiDesc.ActionDescriptor.ActionConstraints.OfType<VersionsAttribute>()
                .FirstOrDefault();
            if (versionAttribute == null) return true;

            return versionAttribute.AcceptedVersions.Contains(version);
        }
    }
}
