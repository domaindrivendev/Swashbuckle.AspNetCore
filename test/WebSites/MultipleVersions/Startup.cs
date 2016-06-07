using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.Swagger.Model;
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
            // You will also need to add the Microsoft.AspNetCore.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            services.AddSwaggerGen(c =>
            {
                c.MultipleApiVersions(
                    new[]
                    {
                        new Info { Version = "v1", Title = "API V1" },
                        new Info { Version = "v2", Title = "API V2" }
                    },
                    ResolveVersionSupportByVersionsConstraint
                );

                c.DocumentFilter<SetVersionInPaths>();
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();


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
