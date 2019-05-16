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

        public SchemaGenerator(
            ISerializerSettingsAccessor serializationSettingsAccessor,
            IOptions<SchemaGeneratorOptions> optionsAccessor)
            : this(serializationSettingsAccessor.Value, optionsAccessor.Value)
        { }

        public SchemaGenerator(
            JsonSerializerSettings serializerSettings,
            SchemaGeneratorOptions options)
        {
            // NOTE: An OpenApiSchema MAY be used to describe both JSON and non-JSON media types. However, this implementation of ISchemaGenerator
            // is optimized for JSON and therefore couples to several JSON.NET abstractions so that it can provide accurate descriptions of types
            // in their serialized form.
            serializerSettings = serializerSettings ?? new JsonSerializerSettings();
            var contractResolver = serializerSettings.ContractResolver ?? new DefaultContractResolver();

            options = options ?? new SchemaGeneratorOptions();

            _generatorChain = new TypeSpecificSchemaGenerator(contractResolver, this, options)
                .Add(new FileSchemaGenerator(contractResolver, this, options))
                .Add(new ReferencedSchemaGenerator(contractResolver, this, options))
                .Add(new PolymorphicSchemaGenerator(contractResolver, this, options))
                .Add(new PrimitiveSchemaGenerator(contractResolver, this, serializerSettings, options))
                .Add(new DictionarySchemaGenerator(contractResolver, this, options))
                .Add(new ArraySchemaGenerator(contractResolver, this, options))
                .Add(new ObjectSchemaGenerator(contractResolver, this, options));
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository)
        {
            // Check if is nullable
            var isNullable = type.IsNullable() || type.IsFSharpOption();

            if (isNullable)
            {
                type = type.IsArray
                    ? type.GetGenericArguments()[0].MakeArrayType()
                    : type.GenericTypeArguments[0];
            }

            var schema = _generatorChain.GenerateSchema(type, schemaRepository);

            // Set Nullable
            schema.Nullable = isNullable;

            return schema;
        }
    }

    public abstract class ChainableSchemaGenerator : ISchemaGenerator
    {
        protected ChainableSchemaGenerator(
            IContractResolver contractResolver,
            ISchemaGenerator rootGenerator,
            SchemaGeneratorOptions options)
        {
            ContractResolver = contractResolver;
            RootGenerator = rootGenerator;
            Options = options;
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

            throw new InvalidOperationException($"Unable to generate schema for type - {type}");
        }

        protected IContractResolver ContractResolver { get; }

        protected ISchemaGenerator RootGenerator { get; }

        protected SchemaGeneratorOptions Options { get; }

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
