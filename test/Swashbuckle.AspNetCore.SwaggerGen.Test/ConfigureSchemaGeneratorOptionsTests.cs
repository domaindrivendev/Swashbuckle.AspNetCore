using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class ConfigureSchemaGeneratorOptionsTests
{
    [Fact]
    public static void DeepCopy_Copies_All_Properties()
    {
        var type = typeof(SchemaGeneratorOptions);
        var publicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // If this assertion fails, it means that a new property has been added
        // to SwaggerGeneratorOptions and ConfigureSchemaGeneratorOptions.DeepCopy() needs to be updated
        Assert.Equal(12, publicProperties.Length);
    }

    [Fact]
    public static void AddingSchemaFilterInstances_WhenConfiguringOptions_SameInstanceIsAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var testSchemaFilter = new TestSchemaFilter();

        var options = new SwaggerGenOptions();
        options.AddSchemaFilterInstance(testSchemaFilter);
        options.AddSchemaFilterInstance(testSchemaFilter);

        var configureSchemaGeneratorOptions = new ConfigureSchemaGeneratorOptions(Options.Create(options), null);
        var schemaGeneratorOptions = new SchemaGeneratorOptions();

        configureSchemaGeneratorOptions.Configure(schemaGeneratorOptions);

        Assert.Equal(2, schemaGeneratorOptions.SchemaFilters.Count);
        Assert.Same(testSchemaFilter, schemaGeneratorOptions.SchemaFilters.First());
        Assert.Same(testSchemaFilter, schemaGeneratorOptions.SchemaFilters.Last());
    }

    [Fact]
    public static void AddingSchemaFilterTypes_WhenConfiguringOptions_DifferentInstancesAreAdded()
    {
        var webhostingEnvironment = Substitute.For<IWebHostEnvironment>();
        webhostingEnvironment.ApplicationName.Returns("Swashbuckle.AspNetCore.SwaggerGen.Test");

        var options = new SwaggerGenOptions();
        options.SchemaFilter<TestSchemaFilter>();
        options.SchemaFilter<TestSchemaFilter>();

        using var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var configureSchemaGeneratorOptions = new ConfigureSchemaGeneratorOptions(Options.Create(options), serviceProvider);
        var schemaGeneratorOptions = new SchemaGeneratorOptions();

        configureSchemaGeneratorOptions.Configure(schemaGeneratorOptions);

        Assert.Equal(2, schemaGeneratorOptions.SchemaFilters.Count);
        Assert.NotSame(schemaGeneratorOptions.SchemaFilters.First(), schemaGeneratorOptions.SchemaFilters.Last());
    }
}
