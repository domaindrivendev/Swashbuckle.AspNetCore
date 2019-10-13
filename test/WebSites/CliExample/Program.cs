using Microsoft.AspNetCore.Hosting;

#if NETCOREAPP3_0
using Microsoft.Extensions.Hosting;
#else
using Microsoft.AspNetCore;
#endif

namespace CliExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilderAndRun(args);
        }

        public static void CreateHostBuilderAndRun(string[] args) =>
#if NETCOREAPP3_0
            CreateHostBuilder(args).Build().Run();
#else
            BuildWebHost(args).Run();

#endif

#if NETCOREAPP3_0
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
#else
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
#endif
    }
}
