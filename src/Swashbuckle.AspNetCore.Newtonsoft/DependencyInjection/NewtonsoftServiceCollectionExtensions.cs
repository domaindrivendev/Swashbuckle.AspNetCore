using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Newtonsoft;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewtonsoftServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenNewtonsoftSupport(this IServiceCollection services)
        {
            return services.AddTransient<IApiModelResolver>(s =>
            {
                var jsonSerializerSettings = s.GetJsonSerializerSettings() ?? new JsonSerializerSettings();
                var options = s.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value;

                return new NewtonsoftApiModelResolver(jsonSerializerSettings, options);
            });
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(this IServiceProvider serviceProvider)
        {
#if NETCOREAPP3_0
            return serviceProvider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value?.SerializerSettings;
#else
            return serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value?.SerializerSettings;
#endif
        }
    }
}
