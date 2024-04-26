using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class ConfigureSwaggerGeneratorOptionsTests
{
    [Fact]
    public static void DeepCopy_Copies_All_Properties()
    {
        var type = typeof(SwaggerGeneratorOptions);
        var publicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // If this assertion fails, it means that a new property has been added
        // to SwaggerGeneratorOptions and ConfigureSwaggerGeneratorOptions.DeepCopy() needs to be updated
        Assert.Equal(18, publicProperties.Length);
    }

    [Fact]
    public static void AddingParameterFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testParameterFilter = new TestParameterFilter();

        var options = new SwaggerGenOptions();
        options.AddParameterFilterInstance(testParameterFilter);
        options.AddParameterFilterInstance(testParameterFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.ParameterFilters.Count);
        Assert.Same(testParameterFilter, swaggerGeneratorOptions.ParameterFilters.First());
        Assert.Same(testParameterFilter, swaggerGeneratorOptions.ParameterFilters.Last());
    }

    [Fact]
    public static void AddingParameterFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.ParameterFilter<TestParameterFilter>();
        options.ParameterFilter<TestParameterFilter>();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.ParameterFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.ParameterFilters.First(), swaggerGeneratorOptions.ParameterFilters.Last());
    }

    [Fact]
    public static void AddingRequestBodyFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testRequestBodyFilter = new TestRequestBodyFilter();

        var options = new SwaggerGenOptions();
        options.AddRequestBodyFilterInstance(testRequestBodyFilter);
        options.AddRequestBodyFilterInstance(testRequestBodyFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.RequestBodyFilters.Count);
        Assert.Same(testRequestBodyFilter, swaggerGeneratorOptions.RequestBodyFilters.First());
        Assert.Same(testRequestBodyFilter, swaggerGeneratorOptions.RequestBodyFilters.Last());
    }

    [Fact]
    public static void AddingRequestBodyFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.RequestBodyFilter<TestRequestBodyFilter>();
        options.RequestBodyFilter<TestRequestBodyFilter>();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.RequestBodyFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.RequestBodyFilters.First(), swaggerGeneratorOptions.RequestBodyFilters.Last());
    }

    [Fact]
    public static void AddingOperationFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testOperationFilter = new TestOperationFilter();

        var options = new SwaggerGenOptions();
        options.AddOperationFilterInstance(testOperationFilter);
        options.AddOperationFilterInstance(testOperationFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.OperationFilters.Count);
        Assert.Same(testOperationFilter, swaggerGeneratorOptions.OperationFilters.First());
        Assert.Same(testOperationFilter, swaggerGeneratorOptions.OperationFilters.Last());
    }

    [Fact]
    public static void AddingOperationFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.OperationFilter<TestOperationFilter>();
        options.OperationFilter<TestOperationFilter>();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.OperationFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.OperationFilters.First(), swaggerGeneratorOptions.OperationFilters.Last());
    }

    [Fact]
    public static void AddingDocumentFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testDocumentFilter = new TestDocumentFilter();

        var options = new SwaggerGenOptions();
        options.AddDocumentFilterInstance(testDocumentFilter);
        options.AddDocumentFilterInstance(testDocumentFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.DocumentFilters.Count);
        Assert.Same(testDocumentFilter, swaggerGeneratorOptions.DocumentFilters.First());
        Assert.Same(testDocumentFilter, swaggerGeneratorOptions.DocumentFilters.Last());
    }

    [Fact]
    public static void AddingDocumentFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.DocumentFilter<TestDocumentFilter>();
        options.DocumentFilter<TestDocumentFilter>();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.DocumentFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.DocumentFilters.First(), swaggerGeneratorOptions.DocumentFilters.Last());
    }
}
