using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;

#if !NET
using MvcNewtonsoftJsonOptions = Microsoft.AspNetCore.Mvc.MvcJsonOptions;
#endif

namespace Microsoft.Extensions.DependencyInjection;

public static class NewtonsoftServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerGenNewtonsoftSupport(this IServiceCollection services)
    {
        return services.Replace(
            ServiceDescriptor.Transient<ISerializerDataContractResolver>((s) =>
            {
                var serializerSettings = s.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value?.SerializerSettings
                    ?? new();

                return new NewtonsoftDataContractResolver(serializerSettings);
            }));
    }
}
