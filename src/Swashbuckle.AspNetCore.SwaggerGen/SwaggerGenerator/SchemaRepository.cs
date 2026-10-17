#nullable enable

// Project-wide PublicAPI tracking has not yet adopted '#nullable enable' (tracked in Directory.Build.props).
// Suppress RS0037 here so this file can opt into nullable annotations without forcing assembly-wide rollout.
#pragma warning disable RS0037

using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SchemaRepository(string? documentName = null)
{
    private readonly Dictionary<Type, string> _reservedIds = [];

    public string? DocumentName { get; } = documentName;

    public Dictionary<string, IOpenApiSchema> Schemas { get; } = [];

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

    public bool TryLookupByType(Type type, [NotNullWhen(true)] out OpenApiSchemaReference? referenceSchema)
    {
        referenceSchema = null;

        if (_reservedIds.TryGetValue(type, out string? schemaId) && schemaId is not null)
        {
            referenceSchema = new OpenApiSchemaReference(schemaId);
            return true;
        }

        return false;
    }

    public OpenApiSchemaReference AddDefinition(string schemaId, OpenApiSchema schema)
    {
        Schemas.Add(schemaId, schema);

        return new(schemaId)
        {
            Default = schema.Default,
            Description = schema.Description,
            Deprecated = schema.Deprecated,
            Examples = schema.Examples,
            ReadOnly = schema.ReadOnly,
            Title = schema.Title,
        };
    }

    public bool ReplaceSchemaId(Type schemaType, string replacementSchemaId)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentException.ThrowIfNullOrEmpty(replacementSchemaId);

        if (_reservedIds.TryGetValue(schemaType, out string? oldSchemaId) &&
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
