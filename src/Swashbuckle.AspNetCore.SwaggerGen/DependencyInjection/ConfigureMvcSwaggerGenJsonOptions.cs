using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

internal sealed class ConfigureMvcSwaggerGenJsonOptions(IOptions<JsonOptions> jsonOptions) : IConfigureOptions<SwaggerGenJsonOptions>
{
    public void Configure(SwaggerGenJsonOptions options)
    {
        options.SerializerOptions = jsonOptions.Value.JsonSerializerOptions;
    }
}
