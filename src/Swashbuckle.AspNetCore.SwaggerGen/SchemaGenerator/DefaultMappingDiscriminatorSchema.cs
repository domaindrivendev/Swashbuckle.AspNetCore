using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// A schema with a discriminator whose property is optional if the discriminator has a default mapping.
/// </summary>
/// <remarks>
/// The discriminator property may only be optional if the discriminator has a default mapping, which is
/// only supported by OpenAPI 3.2, so the property is required when serializing earlier versions.
/// </remarks>
internal sealed class DefaultMappingDiscriminatorSchema : OpenApiSchema
{
    public override void SerializeAsV2(IOpenApiWriter writer)
    {
        if (TryRequireDiscriminator(out var schema))
        {
            schema.SerializeAsV2(writer);
        }
        else
        {
            base.SerializeAsV2(writer);
        }
    }

    public override void SerializeAsV3(IOpenApiWriter writer)
    {
        if (TryRequireDiscriminator(out var schema))
        {
            schema.SerializeAsV3(writer);
        }
        else
        {
            base.SerializeAsV3(writer);
        }
    }

    public override void SerializeAsV31(IOpenApiWriter writer)
    {
        if (TryRequireDiscriminator(out var schema))
        {
            schema.SerializeAsV31(writer);
        }
        else
        {
            base.SerializeAsV31(writer);
        }
    }

    private bool TryRequireDiscriminator(out OpenApiSchema schema)
    {
        schema = null;

        if (Discriminator is not { DefaultMapping: not null, PropertyName: { } propertyName } ||
            Required?.Contains(propertyName) == true)
        {
            return false;
        }

        // Serialize a copy so that the same document can be serialized concurrently as different versions
        schema = (OpenApiSchema)CreateShallowCopy();
        schema.Required = Required is { } required
            ? new SortedSet<string>(required) { propertyName }
            : new SortedSet<string>() { propertyName };

        return true;
    }
}
