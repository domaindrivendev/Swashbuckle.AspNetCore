using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using CliStartupAttributeExample.Startups;

namespace CliStartupAttributeExample
{
    public class Program
    {
        private const string _publicUrl = "http://0.0.0.0:8080";
        private const string _privateUrl = "http://0.0.0.0:8081";

        public static void Main(string[] args)
        {
            try
            {
                Task publicApi = BuildWebHost<PublicStartup>(args, _publicUrl)
                    .RunAsync();
                Task privateApi = BuildWebHost<PrivateStartup>(args, _privateUrl)
                    .RunAsync();

                Task.WaitAll(publicApi, privateApi);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        private static IWebHost BuildWebHost<TBaseStartup>(string[] args, string url)
            where TBaseStartup : BaseStartup
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseStartup<TBaseStartup>()
                .UseUrls(url)
                .Build();
        }
    }
}
