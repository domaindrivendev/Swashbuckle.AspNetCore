using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
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
    public static void AddingDocumentFilters_WhenConfiguringOption_DifferentInstanceIsAdded()
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
}
