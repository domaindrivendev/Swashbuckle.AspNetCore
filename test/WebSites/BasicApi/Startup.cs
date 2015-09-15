using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using BasicApi.Swagger;
using Swashbuckle.Swagger;

namespace BasicApi
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
                options.SingleApiVersion(new Info
                    {
                        Version = "v2",
                        Title = "Swashbuckle Sample API",
                        Description = "A sample API for testing Swashbuckle",
                        TermsOfService = "Some terms ..."
                    });

                options.OperationFilter<AssignOperationVendorExtensions>();
            });
            services.ConfigureSwaggerSchema(options =>
            {
                options.DescribeAllEnumsAsStrings = true;
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();

            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
        }
    }
}
