using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

internal sealed class ConfigureMinimalApiSwaggerGenJsonOptions(IOptions<JsonOptions> jsonOptions) : IConfigureOptions<SwaggerGenJsonOptions>
{
    public void Configure(SwaggerGenJsonOptions options)
    {
        options.SerializerOptions = jsonOptions.Value.SerializerOptions;
    }
}
