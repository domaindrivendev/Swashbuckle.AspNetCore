using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SchemaGeneratorOptions
{
    public SchemaGeneratorOptions()
    {
        CustomTypeMappings = new Dictionary<Type, Func<IOpenApiSchema>>();
        SchemaIdSelector = DefaultSchemaIdSelector;
        SubTypesSelector = DefaultSubTypesSelector;
        DiscriminatorNameSelector = DefaultDiscriminatorNameSelector;
        DiscriminatorValueSelector = DefaultDiscriminatorValueSelector;
        DiscriminatorDefaultMappingSelector = DefaultDiscriminatorDefaultMappingSelector;
        SchemaFilters = [];
    }

    public IDictionary<Type, Func<IOpenApiSchema>> CustomTypeMappings { get; set; }

    public bool UseInlineDefinitionsForEnums { get; set; }

    public Func<Type, string> SchemaIdSelector { get; set; }

    public bool IgnoreObsoleteProperties { get; set; }

    public bool UseAllOfForInheritance { get; set; }

    public bool UseOneOfForPolymorphism { get; set; }

    public Func<Type, IEnumerable<Type>> SubTypesSelector { get; set; }

    public Func<Type, string> DiscriminatorNameSelector { get; set; }

    public Func<Type, string> DiscriminatorValueSelector { get; set; }

    /// <summary>
    /// Gets or sets a function that returns the type whose schema should be used to validate a polymorphic payload
    /// when the discriminator property is not present, or has a value that is not in the discriminator mapping.
    /// </summary>
    /// <remarks>
    /// The selected type is emitted as the <c>defaultMapping</c> of the discriminator, which is only supported by
    /// OpenAPI 3.2 and therefore only when targeting .NET 11 or later. Return <see langword="null"/> to not emit a default mapping.
    /// </remarks>
    public Func<Type, Type> DiscriminatorDefaultMappingSelector { get; set; }

    public bool UseAllOfToExtendReferenceSchemas { get; set; }

    public bool SupportNonNullableReferenceTypes { get; set; }

    public bool NonNullableReferenceTypesAsRequired { get; set; }

    public IList<ISchemaFilter> SchemaFilters { get; set; }

    private string DefaultSchemaIdSelector(Type modelType)
    {
        if (!modelType.IsConstructedGenericType)
        {
            return modelType.Name.Replace("[]", "Array");
        }

        var prefix = modelType.GetGenericArguments()
            .Select(DefaultSchemaIdSelector)
            .Aggregate((previous, current) => previous + current);

        return prefix + modelType.Name.Split('`').First();
    }

    private IEnumerable<Type> DefaultSubTypesSelector(Type baseType)
        => baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));

    private string DefaultDiscriminatorNameSelector(Type baseType) => null;

    private string DefaultDiscriminatorValueSelector(Type subType) => null;

    private Type DefaultDiscriminatorDefaultMappingSelector(Type baseType) => null;
}
