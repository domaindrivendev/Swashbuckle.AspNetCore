using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Swashbuckle.AspNetCore.Cli
{
    // Represents an application that uses Microsoft.Extensions.Hosting and supports
    // the various entry point flavors. The final model *does not* have an explicit CreateHost entry point and thus inverts the typical flow where the
    // execute Main and we wait for events to fire in order to access the appropriate state.
    // This is what allows top level statements to work, but getting the IServiceProvider is slightly more complex.
    internal class HostingApplication
    {
        internal static IServiceProvider GetServiceProvider(Assembly assembly)
        {
            // We're disabling the default server and the console host lifetime. This will disable:
            // 1. Listening on ports
            // 2. Logging to the console from the default host.
            // This is essentially what the test server does in order to get access to the application's
            // IServicerProvider *and* middleware pipeline.
            void ConfigureHostBuilder(object hostBuilder)
            {
                ((IHostBuilder)hostBuilder).ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IServer, NoopServer>();
                    services.AddSingleton<IHostLifetime, NoopHostLifetime>();

                    for (var i = services.Count - 1; i >= 0; i--)
                    {
                        // exclude all implementations of IHostedService
                        // except Microsoft.AspNetCore.Hosting.GenericWebHostService because that one will build/configure
                        // the WebApplication/Middleware pipeline in the case of the GenericWebHostBuilder.
                        var registration = services[i];
                        if (registration.ServiceType == typeof(IHostedService)
                            && registration.ImplementationType is not { FullName: "Microsoft.AspNetCore.Hosting.GenericWebHostService" })
                        {
                            services.RemoveAt(i);
                        }
                    }
                });
            }

            var waitForStartTcs = new TaskCompletionSource<object>();

            void OnEntryPointExit(Exception exception)
            {
                // If the entry point exited, we'll try to complete the wait
                if (exception != null)
                {
                    waitForStartTcs.TrySetException(exception);
                }
                else
                {
                    waitForStartTcs.TrySetResult(null);
                }
            }

            // If all of the existing techniques fail, then try to resolve the ResolveHostFactory
            var factory = HostFactoryResolver.ResolveHostFactory(assembly,
                                                                 stopApplication: false,
                                                                 configureHostBuilder: ConfigureHostBuilder,
                                                                 entrypointCompleted: OnEntryPointExit);

            // We're unable to resolve the factory. This could mean the application wasn't referencing the right
            // version of hosting.
            if (factory == null)
            {
                return null;
            }

            try
            {
                // Get the IServiceProvider from the host
                var assemblyName = assembly.GetName()?.FullName ?? string.Empty;
                // We set the application name in the hosting environment to the startup assembly
                // to avoid falling back to the entry assembly (dotnet-swagger) when configuring our
                // application.
                var services = ((IHost)factory([$"--{HostDefaults.ApplicationKey}={assemblyName}"])).Services;

                // Wait for the application to start so that we know it's fully configured. This is important because
                // we need the middleware pipeline to be configured before we access the ISwaggerProvider in
                // in the IServiceProvider
                var applicationLifetime = services.GetRequiredService<IHostApplicationLifetime>();

                using var registration = applicationLifetime.ApplicationStarted.Register(() => waitForStartTcs.TrySetResult(null));
                waitForStartTcs.Task.Wait();

                return services;
            }
            catch (InvalidOperationException)
            {
                // We're unable to resolve the host, swallow the exception and return null
            }

            return null;
        }

        private class NoopHostLifetime : IHostLifetime
        {
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }

        private class NoopServer : IServer
        {
            public IFeatureCollection Features { get; } = new FeatureCollection();
            public void Dispose() { }
            public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) => Task.CompletedTask;
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }
    }
}
