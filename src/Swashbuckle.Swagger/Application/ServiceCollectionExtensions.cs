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
        public static void AddSwagger(
            this IServiceCollection serviceCollection,
            Action<SwaggerOptionsBuilder> configure = null)
        {
            serviceCollection.Configure<MvcOptions>(c =>
                c.ApplicationModelConventions.Add(new SwaggerApplicationConvention()));

            var optionsBuilder = new SwaggerOptionsBuilder();
            if (configure != null) configure(optionsBuilder);

            serviceCollection.AddTransient(services => optionsBuilder.RootUrlResolver);
            
            serviceCollection.AddTransient(services =>
                CreateSchemaRegistry(services, optionsBuilder.SchemaGeneratorOptionsBuilder.Build()));

            serviceCollection.AddTransient(services =>
                CreateSwaggerProvider(services, optionsBuilder.SwaggerGeneratorOptionsBuilder.Build()));
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