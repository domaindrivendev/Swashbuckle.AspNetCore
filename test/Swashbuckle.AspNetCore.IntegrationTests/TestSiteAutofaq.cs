using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class TestSiteAutofaq
{
    private readonly Type _startupType;

    public TestSiteAutofaq(Type startupType)
    {
        _startupType = startupType;
    }

    public TestServer BuildServer()
    {
        var startupAssembly = _startupType.Assembly;
        var applicationName = startupAssembly.GetName().Name;

#pragma warning disable ASPDEPR004, ASPDEPR008
        // TODO Use WebApplicationFactory
        var hostBuilder = new WebHostBuilder()
            .UseEnvironment("Development")
            .ConfigureServices(services => services.AddAutofac())
            .UseSolutionRelativeContentRoot(Path.Combine("test", "WebSites", applicationName), "*.slnx")
            .UseStartup(_startupType);

        return new TestServer(hostBuilder);
#pragma warning restore ASPDEPR004, ASPDEPR008
    }

    public HttpClient BuildClient()
    {
        var server = BuildServer();
        var client = server.CreateClient();

        return client;
    }
}
