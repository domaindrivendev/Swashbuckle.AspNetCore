using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NSubstitute;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class SwaggerGenOptionsExtensionsTests
{
    [Fact]
    public static void SwaggerGeneratorOptions_Setters_AssignValues()
    {
        var options = new SwaggerGenOptions();
        Func<string, ApiDescription, bool> predicate = (_, _) => true;
        Func<IEnumerable<ApiDescription>, ApiDescription> resolver = (_) => null;
        Func<ApiDescription, string> selector = (_) => "id";
        Func<ApiDescription, IList<string>> tags = (_) => ["tag"];
        var comparer = Comparer<string>.Default;
        var server = new OpenApiServer();

        options.DocInclusionPredicate(predicate);
        options.IgnoreObsoleteActions();
        options.ResolveConflictingActions(resolver);
        options.CustomOperationIds(selector);
        options.TagActionsBy(tags);
        options.OrderActionsBy(selector);
        options.SortSchemasWith(comparer);
        options.DescribeAllParametersInCamelCase();
        options.AddServer(server);
        options.InferSecuritySchemes();

        var generator = options.SwaggerGeneratorOptions;
        Assert.Same(predicate, generator.DocInclusionPredicate);
        Assert.True(generator.IgnoreObsoleteActions);
        Assert.Same(resolver, generator.ConflictingActionsResolver);
        Assert.Same(selector, generator.OperationIdSelector);
        Assert.Same(tags, generator.TagsSelector);
        Assert.Same(selector, generator.SortKeySelector);
        Assert.Same(comparer, generator.SchemaComparer);
        Assert.True(generator.DescribeAllParametersInCamelCase);
        Assert.Contains(server, generator.Servers);
        Assert.True(generator.InferSecuritySchemes);
    }

    [Fact]
    public static void SchemaGeneratorOptions_Setters_AssignValues()
    {
        var options = new SwaggerGenOptions();
        Func<IOpenApiSchema> factory = () => new OpenApiSchema();
        Func<Type, string> name = (_) => "name";
        Func<Type, IEnumerable<Type>> subTypes = (_) => [];

        options.MapType(typeof(Guid), factory);
        options.MapType<DateTime>(factory);
        options.UseInlineDefinitionsForEnums();
        options.CustomSchemaIds(name);
        options.IgnoreObsoleteProperties();
        options.UseAllOfForInheritance();
        options.UseOneOfForPolymorphism();
        options.SelectSubTypesUsing(subTypes);
        options.SelectDiscriminatorNameUsing(name);
        options.SelectDiscriminatorValueUsing(name);
        options.UseAllOfToExtendReferenceSchemas();
        options.SupportNonNullableReferenceTypes();
        options.NonNullableReferenceTypesAsRequired();

        var generator = options.SchemaGeneratorOptions;
        Assert.Same(factory, generator.CustomTypeMappings[typeof(Guid)]);
        Assert.Same(factory, generator.CustomTypeMappings[typeof(DateTime)]);
        Assert.True(generator.UseInlineDefinitionsForEnums);
        Assert.Same(name, generator.SchemaIdSelector);
        Assert.True(generator.IgnoreObsoleteProperties);
        Assert.True(generator.UseAllOfForInheritance);
        Assert.True(generator.UseOneOfForPolymorphism);
        Assert.Same(subTypes, generator.SubTypesSelector);
        Assert.Same(name, generator.DiscriminatorNameSelector);
        Assert.Same(name, generator.DiscriminatorValueSelector);
        Assert.True(generator.UseAllOfToExtendReferenceSchemas);
        Assert.True(generator.SupportNonNullableReferenceTypes);
        Assert.True(generator.NonNullableReferenceTypesAsRequired);
    }

    [Fact]
    public static void FilterRegistration_AddsDescriptors()
    {
        var options = new SwaggerGenOptions();

        options.SchemaFilter<ISchemaFilter>();
        options.AddSchemaFilterInstance(Substitute.For<ISchemaFilter>());
        options.ParameterFilter<IParameterFilter>();
        options.ParameterAsyncFilter<IParameterAsyncFilter>();
        options.AddParameterFilterInstance(Substitute.For<IParameterFilter>());
        options.AddParameterAsyncFilterInstance(Substitute.For<IParameterAsyncFilter>());
        options.RequestBodyFilter<IRequestBodyFilter>();
        options.RequestBodyAsyncFilter<IRequestBodyAsyncFilter>();
        options.AddRequestBodyFilterInstance(Substitute.For<IRequestBodyFilter>());
        options.AddRequestBodyAsyncFilterInstance(Substitute.For<IRequestBodyAsyncFilter>());
        options.OperationFilter<IOperationFilter>();
        options.OperationAsyncFilter<IOperationAsyncFilter>();
        options.AddOperationFilterInstance(Substitute.For<IOperationFilter>());
        options.AddOperationAsyncFilterInstance(Substitute.For<IOperationAsyncFilter>());
        options.DocumentFilter<IDocumentFilter>();
        options.DocumentAsyncFilter<IDocumentAsyncFilter>();
        options.AddDocumentFilterInstance(Substitute.For<IDocumentFilter>());
        options.AddDocumentAsyncFilterInstance(Substitute.For<IDocumentAsyncFilter>());

        Assert.Equal(2, options.SchemaFilterDescriptors.Count);
        Assert.Equal(4, options.ParameterFilterDescriptors.Count);
        Assert.Equal(4, options.RequestBodyFilterDescriptors.Count);
        Assert.Equal(4, options.OperationFilterDescriptors.Count);
        Assert.Equal(4, options.DocumentFilterDescriptors.Count);
    }

    [Fact]
    public static void FilterRegistration_ThrowsIfOptionsIsNull()
    {
        SwaggerGenOptions options = null;

        Assert.Throws<ArgumentNullException>(() => options.SchemaFilter<ISchemaFilter>());
        Assert.Throws<ArgumentNullException>(() => options.AddSchemaFilterInstance(Substitute.For<ISchemaFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.ParameterFilter<IParameterFilter>());
        Assert.Throws<ArgumentNullException>(() => options.ParameterAsyncFilter<IParameterAsyncFilter>());
        Assert.Throws<ArgumentNullException>(() => options.AddParameterFilterInstance(Substitute.For<IParameterFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.AddParameterAsyncFilterInstance(Substitute.For<IParameterAsyncFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.RequestBodyFilter<IRequestBodyFilter>());
        Assert.Throws<ArgumentNullException>(() => options.RequestBodyAsyncFilter<IRequestBodyAsyncFilter>());
        Assert.Throws<ArgumentNullException>(() => options.AddRequestBodyFilterInstance(Substitute.For<IRequestBodyFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.AddRequestBodyAsyncFilterInstance(Substitute.For<IRequestBodyAsyncFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.OperationFilter<IOperationFilter>());
        Assert.Throws<ArgumentNullException>(() => options.OperationAsyncFilter<IOperationAsyncFilter>());
        Assert.Throws<ArgumentNullException>(() => options.AddOperationFilterInstance(Substitute.For<IOperationFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.AddOperationAsyncFilterInstance(Substitute.For<IOperationAsyncFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.DocumentFilter<IDocumentFilter>());
        Assert.Throws<ArgumentNullException>(() => options.DocumentAsyncFilter<IDocumentAsyncFilter>());
        Assert.Throws<ArgumentNullException>(() => options.AddDocumentFilterInstance(Substitute.For<IDocumentFilter>()));
        Assert.Throws<ArgumentNullException>(() => options.AddDocumentAsyncFilterInstance(Substitute.For<IDocumentAsyncFilter>()));
    }

    [Fact]
    public static void FilterInstanceRegistration_ThrowsIfInstanceIsNull()
    {
        var options = new SwaggerGenOptions();

        Assert.Throws<ArgumentNullException>(() => options.AddSchemaFilterInstance<ISchemaFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddParameterFilterInstance<IParameterFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddParameterAsyncFilterInstance<IParameterAsyncFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddRequestBodyFilterInstance<IRequestBodyFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddRequestBodyAsyncFilterInstance<IRequestBodyAsyncFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddOperationFilterInstance<IOperationFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddOperationAsyncFilterInstance<IOperationAsyncFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddDocumentFilterInstance<IDocumentFilter>(null));
        Assert.Throws<ArgumentNullException>(() => options.AddDocumentAsyncFilterInstance<IDocumentAsyncFilter>(null));
    }
}
