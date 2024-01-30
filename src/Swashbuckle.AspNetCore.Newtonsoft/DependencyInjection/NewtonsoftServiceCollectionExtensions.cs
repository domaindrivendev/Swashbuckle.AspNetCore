using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Newtonsoft;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewtonsoftServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenNewtonsoftSupport(this IServiceCollection services)
        {
            return services.Replace(
                ServiceDescriptor.Transient<ISerializerDataContractResolver>((s) =>
                {
                    var serializerSettings = s.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value?.SerializerSettings
                        ?? new JsonSerializerSettings();

                    return new NewtonsoftDataContractResolver(serializerSettings);
                }));
        }
    }
}
