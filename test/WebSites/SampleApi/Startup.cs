using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;
using Swashbuckle.Application;
using System.Linq;

namespace SampleApi
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwashbuckle(s =>
            {
                //s.DocsRoute();

                //s.ApiRoutUrl();
                s.SwaggerGenerator(c =>
                {
                    c.ResolveConflictingActions(apiDescriptions =>
                    {
                        var maxParamCount = apiDescriptions.Max(apiDesc => apiDesc.ParameterDescriptions.Count());
                        return apiDescriptions.First(apiDesc => apiDesc.ParameterDescriptions.Count == maxParamCount);
                    });
                });
            });

            // TODO: Encapsulate below as default behavior behind AddSwashbuckle
            services.Configure<MvcOptions>(options =>
            {
                options.ApplicationModelConventions.Add(new ApiExplorerForSwaggerConvention());
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
