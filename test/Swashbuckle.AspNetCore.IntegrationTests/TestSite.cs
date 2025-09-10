using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class TestSite(Type startupType, ITestOutputHelper outputHelper)
{
    private IHost _host;
    private TestServer _server;

    public virtual TestServer BuildServer()
    {
        if (_server is null)
        {
            var builder = new HostBuilder();

            Configure(builder);

            builder.ConfigureWebHost(Configure);

            _host = builder.Build();
            _host.Start();

            _server = _host.GetTestServer();
        }

        return _server;
    }

    public HttpClient BuildClient()
    {
        var server = BuildServer();
        return server.CreateClient();
    }

    protected virtual void Configure(IHostBuilder builder)
    {
        builder.ConfigureServices((services) =>
        {
            services.AddLogging((logging) => logging.ClearProviders().AddXUnit(outputHelper));
            services.AddTransient<IStartupFilter, LocalizationStartupFilter>();
        });
    }

    protected virtual void Configure(IWebHostBuilder builder)
    {
        var applicationName = startupType.Assembly.GetName().Name;

        builder.UseEnvironment("Development")
               .UseSolutionRelativeContentRoot(Path.Combine("test", "WebSites", applicationName), "*.slnx")
               .UseStartup(startupType)
               .UseTestServer();
    }

    private sealed class LocalizationStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return (builder) =>
            {
                builder.UseMiddleware<LocalizationMiddleware>();
                next(builder);
            };
        }
    }

    private sealed class LocalizationMiddleware(RequestDelegate next)
    {
        /// <summary>
        /// Use a culture that uses different number formatting than the invariant culture.
        /// </summary>
        private static readonly CultureInfo French = new("fr-FR");

        public async Task InvokeAsync(HttpContext context)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = French;
                CultureInfo.CurrentUICulture = French;

                await next(context);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }
    }
}
