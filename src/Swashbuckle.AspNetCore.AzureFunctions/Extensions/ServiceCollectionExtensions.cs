using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.AzureFunctions.Application;
using Swashbuckle.AspNetCore.AzureFunctions.Providers;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.AzureFunctions.Extensions
{
    /// <summary>
    /// Azure functions swagger extensions for <see cref="IServiceCollection"/> and <see cref="IServiceProvider"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Initialize <see cref="IApiDescriptionGroupCollectionProvider"/> for Azure Functions
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="functionAssembly">Functions assembly</param>
        public static void AddAzureFunctionsApiProvider(this IServiceCollection services, Assembly functionAssembly)
        {
            services.AddOptions();
            services.Configure<AzureFunctionsOptions>(o => o.Assembly = functionAssembly);
            services.AddSingleton<IApiDescriptionGroupCollectionProvider, FunctionApiDescriptionProvider>();
        }

        /// <summary>
        /// Gets swagger json
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="documentName"></param>
        /// <returns></returns>
        public static string GetSwagger(this IServiceProvider serviceProvider, string documentName)
        {
            var requiredService = serviceProvider.GetRequiredService<ISwaggerProvider>();
            var swaggerDocument = requiredService.GetSwagger(documentName);

            using (var streamWriter = new StringWriter())
            {
                var mvcOptionsAccessor = (IOptions<MvcJsonOptions>)serviceProvider.GetService(typeof(IOptions<MvcJsonOptions>));
                var serializer = SwaggerSerializerFactory.Create(mvcOptionsAccessor);

                serializer.Serialize(streamWriter, swaggerDocument);
                var content = streamWriter.ToString();
                return content;
            }
        }
    }
}