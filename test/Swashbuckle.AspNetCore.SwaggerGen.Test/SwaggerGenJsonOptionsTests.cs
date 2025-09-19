using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

using MinimalApiJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class SwaggerGenJsonOptionsTests
{
    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_MinimalApi_JsonOptions_When_Overridden()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.AllowTrailingCommas = true);
        services.AddSwaggerGenMinimalApisJsonOptions();

        using var provider = services.BuildServiceProvider();

        var minimalApiJsonOptions = provider.GetService<IOptions<MinimalApiJsonOptions>>().Value.SerializerOptions;
        var swaggerGenSerializerOptions = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions;
        Assert.Equal(minimalApiJsonOptions, swaggerGenSerializerOptions);
    }

    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_Mvc_JsonOptions_When_Overridden()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.ConfigureHttpJsonOptions(o => o.SerializerOptions.AllowTrailingCommas = true);
        services.AddSwaggerGenMvcJsonOptions();

        using var provider = services.BuildServiceProvider();

        var mvcJsonOptions = provider.GetService<IOptions<MvcJsonOptions>>().Value.JsonSerializerOptions;
        var swaggerGenSerializerOptions = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions;
        Assert.Equal(mvcJsonOptions, swaggerGenSerializerOptions);
    }

    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_Mvc_JsonOptions_When_Not_Using_Minimal_Apis()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.AllowTrailingCommas = true);

        using var provider = services.BuildServiceProvider();

        var mvcJsonOptions = provider.GetService<IOptions<MvcJsonOptions>>().Value.JsonSerializerOptions;
        var swaggerGenSerializerOptions = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions;
        Assert.Equal(mvcJsonOptions, swaggerGenSerializerOptions);
    }

    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_MinimalApi_JsonOptions_When_Configured()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.ConfigureHttpJsonOptions(o => o.SerializerOptions.AllowTrailingCommas = true);
        services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.AllowTrailingCommas = true);

        using var provider = services.BuildServiceProvider();

        var minimalApiJsonOptions = provider.GetService<IOptions<MinimalApiJsonOptions>>().Value.SerializerOptions;
        var swaggerGenSerializerOptions = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions;
        Assert.Equal(minimalApiJsonOptions, swaggerGenSerializerOptions);
    }
}
