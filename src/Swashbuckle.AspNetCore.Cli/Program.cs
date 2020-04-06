using System;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Runtime.Loader;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;

namespace Swashbuckle.AspNetCore.Cli
{
    public class Program
    {
        static int Main(string[] args)
        {
            // Helper to simplify command line parsing etc.
            var runner = new CommandRunner("dotnet swagger", "Swashbuckle (Swagger) Command Line Tools", Console.Out);

            // NOTE: The "dotnet swagger tofile" command does not serve the request directly. Instead, it invokes a corresponding
            // command (called _tofile) via "dotnet exec" so that the runtime configuration (*.runtimeconfig & *.deps.json) of the
            // provided startupassembly can be used instead of the tool's. This is neccessary to successfully load the
            // startupassembly and it's transitive dependencies. See https://github.com/dotnet/coreclr/issues/13277 for more.

            // > dotnet swagger tofile ...
            runner.SubCommand("tofile", "retrieves Swagger from a startup assembly, and writes to file ", c =>
            {
                c.Argument("startupassembly", "relative path to the application's startup assembly");
                c.Argument("swaggerdoc", "name of the swagger doc you want to retrieve, as configured in your startup class");
                c.Option("--output", "relative path where the Swagger will be output, defaults to stdout");
                c.Option("--host", "a specific host to include in the Swagger output");
                c.Option("--basepath", "a specific basePath to include in the Swagger output");
                c.Option("--serializeasv2", "output Swagger in the V2 format rather than V3", true);
                c.Option("--yaml", "exports swagger in a yaml format", true);
                c.OnRun((namedArgs) =>
                {
                    var depsFile = namedArgs["startupassembly"].Replace(".dll", ".deps.json");
                    var runtimeConfig = namedArgs["startupassembly"].Replace(".dll", ".runtimeconfig.json");

                    var subProcess = Process.Start("dotnet", string.Format(
                        "exec --depsfile {0} --runtimeconfig {1} {2} _{3}", // note the underscore
                        EscapePath(depsFile),
                        EscapePath(runtimeConfig),
                        EscapePath(typeof(Program).GetTypeInfo().Assembly.Location),
                        string.Join(" ", args)
                    ));

                    subProcess.WaitForExit();
                    return subProcess.ExitCode;
                });
            });

            // > dotnet swagger _tofile ... (* should only be invoked via "dotnet exec")
            runner.SubCommand("_tofile", "", c =>
            {
                c.Argument("startupassembly", "");
                c.Argument("swaggerdoc", "");
                c.Option("--output", "");
                c.Option("--host", "");
                c.Option("--basepath", "");
                c.Option("--serializeasv2", "", true);
                c.Option("--yaml", "", true);
                c.OnRun((namedArgs) =>
                {
                    // 1) Configure host with provided startupassembly
                    var startupAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(
                        Path.Combine(Directory.GetCurrentDirectory(), namedArgs["startupassembly"]));

                    // 2) Build a web host to serve the request
                    var serviceProvider = GetServiceProvider(startupAssembly);

                    // 3) Retrieve Swagger via configured provider
                    var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();
                    var swagger = swaggerProvider.GetSwagger(
                        namedArgs["swaggerdoc"],
                        namedArgs.ContainsKey("--host") ? namedArgs["--host"] : null,
                        namedArgs.ContainsKey("--basepath") ? namedArgs["--basepath"] : null);

                    // 4) Serialize to specified output location or stdout
                    var outputPath = namedArgs.ContainsKey("--output")
                        ? Path.Combine(Directory.GetCurrentDirectory(), namedArgs["--output"])
                        : null;

                    using (var streamWriter = (outputPath != null ? File.CreateText(outputPath) : Console.Out))
                    {
                        IOpenApiWriter writer;
                        if (namedArgs.ContainsKey("--yaml"))
                            writer = new OpenApiYamlWriter(streamWriter);
                        else
                            writer = new OpenApiJsonWriter(streamWriter);

                        if (namedArgs.ContainsKey("--serializeasv2"))
                            swagger.SerializeAsV2(writer);
                        else
                            swagger.SerializeAsV3(writer);

                        if (outputPath != null)
                            Console.WriteLine($"Swagger JSON/YAML succesfully written to {outputPath}");
                    }

                    return 0;
                });
            });

            return runner.Run(args);
        }

        private static string EscapePath(string path)
        {
            return path.Contains(" ")
                ? "\"" + path + "\""
                : path;
        }

        private static IServiceProvider GetServiceProvider(Assembly startupAssembly)
        {

            var host = TryBuildingHost(startupAssembly);
            if (host != null)
            {
                return host.Services;
            }

            var webHost = BuildWebHost(startupAssembly);
            return webHost.Services;
        }

        private static IWebHost BuildWebHost(Assembly startupAssembly)
        {
            // Scan the startup assembly for any types that match the naming convention
            var factoryTypes = startupAssembly.DefinedTypes
                .Where(t => t.Name == "SwaggerWebHostFactory")
                .ToList();

            if (!factoryTypes.Any())
            {
                return WebHost.CreateDefaultBuilder()
                   .UseStartup(startupAssembly.FullName)
                   .Build();
            }

            if (factoryTypes.Count() > 1)
                throw new InvalidOperationException("Multiple SwaggerWebHostFactory classes detected");

            var factoryMethod = factoryTypes
                .Single()
                .GetMethod("CreateWebHost", BindingFlags.Public | BindingFlags.Static);

            if (factoryMethod == null || factoryMethod.ReturnType != typeof(IWebHost))
                throw new InvalidOperationException(
                    "SwaggerWebHostFactory class detected but does not contain a public static method " +
                    "called CreateWebHost with return type IWebHost");

            return (IWebHost)factoryMethod.Invoke(null, null);
        }

        private static IHost TryBuildingHost(Assembly startupAssembly)
        {
            // Scan the startup assembly for any types that match the naming convention
            var factoryTypes = startupAssembly.DefinedTypes
                .Where(t => t.Name == "SwaggerHostFactory")
                .ToList();

            if (!factoryTypes.Any())
            {
                return null;
            }

            if (factoryTypes.Count() > 1)
                throw new InvalidOperationException("Multiple SwaggerHostFactory classes detected");

            var factoryMethod = factoryTypes
                .Single()
                .GetMethod("CreateHost", BindingFlags.Public | BindingFlags.Static);

            if (factoryMethod == null || factoryMethod.ReturnType != typeof(IHost))
                throw new InvalidOperationException(
                    "SwaggerHostFactory class detected but does not contain a public static method " +
                    "called CreateHost with return type IHost");

            return (IHost)factoryMethod.Invoke(null, null);
        }
    }
}
