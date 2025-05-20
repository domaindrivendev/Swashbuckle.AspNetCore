using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class TestApplication<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var startupAssembly = typeof(TEntryPoint).Assembly;
        var applicationName = startupAssembly.GetName().Name;

        builder.UseEnvironment("Development")
               .UseSolutionRelativeContentRoot(Path.Combine("test", "WebSites", applicationName), "*.slnx");
    }
}
