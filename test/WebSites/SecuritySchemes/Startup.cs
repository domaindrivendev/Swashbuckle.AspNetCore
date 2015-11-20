using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger;
using SecuritySchemes.Swagger;

namespace SecuritySchemes
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
                options.SecurityDefinitions.Add("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = "http://petstore.swagger.io/api/oauth/dialog",
                    Scopes = new Dictionary<string, string>
                        {
                            { "read", "read access" },
                            { "write", "write access" }
                        }
                });

                options.OperationFilter<AssignSecurityRequirements>();
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

            // TOOD: Figure out oauth middleware to validate token
            //app.UseOAuthBearerAuthentication(opts =>
            //{
            //});
        }
    }
}
