using System;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings
{
    /// <summary>
    /// Implements a basic <see cref="ISchemaMappingProvider"/> based on the behavior used prior to the
    /// introduction of the <see cref="ISchemaMappingProvider"/> interface.
    /// </summary>
    [Obsolete]
    public class LegacySchemaMappingProvider : ISchemaMappingProvider {
        private readonly Type _mappedType;
        private readonly Func<OpenApiSchema> _schemaProvider;

        public LegacySchemaMappingProvider(Type mappedType, Func<OpenApiSchema> schemaProvider) {
            _mappedType = mappedType;
            _schemaProvider = schemaProvider;
        }

        public bool TryGetMapping(Type type, out SchemaMapping result)
        {
            if(MapsType(type)) {
                result = SchemaMapping.Create(SchemaDisposition.Inline, context => _schemaProvider());
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public bool MapsType(Type type) =>
            _mappedType.Equals(type)
            || _mappedType.IsGenericTypeDefinition && type.IsGenericType && _mappedType.Equals(type.GetGenericTypeDefinition());
    }
}