using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Swashbuckle.AspNetCore.SwaggerStartupAttr;

namespace SwashBuckle.AspNetCore.SwaggerStartupAttr.Test
{
    [SwaggerStartup(
        openApiFileName: "TestStartup2",
        ClientClassName = "Client2",
        ClientNamespace = "TestNamespace")]
    public class Startup2 : BaseStartup
    {
        public Startup2(IConfiguration configuration) : base(configuration) { }

        public override void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) { }

        public override void ConfigureMvcServices(IServiceCollection services) { }
    }
}
