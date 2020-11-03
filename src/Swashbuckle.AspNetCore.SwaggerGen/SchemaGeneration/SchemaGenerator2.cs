using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator2 : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _generatorOptions;

        public SchemaGenerator2(SchemaGeneratorOptions generatorOptions)
        {
            _generatorOptions = generatorOptions;
        }

        public OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null)
        {
            var dataContract = GetDataContractFor(type);

            var schema = IsPolymorphicDataContract(dataContract, out IEnumerable<DataContract> knownTypesDataContracts)
                ? CreatePolymorphicSchema(dataContract, knownTypesDataContracts, schemaRepository)
                : CreateConcreteSchema(dataContract, schemaRepository);

            if (memberInfo != null)
            {
                ApplyMemberMetadata(schema, memberInfo, dataContract);
            }
            else if (parameterInfo != null)
            {
                ApplyParameterMetadata(schema, parameterInfo, dataContract);
            }

            ApplyFilters(schema, dataContract, schemaRepository);

            return schema;
        }

        private DataContract GetDataContractFor(Type type)
        {
            var effectiveType = Nullable.GetUnderlyingType(type) ?? type;

            foreach (var dataContractResolver in _generatorOptions.DataContractResolvers)
            {
                if (dataContractResolver.CanResolveContractFor(effectiveType))
                    return dataContractResolver.ResolveContractFor(effectiveType);
            }

            return DataContract.Undefined(effectiveType);
        }

        private bool IsPolymorphicDataContract(DataContract dataContract, out IEnumerable<DataContract> knownTypesDataContracts)
        {
            var knownTypes = _generatorOptions.UseOneOfForPolymorphism
                ? _generatorOptions.KnownTypesSelector(dataContract.UnderlyingType)
                : Enumerable.Empty<Type>();

            knownTypesDataContracts = knownTypes
                .Select(knownType => GetDataContractFor(knownType));

            return knownTypesDataContracts.Any();
        }

        private OpenApiSchema CreatePolymorphicSchema(
            DataContract dataContract,
            IEnumerable<DataContract> knownTypesDataContracts,
            SchemaRepository schemaRepository)
        {
            var schema = new OpenApiSchema
            {
                OneOf = new List<OpenApiSchema>(),
                Discriminator = (dataContract.ObjectDiscriminatorProperty != null)
                    ? new OpenApiDiscriminator { PropertyName = dataContract.ObjectDiscriminatorProperty }
                    : null
            };

            foreach (var knownTypeDataContract in knownTypesDataContracts)
            {
                var knownTypeSchema = CreateConcreteSchema(knownTypeDataContract, schemaRepository);

                schema.OneOf.Add(knownTypeSchema);

                if (dataContract.ObjectDiscriminatorValue != null)
                {
                    schema.Discriminator.Mapping.Add(dataContract.ObjectDiscriminatorValue, knownTypeSchema.Reference.ReferenceV3);
                }
            }

            return schema;
        }

        private OpenApiSchema CreateConcreteSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            Func<OpenApiSchema> schemaFactory;
            bool returnAsReference;

            switch (dataContract.DataType)
            {
                case DataType.Boolean:
                case DataType.Integer:
                case DataType.Number:
                case DataType.String:
                    {
                        schemaFactory = () => CreatePrimitiveSchema(dataContract);
                        returnAsReference = dataContract.UnderlyingType.IsEnum && !_generatorOptions.UseInlineDefinitionsForEnums;
                        break;
                    }

                case DataType.Dictionary:
                    {
                        schemaFactory = () => CreateDictionarySchema(dataContract, schemaRepository);
                        returnAsReference = dataContract.DictionaryValueType == dataContract.UnderlyingType;
                        break;
                    }

                case DataType.Array:
                    {
                        schemaFactory = () => CreateArraySchema(dataContract, schemaRepository);
                        returnAsReference = dataContract.ArrayItemType == dataContract.UnderlyingType;
                        break;
                    }

                case DataType.Object:
                    {
                        schemaFactory = () => CreateObjectSchema(dataContract, schemaRepository);
                        returnAsReference = true;
                        break;
                    }

                default:
                    {
                        schemaFactory = () => new OpenApiSchema();
                        returnAsReference = false;
                        break;
                    }
            }

            if (returnAsReference)
            {
                return schemaRepository.GetOrAdd(
                    type: dataContract.UnderlyingType,
                    schemaIdSelector: _generatorOptions.SchemaIdSelector,
                    schemaFactory: () =>
                    {
                        var schema = schemaFactory();
                        ApplyFilters(schema, dataContract, schemaRepository);
                        return schema;
                    });
            }

            return schemaFactory();
        }

        private OpenApiSchema CreatePrimitiveSchema(DataContract dataContract)
        {
            var schema = new OpenApiSchema
            {
                Type = dataContract.DataType.ToString().ToLower(CultureInfo.InvariantCulture),
                Format = dataContract.DataFormat
            };

            if (dataContract.UnderlyingType.IsEnum)
            {
                schema.Enum = dataContract.UnderlyingType.GetEnumValues()
                    .Cast<object>()
                    .Distinct()
                    .Select(value => dataContract.JsonConverter(value))
                    .Select(valueAsJson => OpenApiAnyFactory.CreateFromJson(valueAsJson))
                    .ToList();
            }

            return schema;
        }

        private OpenApiSchema CreateArraySchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "array",
                Items = GenerateSchema(dataContract.ArrayItemType, schemaRepository),
                UniqueItems = dataContract.UnderlyingType.IsSet() ? (bool?)true : null
            };
        }

        private OpenApiSchema CreateDictionarySchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            if (dataContract.DictionaryKeyType.IsEnum)
            {
                // This is a special case where the set of key values is known
                var enumValuesAsjson = dataContract.DictionaryKeyType.GetEnumValues()
                    .Cast<object>()
                    .Select(value => dataContract.JsonConverter(value));

                var propertyNames = (enumValuesAsjson.Any() && enumValuesAsjson.First().StartsWith("\""))
                    ? enumValuesAsjson.Select(value => value.Replace("\"", string.Empty))
                    : dataContract.DictionaryKeyType.GetEnumNames();

                return new OpenApiSchema
                {
                    Type = "object",
                    Properties = propertyNames.ToDictionary(name => name, name => GenerateSchema(dataContract.DictionaryValueType, schemaRepository)),
                    AdditionalPropertiesAllowed = false,
                };
            }
            else
            {
                return new OpenApiSchema
                {
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                    AdditionalProperties = GenerateSchema(dataContract.DictionaryValueType, schemaRepository)
                };
            }
        }

        private OpenApiSchema CreateObjectSchema(DataContract dataContract, SchemaRepository schemaRepository)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false,
            };

            var applicableDataProperties = dataContract.ObjectProperties
                .SkipWhile(dp => _generatorOptions.IgnoreObsoleteProperties && dp.MemberInfo.HasCustomAttribute<ObsoleteAttribute>());

            if (_generatorOptions.UseAllOfForInheritance && dataContract.UnderlyingType.BaseType != null)
            {
                var inheritedDataProperties = applicableDataProperties
                    .Where(dp => dp.MemberInfo.DeclaringType != dataContract.UnderlyingType);

                if (inheritedDataProperties.Any())
                {
                    var baseTypeDataContract = GetDataContractFor(dataContract.UnderlyingType.BaseType);
                    schema.AllOf.Add(CreateConcreteSchema(baseTypeDataContract, schemaRepository));
                }

                applicableDataProperties = applicableDataProperties.Except(inheritedDataProperties);
            }

            foreach (var dataProperty in applicableDataProperties)
            {
                var memberType = dataProperty.MemberInfo.MemberType == MemberTypes.Field
                    ? ((FieldInfo)dataProperty.MemberInfo).FieldType
                    : ((PropertyInfo)dataProperty.MemberInfo).PropertyType;

                var propertySchema = GenerateSchema(memberType, schemaRepository, memberInfo: dataProperty.MemberInfo);

                // TODO:
                if (propertySchema.Reference == null)
                {
                    //propertySchema.Nullable = dataProperty.IsNullable;
                }

                schema.Properties.Add(dataProperty.Name, propertySchema);

                if (dataProperty.MemberInfo.HasCustomOrMetadataAttribute<RequiredAttribute>())
                {
                    schema.Required.Add(dataProperty.Name);
                }
            }

            return schema;
        }

        private void ApplyMemberMetadata(OpenApiSchema schema, MemberInfo memberInfo, DataContract dataContract)
        {
            if (_generatorOptions.UseAllOfToExtendReferenceSchemas && schema.Reference != null)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }

            if (schema.Reference != null) return;

            if (memberInfo is PropertyInfo propertyInfo)
            {
                schema.Nullable = propertyInfo.IsContextuallyNullable();
                schema.ReadOnly = propertyInfo.IsPubliclyReadable() && !propertyInfo.IsPubliclyWritable();
                schema.WriteOnly = propertyInfo.IsPubliclyWritable() && !propertyInfo.IsPubliclyReadable();
            }

            var defaultValueAttribute = memberInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null)
            {
                var defaultAsJson = dataContract.JsonConverter(defaultValueAttribute.Value);
                schema.Default = OpenApiAnyFactory.CreateFromJson(defaultAsJson);
            }

            if (memberInfo.GetCustomAttribute<ObsoleteAttribute>() != null)
            {
                schema.Deprecated = true;
            }

            var validationAttributes = memberInfo.GetCustomOrMetadataAttributes<ValidationAttribute>();
            schema.ApplyValidationAttributes(validationAttributes);
        }

        private void ApplyParameterMetadata(OpenApiSchema schema, ParameterInfo parameterInfo, DataContract dataContract)
        {
            if (_generatorOptions.UseAllOfToExtendReferenceSchemas && schema.Reference != null)
            {
                schema.AllOf = new[] { new OpenApiSchema { Reference = schema.Reference } };
                schema.Reference = null;
            }
            if (parameterInfo.HasDefaultValue)
            {
                var defaultAsJson = dataContract.JsonConverter(parameterInfo.DefaultValue);
                schema.Default = OpenApiAnyFactory.CreateFromJson(defaultAsJson);
            }

            var defaultValueAttribute = parameterInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null)
            {
                var defaultAsJson = dataContract.JsonConverter(defaultValueAttribute.Value);
                schema.Default = OpenApiAnyFactory.CreateFromJson(defaultAsJson);
            }

            var validationAttributes = parameterInfo.GetCustomAttributes<ValidationAttribute>();
            schema.ApplyValidationAttributes(validationAttributes);
        }

        private void ApplyFilters(OpenApiSchema schema, DataContract dataContract, SchemaRepository schemaRepository)
        {
            var filterContext = new SchemaFilterContext(dataContract.UnderlyingType, this, schemaRepository);
            foreach (var filter in _generatorOptions.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }
        }
    }
}
