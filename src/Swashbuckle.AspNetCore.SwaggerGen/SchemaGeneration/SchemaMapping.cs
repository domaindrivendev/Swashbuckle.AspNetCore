using System;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaMapping
    {
        public static SchemaMapping Inline(Func<OpenApiSchema> schemaFactory)
            => new SchemaMapping(SchemaMappingKind.Inline, schemaFactory);

        public static SchemaMapping Reference(Func<OpenApiSchema> schemaFactory)
            => new SchemaMapping(SchemaMappingKind.Reference, schemaFactory);

        public static SchemaMapping ReferenceWhen(bool condition, Func<OpenApiSchema> schemaFactory)
        {
            var mappingKind = condition ? SchemaMappingKind.Reference : SchemaMappingKind.Inline;
            return new SchemaMapping(mappingKind, schemaFactory);
        }

        private SchemaMapping(SchemaMappingKind mappingKind, Func<OpenApiSchema> schemaFactory)
        {
            SchemaFactory = schemaFactory;
            MappingKind = mappingKind;
        }

        public SchemaMappingKind MappingKind { get; }

        public Func<OpenApiSchema> SchemaFactory { get; }
    }

    public enum SchemaMappingKind
    {
        Inline,
        Reference
    }
}