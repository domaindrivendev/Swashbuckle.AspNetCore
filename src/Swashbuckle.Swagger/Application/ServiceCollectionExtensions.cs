using System;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Description;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwashbuckle(
            this IServiceCollection serviceCollection,
            Action<SwaggerOptions> configure)
        {
            var options = new SwaggerOptions();
            if (configure != null) configure(options);

            serviceCollection.Configure<MvcOptions>(c =>
                c.ApplicationModelConventions.Add(new SwaggerApplicationConvention(options.RouteTemplate)));

            serviceCollection.AddTransient(services => options.RootUrlResolver);
            
            serviceCollection.AddTransient(services =>
                CreateSchemaRegistry(services, options.SchemaGeneratorOptions));

            serviceCollection.AddTransient(services =>
                CreateSwaggerProvider(services, options.SwaggerGeneratorOptions));
        }

        private static ISchemaRegistry CreateSchemaRegistry(
            IServiceProvider serviceProvider,
            SchemaGeneratorOptions options)
        {
            var jsonContractResolver = GetJsonContractResolver(serviceProvider);
            return new SchemaGenerator(jsonContractResolver, options);
        }

        private static ISwaggerProvider CreateSwaggerProvider(
            IServiceProvider services,
            SwaggerGeneratorOptions options)
        {
            return new SwaggerGenerator(
                services.GetService<IApiDescriptionGroupCollectionProvider>(),
                () => services.GetService<ISchemaRegistry>(),
                options);
        }

        private static IContractResolver GetJsonContractResolver(IServiceProvider serviceProvider)
        {
            var mvcOptions = serviceProvider.GetService<MvcOptions>();
            return new DefaultContractResolver();
        }
    }
}