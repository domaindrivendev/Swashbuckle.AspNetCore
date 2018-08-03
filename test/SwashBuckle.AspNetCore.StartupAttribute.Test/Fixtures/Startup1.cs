using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

using Swashbuckle.AspNetCore.StartupAttribute;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test
{
    [StartupClass("TestStartup1")]
    public class Startup1 : BaseStartup
    {
        public Startup1(IConfiguration configuration) : base(configuration) { }

        public override void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) { }

        public override void ConfigureMvcServices(IServiceCollection services) { }
    }
}
