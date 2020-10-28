using System;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings
{
    public class SimpleSchemaMappingProvider : ISchemaMappingProvider {
        private readonly SchemaDisposition _schemaDisposition;
        private readonly Type _mappedType;
        private readonly Func<ISchemaMappingContext, OpenApiSchema> _mapping;

        public SimpleSchemaMappingProvider(Type mappedType, Func<ISchemaMappingContext, OpenApiSchema> mapping)
            : this(SchemaDisposition.Reference, mappedType, mapping)
        { }

        public SimpleSchemaMappingProvider(SchemaDisposition schemaDisposition, Type mappedType, Func<ISchemaMappingContext, OpenApiSchema> mapping) {
            _schemaDisposition = schemaDisposition;
            _mappedType = mappedType;
            _mapping = mapping;
        }

        public bool TryGetMapping(Type type, out SchemaMapping result)
        {
            if(MapsType(type)) {
                result = SchemaMapping.Create(_schemaDisposition, _mapping);
                return true;
            } else {
                result = null;
                return false;
            }
        }

        public bool MapsType(Type type) =>
            _mappedType.Equals(type)
            || _mappedType.IsGenericTypeDefinition && type.IsGenericType && _mappedType.Equals(type.GetGenericTypeDefinition());

        public override string ToString() => $"{nameof(SimpleSchemaMappingProvider)}({_mappedType})";
    }
}