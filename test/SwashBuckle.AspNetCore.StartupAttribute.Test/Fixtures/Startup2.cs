using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using Swashbuckle.AspNetCore.StartupAttribute;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test
{
    [StartupClass("TestStartup2")]
    public class Startup2 : BaseStartup
    {
        public Startup2(IConfiguration configuration) : base(configuration) { }

        public override void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) { }

        public override void ConfigureMvcServices(IServiceCollection services) { }
    }
}
