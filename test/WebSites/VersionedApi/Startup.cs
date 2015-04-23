using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Description;
using Microsoft.Framework.DependencyInjection;
using Swashbuckle.Application;
using VersionedApi.Versioning;
using VersionedApi.Swagger;

namespace VersionedApi
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwagger(config =>
            {
                config.SwaggerGenerator(c =>
                {
                    c.MultipleApiVersions(
                        ResolveVersionSupportByVersionsConstraint,
                        versions =>
                        {
                            versions.Version("v1", "API V1");
                            versions.Version("v2", "API V2");
                        });

                    c.DocumentFilter<AddVersionToBasePath>();
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();

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
