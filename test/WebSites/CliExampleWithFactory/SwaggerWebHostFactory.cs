using Microsoft.AspNetCore.Hosting;
using Swashbuckle.AspNetCore.Cli;

namespace CliExampleWithFactory
{
    public class MySwaggerWebHostFactory : ISwaggerWebHostFactory
    {
        public IWebHost BuildWebHost()
        {
            return Program.BuildWebHost(new string[0]);
        }
    }
}
