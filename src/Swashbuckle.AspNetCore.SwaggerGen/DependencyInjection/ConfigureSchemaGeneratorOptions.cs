using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen;

internal class ConfigureSchemaGeneratorOptions(
    IOptions<SwaggerGenOptions> swaggerGenOptionsAccessor,
    IServiceProvider serviceProvider) : IConfigureOptions<SchemaGeneratorOptions>
{
    private readonly SwaggerGenOptions _swaggerGenOptions = swaggerGenOptionsAccessor.Value;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void Configure(SchemaGeneratorOptions options)
    {
        DeepCopy(_swaggerGenOptions.SchemaGeneratorOptions, options);

        // Create and add any filters that were specified through the FilterDescriptor lists
        _swaggerGenOptions.SchemaFilterDescriptors.ForEach(
            filterDescriptor => options.SchemaFilters.Add(GetOrCreateFilter<ISchemaFilter>(filterDescriptor)));
    }

    private static void DeepCopy(SchemaGeneratorOptions source, SchemaGeneratorOptions target)
    {
        target.CustomTypeMappings = new Dictionary<Type, Func<OpenApiSchema>>(source.CustomTypeMappings);
        target.UseInlineDefinitionsForEnums = source.UseInlineDefinitionsForEnums;
        target.SchemaIdSelector = source.SchemaIdSelector;
        target.IgnoreObsoleteProperties = source.IgnoreObsoleteProperties;
        target.UseAllOfForInheritance = source.UseAllOfForInheritance;
        target.UseOneOfForPolymorphism = source.UseOneOfForPolymorphism;
        target.SubTypesSelector = source.SubTypesSelector;
        target.DiscriminatorNameSelector = source.DiscriminatorNameSelector;
        target.DiscriminatorValueSelector = source.DiscriminatorValueSelector;
        target.UseAllOfToExtendReferenceSchemas = source.UseAllOfToExtendReferenceSchemas;
        target.UseOneOfForNullableEnums = source.UseOneOfForNullableEnums;
        target.SupportNonNullableReferenceTypes = source.SupportNonNullableReferenceTypes;
        target.NonNullableReferenceTypesAsRequired = source.NonNullableReferenceTypesAsRequired;
        target.SchemaFilters = [.. source.SchemaFilters];
    }

    private TFilter GetOrCreateFilter<TFilter>(FilterDescriptor filterDescriptor)
    {
        return (TFilter)(filterDescriptor.FilterInstance
            ?? ActivatorUtilities.CreateInstance(_serviceProvider, filterDescriptor.Type, filterDescriptor.Arguments));
    }
}
