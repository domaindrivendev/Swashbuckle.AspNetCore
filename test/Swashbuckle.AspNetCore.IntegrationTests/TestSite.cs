using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            .UseSolutionRelativeContentRoot(Path.Combine("test", "WebSites", applicationName), "*.slnx")
            .UseStartup(startupType);

        builder.ConfigureTestServices((services) =>
        {
            services.AddLogging((logging) => logging.ClearProviders().AddXUnit(outputHelper));
            services.AddTransient<IStartupFilter, LocalizationStartupFilter>();
        });

        return new(builder);
    }

    public HttpClient BuildClient()
    {
        var server = BuildServer();
        return server.CreateClient();
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
