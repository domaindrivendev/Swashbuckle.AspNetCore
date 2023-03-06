using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRepository
    {
        private readonly ConcurrentDictionary<Type, string> _reservedIds = new();

        public SchemaRepository(string documentName = null)
        {
            DocumentName = documentName;
        }

        public string DocumentName { get; }

        public ConcurrentDictionary<string, OpenApiSchema> Schemas { get; } = new();

        public void RegisterType(Type type, string schemaId)
        {
            if (_reservedIds.Values.Contains(schemaId))
            {
                var conflictingType = _reservedIds.First(entry => entry.Value == schemaId).Key;

                throw new InvalidOperationException(
                    $"Can't use schemaId \"${schemaId}\" for type \"${type}\". " +
                    $"The same schemaId is already used for type \"${conflictingType}\"");
            }

            _reservedIds.AddOrUpdate(type, schemaId, (_, _) => schemaId);
        }

        public bool TryLookupByType(Type type, out OpenApiSchema referenceSchema)
        {
            if (_reservedIds.TryGetValue(type, out string schemaId))
            {
                referenceSchema = new OpenApiSchema
                {
                    Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = schemaId }
                };
                return true;
            }

            referenceSchema = null;
            return false;
        }

        public OpenApiSchema AddDefinition(string schemaId, OpenApiSchema schema)
        {
            Schemas.AddOrUpdate(schemaId, schema, (_, _) => schema);

            return new OpenApiSchema
            {
                Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = schemaId }
            };
        }
    }
}