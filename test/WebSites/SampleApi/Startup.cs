using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Swashbuckle.Swagger.Application;

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
                s.RouteTemplate = "docs/{apiVersion}/swagger.json";

                s.RootUrlResolver = (request) => "http://foo/api";

                s.SwaggerGenerator(c =>
                {
                    c.Schemes(new[] { "http", "https" });

                    c.SingleApiVersion("v1", "Swashbuckle Sample API")
                        .Description("A sample API for testing Swashbuckle")
                        .TermsOfService("Some terms ...");

                    c.ResolveConflictingActions(apiDescriptions =>
                    {
                        var maxParamCount = apiDescriptions.Max(apiDesc => apiDesc.ParameterDescriptions.Count());
                        return apiDescriptions.First(apiDesc => apiDesc.ParameterDescriptions.Count == maxParamCount);
                    });
                });

                s.SchemaGenerator(c =>
                {
                    c.DescribeAllEnumsAsStrings();
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
