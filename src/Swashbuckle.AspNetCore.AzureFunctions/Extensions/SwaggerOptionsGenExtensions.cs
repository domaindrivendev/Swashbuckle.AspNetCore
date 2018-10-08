using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.AzureFunctions.Extensions
{
    public static class SwaggerOptionsGenExtensions
    {
        /// <summary>
        /// Includes xml comments, if documentation file exists
        /// </summary>
        /// <param name="options">Swagger generator options</param>
        /// <param name="functionsAssembly">Functions assembly</param>
        public static void TryIncludeFunctionXmlComments(this SwaggerGenOptions options, Assembly functionsAssembly)
        {
            // Search at same location as assembly
            var assemblyXmlPath = $"{functionsAssembly.Location.Substring(0, functionsAssembly.Location.LastIndexOf(".dll", StringComparison.Ordinal))}.xml";
            if (!File.Exists(assemblyXmlPath))
            {
                var fileInfo = new FileInfo(functionsAssembly.Location);
                var xmlFile = $"{fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(".dll", StringComparison.Ordinal))}.xml";
                // Try to find documentation file at application base directory 
                string path = Environment.ExpandEnvironmentVariables($@"%home%\site\wwwroot\{xmlFile}");
                if (File.Exists(path))
                    assemblyXmlPath = path;
                else
                {
                    // Try to find documentation file at parent directory, because Azure Functions assemblies are placed into additional bin folder
                    assemblyXmlPath = Path.Combine(fileInfo.Directory.Parent.FullName, xmlFile);
                    if (!File.Exists(assemblyXmlPath))
                    {
                        assemblyXmlPath = null;
                    }
                }
            }

            // Add XML-Comments if exists
            if (assemblyXmlPath != null)
                options.IncludeXmlComments(assemblyXmlPath);
        }
    }
}