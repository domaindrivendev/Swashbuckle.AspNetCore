using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerStartupAttr;

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
                c.Option("--basepath", "a specific basePath to inlcude in the Swagger output");
                c.Option("--format", "overrides the format of the Swagger output, can be Indented or None");
                c.Option("--multiplestartups", "seeks startup classes marked with SwaggerStartup attribute, creating a file for each marked startup class. (--output option must be a directory)");
                c.OnRun((namedArgs) =>
                {
                    var depsFile = namedArgs["startupassembly"].Replace(".dll", ".deps.json");
                    var runtimeConfig = namedArgs["startupassembly"].Replace(".dll", ".runtimeconfig.json");

                    var subProcess = Process.Start("dotnet", string.Format(
                        "exec --depsfile {0} --runtimeconfig {1} {2} _{3}", // note the underscore
                        EscapePath(depsFile),
                        EscapePath(runtimeConfig),
                        typeof(Program).GetTypeInfo().Assembly.Location,
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
                c.Option("--format", "");
                c.Option("--multiplestartups", "");
                c.OnRun((namedArgs) =>
                {
                    var startupAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(
                        Path.Combine(Directory.GetCurrentDirectory(), namedArgs["startupassembly"]));

                    Console.WriteLine($"Startup Assembly Name: {startupAssembly.FullName}");

                    if (!namedArgs.ContainsKey("--multiplestartups"))
                    {
                        // 1) Configure host with provided startupassembly
                        IWebHost host = WebHost.CreateDefaultBuilder()
                            .UseStartup(startupAssembly.FullName)
                            .Build();

                        // 2) Retrieve Swagger via configured provider
                        SwaggerDocument swaggerDocument = RetrieveSwagger(namedArgs, host);

                        // 3) Serialize Swagger to specified output location or stdout
                        string outputPath = namedArgs.ContainsKey("--output")
                            ? Path.Combine(Directory.GetCurrentDirectory(), namedArgs["--output"])
                            : null;
                        SerializeSwagger(namedArgs, host, swaggerDocument, outputPath);
                    }
                    else
                    {
                        IEnumerable<Type> startupList = startupAssembly.GetClassesWithSwaggerStartupAttribute();
                        if (!startupList.Any())
                        {
                            throw new SwaggerStartupAttributeException("No classes marked with StartupClass attribute have been found");
                        }

                        Console.WriteLine("Startup classes detected (marked by StartupClass attribute):");

                        foreach (Type startupClass in startupList)
                        {
                            if (!namedArgs.ContainsKey("--output"))
                            {
                                throw new SwaggerStartupAttributeException("Missing --output argument");
                            }

                            Console.WriteLine("* " + startupClass.FullName);

                            // 1) Configure host with provided startupassembly
                            IWebHost host = WebHost.CreateDefaultBuilder()
                                .UseStartup(startupClass)
                                .Build();

                            // 2) Retrieve Swagger via configured provider
                            SwaggerDocument swaggerDocument = RetrieveSwagger(namedArgs, host);

                            // 3) Serialize Swagger to specified output location or stdout
                            string fileName = string.Concat(
                                namedArgs["--output"],
                                Path.DirectorySeparatorChar,
                                startupClass.GetSwaggerStartupAttribute().OpenApiFileName);

                            string outputPath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                fileName);

                            SerializeSwagger(namedArgs, host, swaggerDocument, outputPath);
                        }
                    }

                    return 0;
                });
            });

            return runner.Run(args);
        }

        /// <summary>
        /// Retrieves Swagger via configured provider
        /// </summary>
        /// <param name="namedArgs"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        protected static SwaggerDocument RetrieveSwagger(IDictionary<string, string> namedArgs, IWebHost host)
        {
            var swaggerProvider = host.Services.GetRequiredService<ISwaggerProvider>();
            var swagger = swaggerProvider.GetSwagger(
                namedArgs["swaggerdoc"],
                namedArgs.ContainsKey("--host") ? namedArgs["--host"] : null,
                namedArgs.ContainsKey("--basepath") ? namedArgs["--basepath"] : null,
                null);
            return swagger;
        }

        /// <summary>
        /// Serializes Swagger to specified output location or stdout
        /// </summary>
        /// <param name="namedArgs"></param>
        /// <param name="host"></param>
        /// <param name="swagger"></param>
        /// <param name="outputPath"></param>
        protected static void SerializeSwagger(IDictionary<string, string> namedArgs, IWebHost host, SwaggerDocument swagger, string outputPath)
        {
            using (var streamWriter = (outputPath != null ? File.CreateText(outputPath) : Console.Out))
            {
                var mvcOptionsAccessor = (IOptions<MvcJsonOptions>)host.Services.GetService(typeof(IOptions<MvcJsonOptions>));

                if (namedArgs.ContainsKey("--format"))
                {
                    // TODO: Should this handle case where mvcJsonOptions.Value == null?
                    mvcOptionsAccessor.Value.SerializerSettings.Formatting = ParseEnum<Newtonsoft.Json.Formatting>(namedArgs["--format"]);
                }

                var serializer = SwaggerSerializerFactory.Create(mvcOptionsAccessor);

                serializer.Serialize(streamWriter, swagger);

                if (!string.IsNullOrWhiteSpace(outputPath))
                {
                    Console.WriteLine($"Swagger JSON succesfully written to {outputPath}");
                }
            }
        }

        /// <summary>
        /// Parses a given option value into a specified enumeration value
        /// </summary>
        /// <typeparam name="T">enum</typeparam>
        /// <param name="optionValue">Expects the string representation of a valid Enumeration value,
        /// anything else defaults to the Enumeration's default value</param>
        protected static T ParseEnum<T>(string optionValue) where T : struct, IConvertible
        {
            var isParsed = Enum.TryParse(optionValue, true, out T parsed);

            return isParsed ? parsed : default(T);
        }

        /// <summary>
        /// Escape a given path value
        /// </summary>
        /// <param name="path">The path which should be escaped</param>
        private static string EscapePath(string path)
        {
            if (path.Contains(" "))
            {
                return "\"" + path + "\"";
            }

            return path;
        }
    }
}
