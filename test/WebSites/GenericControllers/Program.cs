using Microsoft.AspNetCore.Hosting;

#if NETCOREAPP3_0
using Microsoft.Extensions.Hosting;
#else
using System.IO;
#endif

namespace GenericControllers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilderAndRun(args);
        }

        public static void CreateHostBuilderAndRun(string[] args) =>
#if NETCOREAPP3_0
            NC3_CreateHostBuilder(args).Build().Run();
#else
            NC2_CreateHostBuilder().Build().Run();

#endif

#if NETCOREAPP3_0
        public static IHostBuilder NC3_CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
#else
        public static IWebHostBuilder NC2_CreateHostBuilder() =>
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();
#endif
    }
}
