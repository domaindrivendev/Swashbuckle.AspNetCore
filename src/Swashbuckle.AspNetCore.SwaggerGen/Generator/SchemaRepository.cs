using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRepository
    {
        private Dictionary<Type, string> _reservedIds = new Dictionary<Type, string>();

        public void ReserveIdFor(Type type, string schemaId)
        {
            if (_reservedIds.ContainsValue(schemaId))
            {
                var originalType = _reservedIds.First(entry => entry.Value == schemaId).Key;

                throw new InvalidOperationException(
                    $"Can't use schemaId \"${schemaId}\" for type \"${type}\". " +
                    $"The same schemaId was already used for type \"${originalType}\"");
            }

            _reservedIds.Add(type, schemaId);
        }

        public bool TryGetIdFor(Type type, out string schemaId)
        {
            return _reservedIds.TryGetValue(type, out schemaId);
        }

        public void AddSchemaFor(Type type, OpenApiSchema schema)
        {
            if (!_reservedIds.TryGetValue(type, out string schemaId))
                throw new InvalidOperationException("TODO:");

            Schemas.Add(schemaId, schema);
        }

        public Dictionary<string, OpenApiSchema> Schemas { get; private set; } = new Dictionary<string, OpenApiSchema>();
    }
}
