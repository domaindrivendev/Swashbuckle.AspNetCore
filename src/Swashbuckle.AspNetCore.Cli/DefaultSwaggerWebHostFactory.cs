using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Swashbuckle.AspNetCore.Cli
{
    public class DefaultSwaggerWebHostFactory : ISwaggerWebHostFactory
    {
        private readonly Assembly _startupAssembly;

        public DefaultSwaggerWebHostFactory(Assembly startupAssembly)
        {
            _startupAssembly = startupAssembly;
        }

        public IWebHost BuildWebHost()
        {
            return WebHost.CreateDefaultBuilder()
               .UseStartup(_startupAssembly.FullName)
               .Build();
        }
    }
}
