using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            services.Configure<MvcOptions>(c =>
                c.Conventions.Add(new SwaggerApplicationConvention()));

            services.Configure(setupAction ?? (opts => { }));

            services.AddTransient(CreateSwaggerProvider);

            return services;
        }

        public static void ConfigureSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction = null)
        {
            services.Configure(setupAction ?? (opts => { }));
        }

        private static ISwaggerProvider CreateSwaggerProvider(IServiceProvider serviceProvider)
        {
            var swaggerGenOptions = serviceProvider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
            return swaggerGenOptions.CreateSwaggerProvider(serviceProvider);
        }
    }
}
