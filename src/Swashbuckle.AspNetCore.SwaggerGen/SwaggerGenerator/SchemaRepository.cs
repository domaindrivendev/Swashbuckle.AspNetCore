using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SchemaRepository(string documentName = null)
{
    private readonly Dictionary<Type, string> _reservedIds = [];

    public string DocumentName { get; } = documentName;

    public Dictionary<string, IOpenApiSchema> Schemas { get; private set; } = [];

    public void RegisterType(Type type, string schemaId)
    {
        if (_reservedIds.ContainsValue(schemaId))
        {
            var conflictingType = _reservedIds.First(entry => entry.Value == schemaId).Key;

            throw new InvalidOperationException(
                $"Can't use schemaId \"${schemaId}\" for type \"${type}\". " +
                $"The same schemaId is already used for type \"${conflictingType}\"");
        }

        _reservedIds.Add(type, schemaId);
    }

    public bool TryLookupByType(Type type, out OpenApiSchemaReference referenceSchema)
    {
        if (_reservedIds.TryGetValue(type, out string schemaId))
        {
            referenceSchema = new OpenApiSchemaReference(schemaId);
            return true;
        }

        referenceSchema = null;
        return false;
    }

    public OpenApiSchemaReference AddDefinition(string schemaId, OpenApiSchema schema)
    {
        Schemas.Add(schemaId, schema);

        return new OpenApiSchemaReference(schemaId);
    }

    public bool ReplaceSchemaId(Type schemaType, string replacementSchemaId)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentException.ThrowIfNullOrEmpty(replacementSchemaId);

        if (_reservedIds.TryGetValue(schemaType, out string oldSchemaId) &&
            oldSchemaId != replacementSchemaId &&
            Schemas.TryGetValue(oldSchemaId, out var targetSchema))
        {
            if (Schemas.TryAdd(replacementSchemaId, targetSchema))
            {
                Schemas.Remove(oldSchemaId);
                _reservedIds.Remove(schemaType);
                return true;
            }
        }

        return false;
    }
}
