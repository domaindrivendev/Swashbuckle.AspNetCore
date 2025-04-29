using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class TestSite(Type startupType, ITestOutputHelper outputHelper)
{
    public TestServer BuildServer()
    {
        var startupAssembly = startupType.Assembly;
        var applicationName = startupAssembly.GetName().Name;

        var builder = new WebHostBuilder()
            .UseEnvironment("Development")
            .UseSolutionRelativeContentRoot(Path.Combine("test", "WebSites", applicationName))
            .UseStartup(startupType);

        builder.ConfigureTestServices(
            (services) => services.AddLogging(
                (logging) => logging.ClearProviders().AddXUnit(outputHelper)));

        return new TestServer(builder);
    }

    public HttpClient BuildClient()
    {
        var server = BuildServer();
        return server.CreateClient();
    }
}
