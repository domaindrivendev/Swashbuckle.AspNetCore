using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Swashbuckle.IntegrationTests
{
    public class TestSite
    {
        private readonly Type _startupType;

        public TestSite(Type startupType)
        {
            _startupType = startupType;
        }

        public TestServer BuildServer()
        {
            var siteContentRoot = GetApplicationPath(Path.Combine("..", "..", "..", "..", "WebSites"));

            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseContentRoot(siteContentRoot)
                .ConfigureServices(InitializeServices)
                .UseStartup(_startupType);

            return new TestServer(builder);
        }

        public HttpClient BuildClient()
        {
            var server = BuildServer();
            var client = server.CreateClient();
            client.BaseAddress = new Uri("http://localhost");

            return client;
        }

        private void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = _startupType.GetTypeInfo().Assembly;

            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));

            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());

            services.Configure<MvcOptions>(
                c => c.Conventions.Add(new TestAppActionModel(_startupType.GetTypeInfo().Assembly)));

            services.AddSingleton(manager);
        }

        private string GetApplicationPath(string relativePath)
        {
            var startupAssembly = _startupType.GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            return Path.GetFullPath(Path.Combine(applicationBasePath, relativePath, applicationName));
        }
    }
}