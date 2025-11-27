using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A class containing extension methods for setting up Newtonsoft.Json support. This class cannot be inherited.
/// </summary>
public static class NewtonsoftServiceCollectionExtensions
{
    /// <summary>
    /// Add support for using Newtonsoft.Json for serializing OpenAPI documents.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> so that additional calls can be chained.
    /// </returns>
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
