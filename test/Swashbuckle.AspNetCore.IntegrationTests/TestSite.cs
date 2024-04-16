using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Swashbuckle.AspNetCore.IntegrationTests
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
            var startupAssembly = _startupType.GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;

            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseSolutionRelativeContentRoot(Path.Combine("test", "WebSites", applicationName))
                .UseStartup(_startupType);

            return new TestServer(builder);
        }

        public HttpClient BuildClient()
        {
            var server = BuildServer();
            var client = server.CreateClient();

            return client;
        }
    }
}
