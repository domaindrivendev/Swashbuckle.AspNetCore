using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftObjectHandler : SchemaGeneratorHandler
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly IContractResolver _contractResolver;
        private readonly ISchemaGenerator _schemaGenerator;

        public NewtonsoftObjectHandler(
            SchemaGeneratorOptions generatorOptions,
            IContractResolver contractResolver,
            ISchemaGenerator schemaGenerator)
        {
            _generatorOptions = generatorOptions;
            _contractResolver = contractResolver;
            _schemaGenerator = schemaGenerator;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (_contractResolver.ResolveContract(type) is JsonObjectContract)
            {
                var shouldBePolymorphic = _generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(type).Any();
                shouldBeReferenced = !(type == typeof(object) || shouldBePolymorphic);
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateSchema(Type type, SchemaRepository schemaRepository)
        {
            if (_generatorOptions.GeneratePolymorphicSchemas)
            {
                var knownSubTypes = _generatorOptions.SubTypesResolver(type);
                if (knownSubTypes.Any())
                {
                    return CreatePolymorphicSchema(knownSubTypes, schemaRepository);
                }
            }

            return CreateObjectSchema(type, schemaRepository);
        }

        private OpenApiSchema CreatePolymorphicSchema(IEnumerable<Type> knownSubTypes, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                OneOf = knownSubTypes
                    .Select(subType => _schemaGenerator.GenerateSchema(subType, schemaRepository))
                    .ToList()
            };
        }

        private OpenApiSchema CreateObjectSchema(Type type, SchemaRepository schemaRepository)
        {
            if (!(_contractResolver.ResolveContract(type) is JsonObjectContract jsonObjectContract))
                throw new InvalidOperationException($"Type {type} does not resolve to a JsonObjectContract");

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false
            };

            // If it's a baseType with known subTypes, add the discriminator property
            if (_generatorOptions.GeneratePolymorphicSchemas && _generatorOptions.SubTypesResolver(type).Any())
            {
                var discriminatorName = _generatorOptions.DiscriminatorSelector(type);
                schema.Properties.Add(discriminatorName, new OpenApiSchema { Type = "string" });
                schema.Required.Add(discriminatorName);
                schema.Discriminator = new OpenApiDiscriminator { PropertyName = discriminatorName };
            }

            foreach (var jsonProperty in jsonObjectContract.Properties)
            {
                if (jsonProperty.Ignored) continue;

                var customAttributes = jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo)
                    ? memberInfo.GetInlineOrMetadataTypeAttributes()
                    : Enumerable.Empty<object>();

                if (_generatorOptions.IgnoreObsoleteProperties && customAttributes.OfType<ObsoleteAttribute>().Any()) continue;
                var required = jsonProperty.IsRequiredSpecified()
                    ? jsonProperty.Required
                    : jsonObjectContract.ItemRequired ?? Required.Default;

                schema.Properties.Add(jsonProperty.PropertyName, CreatePropertySchema(jsonProperty, customAttributes, required, schemaRepository));

                if (required == Required.Always || required == Required.AllowNull || customAttributes.OfType<RequiredAttribute>().Any())
                {
                    schema.Required.Add(jsonProperty.PropertyName);
                }
            }

            if (jsonObjectContract.ExtensionDataValueType != null)
            {
                schema.AdditionalProperties = _schemaGenerator.GenerateSchema(jsonObjectContract.ExtensionDataValueType, schemaRepository);
                schema.AdditionalPropertiesAllowed = true;
            }

            // If it's a known subType, reference the baseType for inheritied properties
            if (_generatorOptions.GeneratePolymorphicSchemas && type.BaseType != null && _generatorOptions.SubTypesResolver(type.BaseType).Contains(type))
            {
                var baseType = type.BaseType;

                var baseSchemaReference = schemaRepository.GetOrAdd(
                    type: baseType,
                    schemaId: _generatorOptions.SchemaIdSelector(baseType),
                    factoryMethod: () => CreateObjectSchema(baseType, schemaRepository));

                var baseSchema = schemaRepository.Schemas[baseSchemaReference.Reference.Id];

                schema.AllOf = new[] { baseSchemaReference };

                foreach (var basePropertyName in baseSchema.Properties.Keys)
                {
                    schema.Properties.Remove(basePropertyName);
                }
            }

            return schema;
        }

        private OpenApiSchema CreatePropertySchema(
            JsonProperty jsonProperty,
            IEnumerable<object> customAttributes,
            Required required,
            SchemaRepository schemaRepository)
        {
            var typeSchema = _schemaGenerator.GenerateSchema(jsonProperty.PropertyType, schemaRepository);

            // If it's a referenced/shared schema, "extend" it using allOf so that contextual metadata (e.g. property attributes) can be applied
            var propertySchema = (typeSchema.Reference != null)
                ? new OpenApiSchema { AllOf = new[] { typeSchema } }
                : typeSchema;

            propertySchema.ReadOnly = jsonProperty.Readable && !jsonProperty.Writable;
            propertySchema.WriteOnly = !jsonProperty.Readable && jsonProperty.Writable;
            propertySchema.Nullable = (required == Required.Default || required == Required.AllowNull) && jsonProperty.PropertyType.IsReferenceOrNullableType();

            propertySchema.ApplyCustomAttributes(customAttributes);

            return propertySchema;
        }
    }
}