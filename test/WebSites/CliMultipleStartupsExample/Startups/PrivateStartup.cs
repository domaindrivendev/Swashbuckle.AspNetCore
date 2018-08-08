using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.SwaggerStartupAttr;

namespace CliMultipleStartupsExample.Startups
{
    [SwaggerStartup(
    openApiFileName: "PrivateAPI",
    ClientClassName = "PrivateClient",
    ClientNamespace = "Private.Client")]
    public class PrivateStartup : BaseStartup
    {
        private static readonly ApiVersion ApiVersion = new ApiVersion(1, 0);

        protected override ApiInfo ApiInfo => new ApiInfo("Private test API", "Private test API description", ApiVersion);

        public PrivateStartup(IConfiguration configuration) : base(configuration)
        {
            _controllerFilter = new TypedControllerFeatureProvider<PrivateApiController>();
        }

        public override void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) { }

        public override void ConfigureMvcServices(IServiceCollection services) { }
    }
}
