using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRepository
    {
        private readonly Dictionary<Type, string> _reservedIds = new Dictionary<Type, string>();

        private readonly Dictionary<Type, Func<object, string>> _enumConverters = new Dictionary<Type, Func<object, string>>();

        public IDictionary<string, OpenApiSchema> Schemas { get; private set; } = new SortedDictionary<string, OpenApiSchema>();

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

        public void RegisterEnumConverter(Type type, Func<object, string> converter)
        {
            if (!_enumConverters.ContainsKey(type))
            {
                _enumConverters.Add(type, converter);
            }
        }

        public Func<object, string> RetrieveEnumConverter(Type type)
        {
            _enumConverters.TryGetValue(type, out var converter);
            return converter;
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
