using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings
{
    /// <summary>
    /// Encapsulates a resolved method for generating a schema as well as the desired
    /// <see cref="SchemaDisposition"/> of the resulting schema.
    /// </summary>
    public class SchemaMapping
    {
        public static SchemaMapping Inline(Func<OpenApiSchema> schemaFactory) =>
            Create(SchemaDisposition.Inline, schemaFactory);

        public static SchemaMapping Inline(Func<ISchemaMappingContext, OpenApiSchema> schemaFactory) =>
            Create(SchemaDisposition.Inline, schemaFactory);

        public static SchemaMapping Reference(Func<OpenApiSchema> schemaFactory) =>
            Create(SchemaDisposition.Reference, schemaFactory);

        public static SchemaMapping Reference(Func<ISchemaMappingContext, OpenApiSchema> schemaFactory) =>
            Create(SchemaDisposition.Reference, schemaFactory);

        /// <summary>
        /// Creates a <see cref="SchemaMapping"/> with a <see cref="SchemaDisposition"/> of
        /// <see cref="SchemaDisposition.Reference"/> when the provided condition is true.
        /// </summary>
        public static SchemaMapping ReferenceWhen(bool condition, Func<OpenApiSchema> schemaFactory) =>
            Create(condition ? SchemaDisposition.Reference : SchemaDisposition.Inline, schemaFactory);

        /// <summary>
        /// Creates a <see cref="SchemaMapping"/> with a <see cref="SchemaDisposition"/> of
        /// <see cref="SchemaDisposition.Reference"/> when the provided condition is true.
        /// </summary>
        public static SchemaMapping ReferenceWhen(bool condition, Func<ISchemaMappingContext, OpenApiSchema> schemaFactory) =>
            Create(condition ? SchemaDisposition.Reference : SchemaDisposition.Inline, schemaFactory);

        public static SchemaMapping Create(SchemaDisposition disposition, Func<OpenApiSchema> schemaFactory) =>
            new SchemaMapping(disposition, context => schemaFactory());

        public static SchemaMapping Create(SchemaDisposition disposition, Func<ISchemaMappingContext, OpenApiSchema> schemaFactory) =>
            new SchemaMapping(disposition, schemaFactory);

        public SchemaDisposition Disposition { get; }
        private readonly Func<ISchemaMappingContext, OpenApiSchema> _schemaFactory;

        private SchemaMapping(SchemaDisposition disposition, Func<ISchemaMappingContext, OpenApiSchema> schemaFactory) {
            Disposition = disposition;
            _schemaFactory = schemaFactory;
        }

        public OpenApiSchema GenerateSchema(ISchemaMappingContext context) {
            var generatedSchema = _schemaFactory(context);

            // TODO: We can't enforce this for Nullable<T> at the moment because the current design
            // forces the SchemaGenerator to return a reference to T if UseAllOfToExtendReferences is
            // not set.
            if(generatedSchema.Reference != null && !context.Type.IsNullable(out _))
                throw new Exception("Schema factory method produced a reference schema");

            return generatedSchema;
        }

        /// <summary>
        /// Creates a new <see cref="SchemaMapping"/> which applies the provided transformation to the
        /// generated schema.
        /// </summary>
        public SchemaMapping Transform(Func<OpenApiSchema, OpenApiSchema> transformation) =>
            new SchemaMapping(Disposition, context => transformation(_schemaFactory(context)));

        /// <summary>
        /// Creates a new <see cref="SchemaMapping"/> which applies the provided transformation to the
        /// generated schema.
        /// </summary>
        public SchemaMapping Transform(Func<ISchemaMappingContext, OpenApiSchema, OpenApiSchema> transformation) =>
            new SchemaMapping(Disposition, context => transformation(context, _schemaFactory(context)));
    }
}