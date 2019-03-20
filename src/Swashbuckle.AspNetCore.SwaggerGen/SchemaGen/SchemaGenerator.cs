using System;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly ChainableSchemaGenerator _generatorChain;

        public SchemaGenerator(IOptions<SchemaGeneratorOptions> optionsAccessor, ISerializerSettingsAccessor serializationSettingsAccessor)
            : this(optionsAccessor.Value, serializationSettingsAccessor?.SerializerSettings)
        { }

        public SchemaGenerator(SchemaGeneratorOptions options, JsonSerializerSettings serializerSettings)
        {
            options = options ?? new SchemaGeneratorOptions();

            // NOTE: An OpenApiSchema MAY be used to describe both JSON and non-JSON media types. However, this implementation of ISchemaGenerator
            // is optimized for JSON and therefore couples to several JSON.NET abstractions so that it can provide accurate descriptions of types
            // in their serialized form.

            serializerSettings = serializerSettings ?? new JsonSerializerSettings();
            var contractResolver = serializerSettings.ContractResolver ?? new DefaultContractResolver();

            _generatorChain = new TypeSpecificSchemaGenerator(options, this, contractResolver)
                .Add(new FileSchemaGenerator(options, this, contractResolver))
                .Add(new ReferencedSchemaGenerator(options, this, contractResolver))
                .Add(new PolymorphicSchemaGenerator(options, this, contractResolver))
                .Add(new PrimitiveSchemaGenerator(options, this, contractResolver, serializerSettings))
                .Add(new DictionarySchemaGenerator(options, this, contractResolver))
                .Add(new ArraySchemaGenerator(options, this, contractResolver))
                .Add(new ObjectSchemaGenerator(options, this, contractResolver));
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository)
        {
            // Check if is nullable
            var isNullable = type.IsNullable() || type.IsFSharpOption();

            // Update type
            type = isNullable
                ? type.GenericTypeArguments[0]
                : type;

            var schema = _generatorChain.GenerateSchema(type, schemaRepository);

            // Set Nullable
            schema.Nullable = isNullable;

            return schema;
        }
    }

    public abstract class ChainableSchemaGenerator : ISchemaGenerator
    {
        protected ChainableSchemaGenerator(SchemaGeneratorOptions options, ISchemaGenerator rootGenerator, IContractResolver contractResolver)
        {
            Options = options;
            RootGenerator = rootGenerator;
            ContractResolver = contractResolver;
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository)
        {
            if (CanGenerateSchemaFor(type))
            {
                var schema = GenerateSchemaFor(type, schemaRepository);

                if (schema.Reference == null) ApplyFilters(schema, type, schemaRepository);

                return schema;
            }

            if (Next != null)
                return Next.GenerateSchema(type, schemaRepository);

            throw new InvalidOperationException("TODO:");
        }

        protected SchemaGeneratorOptions Options { get; }

        protected ISchemaGenerator RootGenerator { get; }

        protected IContractResolver ContractResolver { get; }

        protected ChainableSchemaGenerator Next { get; private set; }

        public ChainableSchemaGenerator Add(ChainableSchemaGenerator chainableSchemaGenerator)
        {
            var tail = this;
            while (tail.Next != null)
            {
                tail = tail.Next;
            }

            tail.Next = chainableSchemaGenerator;
            return this;
        }

        protected abstract bool CanGenerateSchemaFor(Type type);

        protected abstract OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository);

        private void ApplyFilters(OpenApiSchema schema, Type type, SchemaRepository schemaRepository)
        {
            if (schema.Reference != null) return;

            var filterContext = new SchemaFilterContext(
                type,
                ContractResolver.ResolveContract(type),
                schemaRepository,
                RootGenerator);

            foreach (var filter in Options.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }
        }
    }
}