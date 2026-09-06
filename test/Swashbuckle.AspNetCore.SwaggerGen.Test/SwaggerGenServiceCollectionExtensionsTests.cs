using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class SwaggerGenServiceCollectionExtensionsTests
{
    [Fact]
    public static void AddSwaggerGen_UsesMinimalApiJsonOptions_WhenMvcIsNotRegistered()
    {
        // IOptions<Mvc.JsonOptions> resolves even in an application that never registered MVC,
        // so without a registration check its camelCase default would win over what the
        // application actually configured for Minimal APIs.
        var services = CreateServices();
        services.ConfigureHttpJsonOptions((options) => options.SerializerOptions.PropertyNamingPolicy = MinimalApiNamingPolicy);
        services.AddSwaggerGen();

        var contract = ResolveDataContract(services);

        Assert.Equal("HelloWorld-minimal-api", Assert.Single(contract.ObjectProperties).Name);
    }

    [Fact]
    public static void AddSwaggerGen_UsesMvcJsonOptions_WhenMvcIsRegistered()
    {
        var services = CreateServices();
        services.AddControllers().AddJsonOptions((options) => options.JsonSerializerOptions.PropertyNamingPolicy = MvcNamingPolicy);
        services.AddSwaggerGen();

        var contract = ResolveDataContract(services);

        Assert.Equal("HelloWorld-mvc", Assert.Single(contract.ObjectProperties).Name);
    }

    [Fact]
    public static void AddSwaggerGen_PrefersMvcJsonOptions_WhenBothAreRegistered()
    {
        // A document describes every endpoint with one set of serializer options, so when an
        // application registers both, MVC keeps precedence as it always has.
        var services = CreateServices();
        services.ConfigureHttpJsonOptions((options) => options.SerializerOptions.PropertyNamingPolicy = MinimalApiNamingPolicy);
        services.AddControllers().AddJsonOptions((options) => options.JsonSerializerOptions.PropertyNamingPolicy = MvcNamingPolicy);
        services.AddSwaggerGen();

        var contract = ResolveDataContract(services);

        Assert.Equal("HelloWorld-mvc", Assert.Single(contract.ObjectProperties).Name);
    }

    [Fact]
    public static void AddSwaggerGen_UsesCamelCase_WhenNothingIsConfigured()
    {
        // Both option types default to the web defaults, so neither branch is observable here.
        var services = CreateServices();
        services.AddSwaggerGen();

        var contract = ResolveDataContract(services);

        Assert.Equal("helloWorld", Assert.Single(contract.ObjectProperties).Name);
    }

    private static JsonNamingPolicy MinimalApiNamingPolicy { get; } = new SuffixNamingPolicy("-minimal-api");

    private static JsonNamingPolicy MvcNamingPolicy { get; } = new SuffixNamingPolicy("-mvc");

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging();

        var hostEnvironment = Substitute.For<IWebHostEnvironment>();
        hostEnvironment.ApplicationName.Returns(typeof(SwaggerGenServiceCollectionExtensionsTests).Assembly.GetName().Name);
        services.AddSingleton(hostEnvironment);
        services.AddSingleton<IHostEnvironment>(hostEnvironment);

        return services;
    }

    private static DataContract ResolveDataContract(IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<ISerializerDataContractResolver>();
        return resolver.GetDataContractForType(typeof(Dto));
    }

    private sealed class SuffixNamingPolicy(string suffix) : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name + suffix;
    }

    private class Dto
    {
        public string HelloWorld { get; set; }
    }
}
