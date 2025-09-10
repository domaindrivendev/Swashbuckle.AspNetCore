using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class TestSiteAutofaq(Type startupType, ITestOutputHelper outputHelper)
    : TestSite(startupType, outputHelper)
{
    protected override void Configure(IHostBuilder builder)
    {
        base.Configure(builder);
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    }

    protected override void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((services) => services.AddAutofac());
        base.Configure(builder);
    }
}
