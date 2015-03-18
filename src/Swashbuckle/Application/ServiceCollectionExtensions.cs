using System;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Description;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Application
{
    public static class ApplicationBuilderExtensions
    {
        public static void AddSwashbuckle(
            this IServiceCollection serviceCollection,
            Action<SwashbuckleOptions> configure = null)
        {
            var options = new SwashbuckleOptions();
            if (configure != null) configure(options);
            
            serviceCollection.AddTransient(serviceProvider =>
                CreateSwaggerProvider(serviceProvider, options.SwaggerGeneratorOptions.Build()));
        }

        private static ISwaggerProvider CreateSwaggerProvider(
            IServiceProvider serviceProvider,
            SwaggerGeneratorOptions swaggerGeneratorOptions)
        {
            var apiDescriptionsProvider = serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>();
            var mvcOptions = serviceProvider.GetService<MvcOptions>();

            return new SwaggerGenerator(
                apiDescriptionsProvider,
                GetJsonContractResolverOrDefault(mvcOptions),
                swaggerGeneratorOptions);
        }

        private static IContractResolver GetJsonContractResolverOrDefault(MvcOptions mvcOptions)
        {
            // TODO: If available, pick one from mvcOptions
            return new DefaultContractResolver();
        }
    }
}