using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen.DependencyInjection;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class SwaggerGenJsonOptionsTests
{
    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_MinimalApi_JsonOptions_When_Overridden()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new DummyConverter()));
        services.AddSwaggerGenMinimalApisJsonOptions();

        using var provider = services.BuildServiceProvider();

        var swaggerGenConverters = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions.Converters;

        Assert.Empty(swaggerGenConverters);
    }

    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_Mvc_JsonOptions_When_Overridden()
    {
        var expectedDummyConverter = new DummyConverter();

        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new DummyConverter()));
        services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(expectedDummyConverter));
        services.AddSwaggerGenMvcJsonOptions();

        using var provider = services.BuildServiceProvider();

        var swaggerGenDummyConverter = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions.Converters.FirstOrDefault();

        Assert.Equal(expectedDummyConverter, swaggerGenDummyConverter);
    }

    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_Mvc_JsonOptions_When_Not_Using_Minimal_Apis()
    {
        var expectedDummyConverter = new DummyConverter();

        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(expectedDummyConverter));

        using var provider = services.BuildServiceProvider();

        var swaggerGenDummyConverter = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions.Converters.FirstOrDefault();

        Assert.Equal(expectedDummyConverter, swaggerGenDummyConverter);
    }

    [Fact]
    public static void Ensure_SwaggerGenJsonOptions_Uses_MinimalApi_JsonOptions_When_Configured()
    {
        var expectedDummyConverter = new DummyConverter();

        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment, DummyHostEnvironment>();
        services.AddSwaggerGen();
        services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(expectedDummyConverter));
        services.AddMvcCore().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new DummyConverter()));

        using var provider = services.BuildServiceProvider();

        var swaggerGenDummyConverter = provider.GetService<IOptions<SwaggerGenJsonOptions>>().Value.SerializerOptions.Converters.FirstOrDefault();

        Assert.Equal(expectedDummyConverter, swaggerGenDummyConverter);
    }

    private sealed class DummyHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
    }

    private sealed class DummyConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
