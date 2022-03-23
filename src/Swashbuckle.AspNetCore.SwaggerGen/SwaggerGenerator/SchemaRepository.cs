using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRepository
    {
        private readonly Dictionary<Type, string> _reservedIds = new Dictionary<Type, string>();

        public SchemaRepository(string documentName = null)
        {
            DocumentName = documentName;
        }

        public string DocumentName { get; }

        public Dictionary<string, OpenApiSchema> Schemas { get; private set; } = new Dictionary<string, OpenApiSchema>();

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
            Schemas.Add(schemaId, schema);

            return new OpenApiSchema
            {
                Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = schemaId }
            };
        }
    }
}
