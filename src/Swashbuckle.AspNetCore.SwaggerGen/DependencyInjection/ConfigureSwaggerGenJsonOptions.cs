using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

internal sealed class ConfigureSwaggerGenJsonOptions : IPostConfigureOptions<SwaggerGenJsonOptions>
{
    private readonly IEnumerable<IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>> _minimalApiConfigureOptions;
    private readonly IEnumerable<IPostConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>> _minimalApiPostConfigureOptions;
    private readonly Microsoft.AspNetCore.Http.Json.JsonOptions _minimalApiJsonOptions;
    private readonly Microsoft.AspNetCore.Mvc.JsonOptions _mvcJsonOptions;

    public ConfigureSwaggerGenJsonOptions(
        IEnumerable<IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>> minimalApiConfigureOptions,
        IEnumerable<IPostConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>> minimalApiPostConfigureOptions,
        IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> minimalApiJsonOptions,
        IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> mvcJsonOptions)
    {
        _minimalApiConfigureOptions = minimalApiConfigureOptions;
        _minimalApiPostConfigureOptions = minimalApiPostConfigureOptions;
        _minimalApiJsonOptions = minimalApiJsonOptions.Value;
        _mvcJsonOptions = mvcJsonOptions.Value;
    }

    public void PostConfigure(string name, SwaggerGenJsonOptions options)
    {
        if (options.SerializerOptions != null)
        {
            return;
        }

        /*
         * There is no surefire way to do this.
         * However, both JsonOptions are defaulted in the same way.
         * If neither is configured it makes no difference which one is chosen.
         * If both are configured, then we just need to make a choice.
         * As Minimal APIs are newer if someone is configuring them
         * it's probably more likely that is what they're using.
         * 
         * If either JsonOptions is null we will try to create a new instance as
         * a last resort as this is an expensive operation.
         */

        var serializerOptions = _mvcJsonOptions.JsonSerializerOptions;

        if (_minimalApiConfigureOptions.Any() || _minimalApiPostConfigureOptions.Any())
        {
            serializerOptions = _minimalApiJsonOptions.SerializerOptions;
        }

        options.SerializerOptions = serializerOptions;
    }
}
