using System;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Description;
using Swashbuckle.Swagger;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Application
{
    public static class ApplicationBuilderExtensions
    {
        public static void AddSwashbuckle(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(CreateSwaggerProvider);
        }

        private static ISwaggerProvider CreateSwaggerProvider(IServiceProvider serviceProvider)
        {
            var apiDescriptionsProvider = serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>();
            var mvcOptions = serviceProvider.GetService<MvcOptions>();

            return new SwaggerGenerator(
                apiDescriptionsProvider,
                GetJsonContractResolverOrDefault(mvcOptions));
        }

        private static IContractResolver GetJsonContractResolverOrDefault(MvcOptions mvcOptions)
        {
            // TODO: If available, pick one from mvcOptions
            return new DefaultContractResolver();
        }
    }
}