using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [Theory]
        [InlineData(typeof(object), "object")]
        [InlineData(typeof(JToken), "object")]
        [InlineData(typeof(JObject), "object")]
        [InlineData(typeof(JArray), "array")]
        public void GenerateSchema_GeneratesDynamicSchema_IfDynamicType(
            Type type,
            string expectedType)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal(expectedType, schema.Type);
            Assert.Empty(schema.Properties);
        }

        [Theory]
        [InlineData(typeof(IFormFile))]
        [InlineData(typeof(FileResult))]
        public void GenerateSchema_GeneratesFileSchema_IfFormFileOrFileResultType(Type type)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

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
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedSchemaFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(bool?), "boolean", null)]
        [InlineData(typeof(int?), "integer", "int32")]
        [InlineData(typeof(DateTime?), "string", "date-time")]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<bool>), "boolean", null)]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<int>), "integer", "int32")]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<DateTime>), "string", "date-time")]
        public void GenerateSchema_GeneratesPrimitiveSchema_IfNullableOrFSharpOptionType(
            Type type,
            string expectedSchemaType,
            string expectedSchemaFormat)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedSchemaFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(IntEnum), "IntEnum", "integer", "int32", 3)]
        [InlineData(typeof(LongEnum), "LongEnum", "integer", "int64", 3)]
        [InlineData(typeof(IntEnum?), "IntEnum", "integer", "int32", 3)]
        [InlineData(typeof(IntEnum?), "IntEnum", "integer", "int32", 3)]
        public void GenerateSchema_GeneratesEnumSchema_IfEnumType(
            Type type,
            string expectedSchemaId,
            string expectedSchemaType,
            string expectedSchemaFormat,
            int expectedEnumCount)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedSchemaFormat, schema.Format);
            Assert.NotNull(schema.Enum);
            Assert.Equal(expectedEnumCount, schema.Enum.Count);
        }

        [Theory]
        [InlineData(typeof(IDictionary<string, int>), "integer")]
        [InlineData(typeof(IReadOnlyDictionary<string, bool>), "boolean")]
        [InlineData(typeof(IDictionary), "object")]
        public void GenerateSchema_GeneratesDictionarySchema_IfDictionaryType(
            Type type,
            string expectedAdditionalPropertiesType)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal("object", schema.Type);
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal(expectedAdditionalPropertiesType, schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GenerateSchema_GeneratesObjectSchema_IfDictionaryTypeWithEnumKeys()
        {
            var schema = Subject().GenerateSchema(typeof(IDictionary<IntEnum, int>), new SchemaRepository());

            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Value2", "Value4", "Value8" }, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedDictionarySchema_IfSelfReferencingDictionaryType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(DictionaryOfSelf), schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            Assert.Equal("DictionaryOfSelf", referenceSchema.Reference.Id);
            Assert.True(schemaRepository.Schemas.ContainsKey(referenceSchema.Reference.Id));
        }

        [Theory]
        [InlineData(typeof(int[]), "integer", "int32")]
        [InlineData(typeof(IEnumerable<string>), "string", null)]
        [InlineData(typeof(DateTime?[]), "string", "date-time")]
        public void GenerateSchema_GeneratesArraySchema_IfEnumerableType(
            Type type,
            string expectedItemsType,
            string expectedItemsFormat)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.NotNull(schema.Items);
            Assert.Equal(expectedItemsType, schema.Items.Type);
            Assert.Equal(expectedItemsFormat, schema.Items.Format);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedArraySchema_IfSelfReferencingEnumerableType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ListOfSelf), schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            Assert.Equal("ListOfSelf", referenceSchema.Reference.Id);
            Assert.True(schemaRepository.Schemas.ContainsKey(referenceSchema.Reference.Id));
        }

        [Theory]
        [InlineData(typeof(ISet<string>))]
        [InlineData(typeof(SortedSet<string>))]
        public void Generateschema_SetsUniqueItems_IfSetType(Type type)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.True(schema.UniqueItems);
        }

        [Theory]
        [InlineData(typeof(ComplexType), "ComplexType", new[] { "Property1", "Property2", "Property3", "Property4", "Property5", "Property6", "Property7", "Property8", "Property9" })]
        [InlineData(typeof(ComplexTypeWithFields), "ComplexTypeWithFields", new[] { "Field1", "Field2", "Field3" })]
        [InlineData(typeof(GenericType<bool, int>), "BooleanInt32GenericType", new[] { "Property1", "Property2" })]
        [InlineData(typeof(ContainingType.NestedType), "NestedType", new[] { "Property1" })]
        public void GenerateSchema_GeneratesReferencedObjectSchema_IfComplexType(
            Type type,
            string expectedSchemaId,
            string[] expectedProperties)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

            Assert.Equal(expectedSchemaId, referenceSchema.Reference.Id);
            var schema = schemaRepository.Schemas[expectedSchemaId];
            Assert.Equal("object", schema.Type);
            Assert.Equal(expectedProperties, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SetsReadOnlyAndWriteOnlyFlags_IfPropertyAccessIsRestricted()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ComplexType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(false, schema.Properties["Property3"].ReadOnly);
            Assert.Equal(false, schema.Properties["Property3"].WriteOnly);
            Assert.Equal(true, schema.Properties["Property4"].ReadOnly);
            Assert.Equal(false, schema.Properties["Property4"].WriteOnly);
            Assert.Equal(false, schema.Properties["Property5"].ReadOnly);
            Assert.Equal(true, schema.Properties["Property5"].WriteOnly);
        }

        [Fact]
        public void GenerateSchema_IncludesInheritedProperties_IfDerivedType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(SubType1), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1", "BaseProperty" }, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_ExcludesIndexerProperties_IfIndexedType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(IndexedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1" }, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SetsValidationProperties_IfDataAnnotatedType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(DataAnnotatedType), schemaRepository);

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
        public void GenerateSchema_SetsValidationProperties_IfMetadataAnnotatedType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(MetadataAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(new[] { "IntWithRequired", "StringWithRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerBehavior_IfJsonAnnotatedType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.DoesNotContain("StringWithJsonIgnore", schema.Properties.Keys);
            Assert.Contains("Foobar", schema.Properties.Keys);
            Assert.Equal(new[] { "StringWithJsonPropertyRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void GenerateSchema_SetsAdditionalProperties_IfExtensionDataAnnotatedType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ExtensionDataAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal("object", schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GenerateSchema_HonorsStringEnumConverters_IfConfiguredViaAttributes()
        {
            var schema = Subject().GenerateSchema(typeof(JsonConvertedEnum), new SchemaRepository());

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

            var schema = subject.GenerateSchema(typeof(IntEnum), new SchemaRepository());

            Assert.Equal("string", schema.Type);
            Assert.Equal(expectedEnumValues, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Fact]
        public void GenerateSchema_HandlesMultiDemensionalArrays()
        {
            var schema = Subject().GenerateSchema(typeof(int[][]), new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.Equal("array", schema.Items.Type);
            Assert.Equal("integer", schema.Items.Items.Type);
            Assert.Equal("int32", schema.Items.Items.Format);
        }

        [Fact]
        public void GenerateSchema_HandlesCompositeTypes()
        {

            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(CompositeType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal("ComplexType", schema.Properties["Property1"].Reference.Id);
            Assert.Equal("array", schema.Properties["Property2"].Type);
            Assert.Equal("ComplexType", schema.Properties["Property2"].Items.Reference.Id);
        }

        [Fact]
        public void GenerateSchema_HandlesNestedTypes()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ContainingType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal("NestedType", schema.Properties["Property1"].Reference.Id);
        }

        [Fact]
        public void GenerateSchema_HandlesRecursion_IfCalledAgainWithinAFilter()
        {
            var subject = Subject(c => c.SchemaFilters.Add(new RecursiveCallSchemaFilter()));

            subject.GenerateSchema(typeof(object), new SchemaRepository());
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToCustomizeSchemaForSpecificTypes()
        {
            var subject = Subject(c =>
                c.CustomTypeMappings.Add(typeof(ComplexType), () => new OpenApiSchema { Type = "string" })
            );

            var schema = subject.GenerateSchema(typeof(ComplexType), new SchemaRepository());

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
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(type, schemaRepository);

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
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(ObsoletePropertiesType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.DoesNotContain("ObsoleteProperty", schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToCustomizeReferencedSchemaIds()
        {
            var subject = Subject(c =>
                c.SchemaIdSelector = (type) => type.FullName
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(ComplexType), schemaRepository);

            Assert.Equal("Swashbuckle.AspNetCore.SwaggerGen.Test.ComplexType", referenceSchema.Reference.Id);
            Assert.Contains(referenceSchema.Reference.Id, schemaRepository.Schemas.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToDescribeAllEnumsAsStrings()
        {
            var subject = Subject(c =>
                c.DescribeAllEnumsAsStrings = true
            );

            var schema = subject.GenerateSchema(typeof(IntEnum), new SchemaRepository());

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

            var schema = subject.GenerateSchema(typeof(IntEnum), new SchemaRepository());

            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "value2", "value4", "value8" }, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToUserReferencedDefinitionsForEnums()
        {
            var subject = Subject(c =>
            {
                c.UseReferencedDefinitionsForEnums = true;
            });
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(IntEnum), schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("integer", schema.Type);
            Assert.Equal(new[] { 2, 4, 8 }, schema.Enum.Cast<OpenApiInteger>().Select(i => i.Value));
        }

        [Fact]
        public void GenerateSchema_SupportsOptionToGeneratePolymorphicSchemas()
        {
            var subject = Subject(c =>
            {
                c.GeneratePolymorphicSchemas = true;
            });
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(PolymorphicType), schemaRepository);

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

            subject.GenerateSchema(typeof(Namespace1.ConflictingType), schemaRepository);
            Assert.Throws<InvalidOperationException>(() =>
            {
                subject.GenerateSchema(typeof(Namespace2.ConflictingType), schemaRepository);
            });
        }

        private ISchemaGenerator Subject(
            Action<SchemaGeneratorOptions> configureOptions = null,
            Action<JsonSerializerSettings> configureSerializer = null)
        {

            var serializerSettings = new JsonSerializerSettings();
            configureSerializer?.Invoke(serializerSettings);

            var options = new SchemaGeneratorOptions();
            configureOptions?.Invoke(options);

            return new SchemaGenerator(serializerSettings, options);
        }
    }
}
