using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRepository
    {
        private Dictionary<Type, string> _reservedIds = new Dictionary<Type, string>();

        public Dictionary<string, OpenApiSchema> Schemas { get; private set; } = new Dictionary<string, OpenApiSchema>();

        public OpenApiSchema GetOrAdd(Type type, string schemaId, Func<OpenApiSchema> factoryMethod)
        {
            if (!_reservedIds.TryGetValue(type, out string reservedId))
            {
                // First invocation of the factoryMethod for this type - reserve the provided schemaId first, and then invoke the factory method.
                // Reserving the id first ensures that the factoryMethod will only be invoked once for a given type, even in recurrsive scenarios.
                // If subsequent calls are made for the same type, a simple reference will be created instead.
                ReserveIdFor(type, schemaId);
                Schemas.Add(schemaId, factoryMethod());
            }
            else
            {
                schemaId = reservedId;
            }

            return new OpenApiSchema
            {
                Reference = new OpenApiReference { Id = schemaId, Type = ReferenceType.Schema }
            };
        }

        public bool TryGetIdFor(Type type, out string schemaId)
        {
            return _reservedIds.TryGetValue(type, out schemaId);
        }
        
        private void ReserveIdFor(Type type, string schemaId)
        {
            if (_reservedIds.ContainsValue(schemaId))
            {
                var reservedForType = _reservedIds.First(entry => entry.Value == schemaId).Key;

                throw new InvalidOperationException(
                    $"Can't use schemaId \"${schemaId}\" for type \"${type}\". " +
                    $"The same schemaId is already used for type \"${reservedForType}\"");
            }

            _reservedIds.Add(type, schemaId);
        }
    }
}
