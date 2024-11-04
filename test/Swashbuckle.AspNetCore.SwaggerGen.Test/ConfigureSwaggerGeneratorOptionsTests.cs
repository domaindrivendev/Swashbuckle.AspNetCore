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
        Assert.Equal(22, publicProperties.Length);
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

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
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

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
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

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
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

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.DocumentFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.DocumentFilters.First(), swaggerGeneratorOptions.DocumentFilters.Last());
    }

    [Fact]
    public static void AddingParameterAsyncFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testParameterFilter = new TestParameterFilter();

        var options = new SwaggerGenOptions();
        options.AddParameterAsyncFilterInstance(testParameterFilter);
        options.AddParameterAsyncFilterInstance(testParameterFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.ParameterAsyncFilters.Count);
        Assert.Same(testParameterFilter, swaggerGeneratorOptions.ParameterAsyncFilters.First());
        Assert.Same(testParameterFilter, swaggerGeneratorOptions.ParameterAsyncFilters.Last());
    }

    [Fact]
    public static void AddingParameterAsyncFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.ParameterAsyncFilter<TestParameterFilter>();
        options.ParameterAsyncFilter<TestParameterFilter>();

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.ParameterAsyncFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.ParameterAsyncFilters.First(), swaggerGeneratorOptions.ParameterAsyncFilters.Last());
    }

    [Fact]
    public static void AddingRequestBodyAsyncFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testRequestBodyFilter = new TestRequestBodyFilter();

        var options = new SwaggerGenOptions();
        options.AddRequestBodyAsyncFilterInstance(testRequestBodyFilter);
        options.AddRequestBodyAsyncFilterInstance(testRequestBodyFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.RequestBodyAsyncFilters.Count);
        Assert.Same(testRequestBodyFilter, swaggerGeneratorOptions.RequestBodyAsyncFilters.First());
        Assert.Same(testRequestBodyFilter, swaggerGeneratorOptions.RequestBodyAsyncFilters.Last());
    }

    [Fact]
    public static void AddingRequestBodyAsyncFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.RequestBodyAsyncFilter<TestRequestBodyFilter>();
        options.RequestBodyAsyncFilter<TestRequestBodyFilter>();

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.RequestBodyAsyncFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.RequestBodyAsyncFilters.First(), swaggerGeneratorOptions.RequestBodyAsyncFilters.Last());
    }

    [Fact]
    public static void AddingOperationAsyncFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testOperationFilter = new TestOperationFilter();

        var options = new SwaggerGenOptions();
        options.AddOperationAsyncFilterInstance(testOperationFilter);
        options.AddOperationAsyncFilterInstance(testOperationFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.OperationAsyncFilters.Count);
        Assert.Same(testOperationFilter, swaggerGeneratorOptions.OperationAsyncFilters.First());
        Assert.Same(testOperationFilter, swaggerGeneratorOptions.OperationAsyncFilters.Last());
    }

    [Fact]
    public static void AddingOperationAsyncFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.OperationAsyncFilter<TestOperationFilter>();
        options.OperationAsyncFilter<TestOperationFilter>();

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.OperationAsyncFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.OperationAsyncFilters.First(), swaggerGeneratorOptions.OperationAsyncFilters.Last());
    }

    [Fact]
    public static void AddingDocumentAsyncFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testDocumentFilter = new TestDocumentFilter();

        var options = new SwaggerGenOptions();
        options.AddDocumentAsyncFilterInstance(testDocumentFilter);
        options.AddDocumentAsyncFilterInstance(testDocumentFilter);

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            null,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.DocumentAsyncFilters.Count);
        Assert.Same(testDocumentFilter, swaggerGeneratorOptions.DocumentAsyncFilters.First());
        Assert.Same(testDocumentFilter, swaggerGeneratorOptions.DocumentAsyncFilters.Last());
    }

    [Fact]
    public static void AddingDocumentAsyncFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.DocumentAsyncFilter<TestDocumentFilter>();
        options.DocumentAsyncFilter<TestDocumentFilter>();

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Equal(2, swaggerGeneratorOptions.DocumentAsyncFilters.Count);
        Assert.NotSame(swaggerGeneratorOptions.DocumentAsyncFilters.First(), swaggerGeneratorOptions.DocumentAsyncFilters.Last());
    }

    [Fact]
    public static void AddingFilterDescriptorWithFilterInstance_WhenConfiguringOptions_NoExceptionIsThrown()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.OperationFilterDescriptors.Add(
            new FilterDescriptor()
            {
                Type = null,
                FilterInstance = new TestOperationFilter(),
            });

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSwaggerGeneratorOptions = new ConfigureSwaggerGeneratorOptions(
            Options.Create(options),
            serviceProvider,
            webhostingEnvironment);
        var swaggerGeneratorOptions = new SwaggerGeneratorOptions();

        configureSwaggerGeneratorOptions.Configure(swaggerGeneratorOptions);

        Assert.Single(swaggerGeneratorOptions.OperationFilters);
    }
}
