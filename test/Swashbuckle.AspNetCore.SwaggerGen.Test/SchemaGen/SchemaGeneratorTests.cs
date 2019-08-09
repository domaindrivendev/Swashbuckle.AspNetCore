using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SchemaGeneratorTests
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public SchemaGeneratorTests()
        {
            _modelMetadataProvider = ModelMetadataHelper.GetDefaultModelMetadataProvider();
        }

        [Theory]
        [InlineData(typeof(object), "object")]
        [InlineData(typeof(JToken), "object")]
        [InlineData(typeof(JObject), "object")]
        [InlineData(typeof(JArray), "array")]
        public void GenerateSchema_GeneratesDynamicSchema_IfDynamicType(
            Type type,
            string expectedType)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal(expectedType, schema.Type);
            Assert.Empty(schema.Properties);
        }

        [Theory]
        [InlineData(typeof(IFormFile))]
        [InlineData(typeof(FileResult))]
        public void GenerateSchema_GeneratesFileSchema_IfFormFileOrFileResultType(Type type)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal("string", schema.Type);
            Assert.Equal("binary", schema.Format);
        }

        [Theory]
        [InlineData(typeof(bool), "boolean", null)]
        [InlineData(typeof(byte), "integer", "int32")]
        [InlineData(typeof(sbyte), "integer", "int32")]
        [InlineData(typeof(short), "integer", "int32")]
        [InlineData(typeof(ushort), "integer", "int32")]
        [InlineData(typeof(int), "integer", "int32")]
        [InlineData(typeof(uint), "integer", "int32")]
        [InlineData(typeof(long), "integer", "int64")]
        [InlineData(typeof(ulong), "integer", "int64")]
        [InlineData(typeof(float), "number", "float")]
        [InlineData(typeof(double), "number", "double")]
        [InlineData(typeof(decimal), "number", "double")]
        [InlineData(typeof(string), "string", null)]
        [InlineData(typeof(char), "string", null)]
        [InlineData(typeof(byte[]), "string", "byte")]
        [InlineData(typeof(DateTime), "string", "date-time")]
        [InlineData(typeof(DateTimeOffset), "string", "date-time")]
        [InlineData(typeof(Guid), "string", "uuid")]
        public void GenerateSchema_GeneratesPrimitiveSchema_IfPrimitiveType(
            Type type,
            string expectedSchemaType,
            string expectedSchemaFormat)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedSchemaFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(string), "string", null)]
        [InlineData(typeof(bool?), "boolean", null)]
        [InlineData(typeof(int?), "integer", "int32")]
        [InlineData(typeof(DateTime?), "string", "date-time")]
        //[InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<bool>), "boolean", null)]
        //[InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<int>), "integer", "int32")]
        //[InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<DateTime>), "string", "date-time")]
        public void GenerateSchema_SetsNullable_IfReferenceOrNullablePrimitiveType(
            Type type,
            string expectedSchemaType,
            string expectedSchemaFormat)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.True(schema.Nullable);
            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedSchemaFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(IntEnum), "integer", "int32", 3)]
        [InlineData(typeof(LongEnum), "integer", "int64", 3)]
        [InlineData(typeof(LongEnum?), "integer", "int64", 3)]
        public void GenerateSchema_GeneratesReferencedEnumSchema_IfEnumType(
            Type type,
            string expectedSchemaType,
            string expectedSchemaFormat,
            int expectedEnumCount)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedSchemaFormat, schema.Format);
            Assert.NotNull(schema.Enum);
            Assert.Equal(expectedEnumCount, schema.Enum.Count);
        }

        [Theory]
        [InlineData(typeof(IntEnum?), "integer", "int32")]
        [InlineData(typeof(LongEnum?), "integer", "int64")]
        public void GenerateSchema_SetsNullable_IfNullableEnumType(
            Type type,
            string expectedSchemaType,
            string expectedSchemaFormat)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.True(schema.Nullable);
            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedSchemaFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(int[]), "integer", "int32")]
        [InlineData(typeof(IEnumerable<string>), "string", null)]
        [InlineData(typeof(DateTime?[]), "string", "date-time")]
        [InlineData(typeof(int[][]), "array", null)]
        public void GenerateSchema_GeneratesArraySchema_IfEnumerableType(
            Type type,
            string expectedItemsType,
            string expectedItemsFormat)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.NotNull(schema.Items);
            Assert.Equal(expectedItemsType, schema.Items.Type);
            Assert.Equal(expectedItemsFormat, schema.Items.Format);
        }

        [Theory]
        [InlineData(typeof(ISet<string>))]
        [InlineData(typeof(SortedSet<string>))]
        public void GenerateSchema_SetsUniqueItems_IfSetType(Type type)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.True(schema.UniqueItems);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedArraySchema_IfSelfReferencingEnumerableType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(ListOfSelf));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            Assert.Equal("ListOfSelf", referenceSchema.Reference.Id);
            Assert.True(schemaRepository.Schemas.ContainsKey(referenceSchema.Reference.Id));
        }

        [Theory]
        [InlineData(typeof(IDictionary<string, int>), "integer")]
        [InlineData(typeof(IReadOnlyDictionary<string, bool>), "boolean")]
        [InlineData(typeof(IDictionary), "object")]
        public void GenerateSchema_GeneratesDictionarySchema_IfDictionaryType(
            Type type,
            string expectedAdditionalPropertiesType)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal("object", schema.Type);
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal(expectedAdditionalPropertiesType, schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GenerateSchema_GeneratesObjectSchema_IfDictionaryTypeWithEnumKeys()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(IDictionary<IntEnum, int>));

            var schema = Subject().GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Value2", "Value4", "Value8" }, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedDictionarySchema_IfSelfReferencingDictionaryType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(DictionaryOfSelf));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            Assert.Equal("DictionaryOfSelf", referenceSchema.Reference.Id);
            Assert.True(schemaRepository.Schemas.ContainsKey(referenceSchema.Reference.Id));
        }

        [Theory]
        [InlineData(typeof(ComplexType), "ComplexType", new[] { "Property1", "Property2", "Property3" })]
        [InlineData(typeof(ComplexTypeWithFields), "ComplexTypeWithFields", new[] { "Field1", "Field2", "Field3" })]
        [InlineData(typeof(GenericType<bool, int>), "BooleanInt32GenericType", new[] { "Property1", "Property2" })]
        [InlineData(typeof(ContainingType.NestedType), "NestedType", new[] { "Property1" })]
        public void GenerateSchema_GeneratesReferencedObjectSchema_IfComplexType(
            Type type,
            string expectedSchemaId,
            string[] expectedProperties)
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(type);
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            Assert.Equal(expectedSchemaId, referenceSchema.Reference.Id);
            var schema = schemaRepository.Schemas[expectedSchemaId];
            Assert.Equal("object", schema.Type);
            Assert.Equal(expectedProperties, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_IncludesInheritedProperties_IfDerivedType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(SubType1));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1", "BaseProperty" }, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_ExcludesIndexerProperties_IfIndexedType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(IndexedType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1" }, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SetsReadOnlyAndWriteOnlyFlags_IfPropertyAccessIsRestricted()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(ComplexType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(false, schema.Properties["Property1"].ReadOnly);
            Assert.Equal(false, schema.Properties["Property1"].WriteOnly);
            Assert.Equal(true, schema.Properties["Property2"].ReadOnly);
            Assert.Equal(false, schema.Properties["Property2"].WriteOnly);
            Assert.Equal(false, schema.Properties["Property3"].ReadOnly);
            Assert.Equal(true, schema.Properties["Property3"].WriteOnly);
        }

        [Fact]
        public void GenerateSchema_SetsValidationProperties_IfDataAnnotatedType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(DataAnnotatedType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(5, schema.Properties["StringWithStringLength"].MinLength);
            Assert.Equal(10, schema.Properties["StringWithStringLength"].MaxLength);
            Assert.Equal(1, schema.Properties["StringWithMinMaxLength"].MinLength);
            Assert.Equal(3, schema.Properties["StringWithMinMaxLength"].MaxLength);
            Assert.Equal(new[] { "IntWithRequired", "StringWithRequired" }, schema.Required.ToArray());
            Assert.Equal("date", schema.Properties["StringWithDataTypeDate"].Format);
            Assert.Equal("date-time", schema.Properties["StringWithDataTypeDateTime"].Format);
            Assert.Equal("password", schema.Properties["StringWithDataTypePassword"].Format);
            Assert.IsType<OpenApiString>(schema.Properties["StringWithDefaultValue"].Default);
            Assert.Equal("foobar", ((OpenApiString)schema.Properties["StringWithDefaultValue"].Default).Value);
        }

        [Fact]
        public void GenerateSchema_SetsValidationProperties_IfDataAnnotatedViaMetadataType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(DataAnnotatedViaMetadataType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(new[] { "IntWithRequired", "StringWithRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerBehavior_IfJsonAnnotatedType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(JsonAnnotatedType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.DoesNotContain("StringWithJsonIgnore", schema.Properties.Keys);
            Assert.Contains("Foobar", schema.Properties.Keys);
            Assert.Equal(new[] { "StringWithJsonPropertyRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void GenerateSchema_SetsAdditionalProperties_IfExtensionDataAnnotatedType()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(ExtensionDataAnnotatedType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal("object", schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GenerateSchema_HonorsStringEnumConverters_IfConfiguredViaAttributes()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(JsonConvertedEnum));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "Value1", "Value2", "X" }, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Theory]
        [InlineData(false, new[] { "Value2", "Value4", "Value8" })]
        [InlineData(true, new[] { "value2", "value4", "value8" })]
        public void GenerateSchema_HonorsStringEnumConverters_IfConfiguredGlobally(bool camelCaseText, string[] expectedEnumValues)
        {
            var subject = Subject(configureSerializer: c =>
                c.Converters = new[] { new StringEnumConverter(camelCaseText) }
            );
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(IntEnum));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(expectedEnumValues, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Fact]
        public void GenerateSchema_HandlesCompositeTypes()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(CompositeType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal("ComplexType", schema.Properties["Property1"].Reference.Id);
            Assert.Equal("array", schema.Properties["Property2"].Type);
            Assert.Equal("ComplexType", schema.Properties["Property2"].Items.Reference.Id);
        }

        [Fact]
        public void GenerateSchema_HandlesNestedTypes()
        {
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(ContainingType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal("NestedType", schema.Properties["Property1"].Reference.Id);
        }

        [Fact]
        public void GenerateSchema_HandlesRecursion_IfCalledAgainWithinAFilter()
        {
            var subject = Subject(c => c.SchemaFilters.Add(new RecursiveCallSchemaFilter()));
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(object));

            subject.GenerateSchema(modelMetadata, new SchemaRepository());
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToCustomizeSchemaForSpecificTypes()
        {
            var subject = Subject(c =>
                c.CustomTypeMappings.Add(typeof(CompositeType), () => new OpenApiSchema { Type = "string" })
            );
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(CompositeType));

            var schema = subject.GenerateSchema(modelMetadata, new SchemaRepository());

            Assert.Equal("string", schema.Type);
            Assert.Empty(schema.Properties);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(IDictionary<string, string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(ComplexType))]
        public void GenerateSchema_SupportsOptionToPostProcessGeneratedSchemas(Type type)
        {
            var subject = Subject(c =>
                c.SchemaFilters.Add(new VendorExtensionsSchemaFilter())
            );
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(CompositeType));
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(modelMetadata, schemaRepository);

            if (schema.Reference == null)
                Assert.Contains("X-property1", schema.Extensions.Keys);
            else
                Assert.Contains("X-property1", schemaRepository.Schemas[schema.Reference.Id].Extensions.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOptionstoIgnoreObsoleteProperties()
        {
            var subject = Subject(c =>
                c.IgnoreObsoleteProperties = true
            );
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(ObsoletePropertiesType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.DoesNotContain("ObsoleteProperty", schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToCustomizeReferencedSchemaIds()
        {
            var subject = Subject(c =>
                c.SchemaIdSelector = (type) => type.FullName
            );
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(ComplexType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(modelMetadata, schemaRepository);

            Assert.Equal("Swashbuckle.AspNetCore.SwaggerGen.Test.ComplexType", referenceSchema.Reference.Id);
            Assert.Contains(referenceSchema.Reference.Id, schemaRepository.Schemas.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToDescribeAllEnumsAsStrings()
        {
            var subject = Subject(c =>
                c.DescribeAllEnumsAsStrings = true
            );
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(IntEnum));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "Value2", "Value4", "Value8" }, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToDescribeStringEnumsInCamelCase()
        {
            var subject = Subject(c =>
            {
                c.DescribeAllEnumsAsStrings = true;
                c.DescribeStringEnumsInCamelCase = true;
            });
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(IntEnum));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "value2", "value4", "value8" }, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToGeneratePolymorphicSchemas()
        {
            var subject = Subject(c =>
            {
                c.GeneratePolymorphicSchemas = true;
            });
            var modelMetadata = _modelMetadataProvider.GetMetadataForType(typeof(PolymorphicType));
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(modelMetadata, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.NotNull(schema.OneOf);
            Assert.Equal(2, schema.OneOf.Count);
            Assert.NotNull(schema.OneOf[0].Reference);
            Assert.Equal("SubType1", schema.OneOf[0].Reference.Id);
            Assert.Equal(new[] { "Property1", "BaseProperty" }, schemaRepository.Schemas["SubType1"].Properties.Keys);
            Assert.NotNull(schema.OneOf[1].Reference);
            Assert.Equal("SubType2", schema.OneOf[1].Reference.Id);
            Assert.Equal(new[] { "Property2", "BaseProperty" }, schemaRepository.Schemas["SubType2"].Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_Errors_IfTypesHaveConflictingSchemaIds()
        {
            var subject = Subject();
            var schemaRepository = new SchemaRepository();

            subject.GenerateSchema(_modelMetadataProvider.GetMetadataForType(typeof(Namespace1.ConflictingType)), schemaRepository);
            Assert.Throws<InvalidOperationException>(() =>
            {
                subject.GenerateSchema(_modelMetadataProvider.GetMetadataForType(typeof(Namespace2.ConflictingType)), schemaRepository);
            });
        }

        private ISchemaGenerator Subject(
            Action<SchemaGeneratorOptions> configureOptions = null,
            Action<JsonSerializerSettings> configureSerializer = null)
        {
            var schemaGeneratorOptions = new SchemaGeneratorOptions();
            configureOptions?.Invoke(schemaGeneratorOptions);

            var jsonSerializerSettings = new JsonSerializerSettings();
            configureSerializer?.Invoke(jsonSerializerSettings);

            return new SchemaGenerator(schemaGeneratorOptions, jsonSerializerSettings);
        }
    }
}
