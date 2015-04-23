using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Description;
using Microsoft.Framework.DependencyInjection;
using Swashbuckle.Application;

namespace BasicApi
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwagger(s =>
            {
                s.SwaggerGenerator(c =>
                {
                    c.Schemes(new[] { "http", "https" });

                    c.SingleApiVersion("v1", "Swashbuckle Sample API")
                        .Description("A sample API for testing Swashbuckle")
                        .TermsOfService("Some terms ...");

                    c.ResolveConflictingActions(MaxParametersWins);
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

            app.UseSwagger("docs/{apiVersion}/swagger.json");
            app.UseSwaggerUi("docs/ui/{*assetPath}");
        }

        private ApiDescription MaxParametersWins(IEnumerable<ApiDescription> apiDescriptions)
        {
            var maxParamCount = apiDescriptions.Max(apiDesc => apiDesc.ParameterDescriptions.Count());
            return apiDescriptions.First(apiDesc => apiDesc.ParameterDescriptions.Count == maxParamCount);
        }
    }
}
