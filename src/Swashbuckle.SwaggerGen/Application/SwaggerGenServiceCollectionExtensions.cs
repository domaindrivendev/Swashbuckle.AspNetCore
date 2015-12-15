using System;
using Microsoft.AspNet.Mvc;
using Swashbuckle.SwaggerGen.Application;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static void AddSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerProviderBuilder> configure = null)
        {
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            var swaggerProviderBuilder = new SwaggerProviderBuilder();
            if (configure != null) configure(swaggerProviderBuilder);

            services.AddSingleton(swaggerProviderBuilder.Build);
        }
    }
}
