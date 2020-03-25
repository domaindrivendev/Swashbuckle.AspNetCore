using Microsoft.AspNetCore.Hosting;

namespace CliExampleWithFactory
{
    public class SwaggerWebHostFactory
    {
        public static IWebHost CreateWebHost()
        {
            return Program.BuildWebHost(new string[0]);
        }
    }
}
