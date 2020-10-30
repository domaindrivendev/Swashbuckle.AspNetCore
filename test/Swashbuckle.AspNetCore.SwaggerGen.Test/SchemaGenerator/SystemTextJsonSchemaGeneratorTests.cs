using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;
using Swashbuckle.AspNetCore.TestSupport.Fixtures;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SystemTextJsonSchemaGeneratorTests
    {
        [Theory]
        [InlineData(typeof(IFormFile))]
        [InlineData(typeof(FileResult))]
        public void GeneratesFileSchema_IfFormFileOrFileResultType(Type type)
        {
            var schema = ConfiguredSchemaRepository().GetTypeSchema(type);

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
        [InlineData(typeof(Uri), "string", "uri")]
        [InlineData(typeof(bool?), "boolean", null)]
        [InlineData(typeof(int?), "integer", "int32")]
        [InlineData(typeof(DateTime?), "string", "date-time")]
        [InlineData(typeof(Guid?), "string", "uuid")]
        public void GeneratesPrimitiveSchema_IfPrimitiveOrNullablePrimitiveType(
            Type type,
            string expectedSchemaType,
            string expectedFormat)
        {
            var schema = ConfiguredSchemaRepository().GetTypeSchema(type);

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(IntEnum), "integer", "int32", "2", "4", "8")]
        [InlineData(typeof(LongEnum), "integer", "int64", "2", "4", "8")]
        [InlineData(typeof(IntEnum?), "integer", "int32", "2", "4", "8")]
        [InlineData(typeof(LongEnum?), "integer", "int64", "2", "4", "8")]
        public void GeneratesReferencedEnumSchema_IfEnumOrNullableEnumType(
            Type type,
            string expectedSchemaType,
            string expectedFormat,
            params string[] expectedEnumAsJson)
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(type);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
            Assert.NotNull(schema.Enum);
            Assert.Equal(expectedEnumAsJson, schema.Enum.Select(openApiAny => openApiAny.ToJson()));
        }

        [Fact]
        public void DedupsEnumValues_IfEnumTypeHasDuplicateValues()
        {
            var enumType = typeof(HttpStatusCode);
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(enumType);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(enumType.GetEnumValues().Cast<HttpStatusCode>().Distinct().Count(), schema.Enum.Count);
        }

        [Theory]
        [InlineData(typeof(IDictionary<string, int>), "integer")]
        [InlineData(typeof(IReadOnlyDictionary<string, bool>), "boolean")]
        [InlineData(typeof(IDictionary), null)]
        [InlineData(typeof(ExpandoObject), null)]
        public void GeneratesDictionarySchema_IfDictionaryType(
            Type type,
            string expectedAdditionalPropertiesType)
        {
            var schema = ConfiguredSchemaRepository().GetTypeSchema(type);

            Assert.Equal("object", schema.Type);
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal(expectedAdditionalPropertiesType, schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GeneratesObjectSchema_IfDictionaryTypeHasEnumKey()
        {
            var schema = ConfiguredSchemaRepository().GetTypeSchema(typeof(IDictionary<IntEnum, int>));

            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Value2", "Value4", "Value8" }, schema.Properties.Keys);
        }

        [Fact]
        public void GeneratesReferencedDictionarySchema_IfDictionaryTypeIsSelfReferencing()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(DictionaryOfSelf));

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal(schema.AdditionalProperties.Reference.Id, referenceSchema.Reference.Id); // ref to self
        }

        [Theory]
        [InlineData(typeof(int[]), "integer", "int32")]
        [InlineData(typeof(IEnumerable<string>), "string", null)]
        [InlineData(typeof(DateTime?[]), "string", "date-time")]
        [InlineData(typeof(int[][]), "array", null)]
        [InlineData(typeof(IList), null, null)]
        public void GeneratesArraySchema_IfEnumerableType(
            Type type,
            string expectedItemsType,
            string expectedItemsFormat)
        {
            var schema = ConfiguredSchemaRepository().GetTypeSchema(type);

            Assert.Equal("array", schema.Type);
            Assert.NotNull(schema.Items);
            Assert.Equal(expectedItemsType, schema.Items.Type);
            Assert.Equal(expectedItemsFormat, schema.Items.Format);
        }

        [Theory]
        [InlineData(typeof(ISet<string>))]
        [InlineData(typeof(SortedSet<string>))]
        public void SetsUniqueItems_IfEnumerableTypeIsASet(Type type)
        {
            var schema = ConfiguredSchemaRepository().GetTypeSchema(type);

            Assert.Equal("array", schema.Type);
            Assert.True(schema.UniqueItems);
        }

        [Fact]
        public void GeneratesReferencedArraySchema_IfEnumerableTypeIsSelfReferencing()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(ListOfSelf));

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("array", schema.Type);
            Assert.Equal(schema.Items.Reference.Id, referenceSchema.Reference.Id); // ref to self
        }

        [Theory]
        [InlineData(typeof(ComplexType), "ComplexType", new[] { "Property1", "Property2" })]
        [InlineData(typeof(GenericType<bool, int>), "BooleanInt32GenericType", new[] { "Property1", "Property2" })]
        [InlineData(typeof(GenericType<bool, int[]>), "BooleanInt32ArrayGenericType", new[] { "Property1", "Property2" })]
        [InlineData(typeof(ContainingType.NestedType), "NestedType", new[] { "Property2" })]
        public void GeneratesReferencedObjectSchema_IfComplexType(
            Type type,
            string expectedSchemaId,
            string[] expectedProperties)
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(type);

            Assert.NotNull(referenceSchema.Reference);
            Assert.Equal(expectedSchemaId, referenceSchema.Reference.Id);
            var schema = schemaRepository.Schemas[expectedSchemaId];
            Assert.Equal("object", schema.Type);
            Assert.Equal(expectedProperties, schema.Properties.Keys);
            Assert.False(schema.AdditionalPropertiesAllowed);
        }

        [Fact]
        public void IncludesInheritedProperties_IfComplexTypeIsDerived()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(SubType1));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "BaseProperty", "Property1" }, schema.Properties.Keys);
        }

        [Fact]
        public void ExcludesIndexerProperties_IfComplexTypeIsIndexed()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(IndexedType));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1" }, schema.Properties.Keys);
        }

        [Theory]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.IntProperty), false)]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.StringProperty), true)]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.NullableIntProperty), true)]
        public void SetsNullableFlag_IfPropertyIsReferenceOrNullableType(
            Type declaringType,
            string propertyName,
            bool expectedNullable)
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(declaringType);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(expectedNullable, schema.Properties[propertyName].Nullable);
        }

        [Theory]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.BoolWithDefault), "true")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.IntWithDefault), "2147483647")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.LongWithDefault), "9223372036854775807")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.FloatWithDefault), "3.4028235E+38")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.DoubleWithDefault), "1.7976931348623157E+308")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.StringWithDefault), "\"foobar\"")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.IntArrayWithDefault), "[\n  1,\n  2,\n  3\n]")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.StringArrayWithDefault), "[\n  \"foo\",\n  \"bar\"\n]")]
        [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.StringWithDefaultNull), "null")]
        public void SetsDefault_IfPropertyHasDefaultValueAttribute(
            Type declaringType,
            string propertyName,
            string expectedDefaultAsJson)
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(declaringType);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            var propertySchema = schema.Properties[propertyName];
            Assert.NotNull(propertySchema.Default);
            Assert.Equal(expectedDefaultAsJson, propertySchema.Default.ToJson());
        }

        [Fact]
        public void SetsDeprecatedFlag_IfPropertyHasObsoleteAttribute()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(TypeWithObsoleteAttribute));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.True(schema.Properties["ObsoleteProperty"].Deprecated);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType))]

        public void SetsValidationProperties_IfComplexTypeHasValidationAttributes(Type type)
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(type);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("credit-card", schema.Properties["StringWithDataTypeCreditCard"].Format);
            Assert.Equal(1, schema.Properties["StringWithMinMaxLength"].MinLength);
            Assert.Equal(3, schema.Properties["StringWithMinMaxLength"].MaxLength);
            Assert.Equal(1, schema.Properties["ArrayWithMinMaxLength"].MinItems);
            Assert.Equal(3, schema.Properties["ArrayWithMinMaxLength"].MaxItems);
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(10, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(5, schema.Properties["StringWithStringLength"].MinLength);
            Assert.Equal(10, schema.Properties["StringWithStringLength"].MaxLength);
            Assert.Equal(new[] { "StringWithRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void SetsReadOnlyAndWriteOnlyFlags_IfPropertyIsRestricted()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(TypeWithRestrictedProperties));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.False(schema.Properties["ReadWriteProperty"].ReadOnly);
            Assert.False(schema.Properties["ReadWriteProperty"].WriteOnly);
            Assert.True(schema.Properties["ReadOnlyProperty"].ReadOnly);
            Assert.False(schema.Properties["ReadOnlyProperty"].WriteOnly);
            Assert.False(schema.Properties["WriteOnlyProperty"].ReadOnly);
            Assert.True(schema.Properties["WriteOnlyProperty"].WriteOnly);
        }

        [Theory]
        [InlineData(typeof(ComplexType), typeof(ComplexType), "string")]
        [InlineData(typeof(GenericType<int, string>), typeof(GenericType<int, string>), "string")]
        [InlineData(typeof(GenericType<,>), typeof(GenericType<int, int>), "string")]
        public void SupportsOption_CustomTypeMappings(
            Type mappingType,
            Type type,
            string expectedSchemaType)
        {
            var subject = ConfiguredSchemaRepository(
                #pragma warning disable 0618
                configureGenerator: c => c.CustomTypeMappings.Add(mappingType, () => new OpenApiSchema { Type = "string" })
                #pragma warning restore 0618
            );
            var schema = subject.GetTypeSchema(type);

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Empty(schema.Properties);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(IDictionary<string, string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(ComplexType))]
        public void SupportsOption_SchemaFilters(Type type)
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: (c) => c.SchemaFilters.Add(new VendorExtensionsSchemaFilter())
            );

            var schema = schemaRepository.GetTypeSchema(type);

            if (schema.Reference == null)
                Assert.Contains("X-foo", schema.Extensions.Keys);
            else
                Assert.Contains("X-foo", schemaRepository.Schemas[schema.Reference.Id].Extensions.Keys);
        }

        [Fact]
        public void SupportsOption_IgnoreObsoleteProperties()
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: c => c.IgnoreObsoleteProperties = true
            );

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(TypeWithObsoleteAttribute));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.DoesNotContain("ObsoleteProperty", schema.Properties.Keys);
        }

        [Fact]
        public void SupportsOption_SchemaIdSelector()
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: c => c.SchemaIdSelector = (type) => type.FullName
            );

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(ComplexType));

            Assert.Equal("Swashbuckle.AspNetCore.TestSupport.ComplexType", referenceSchema.Reference.Id);
            Assert.Contains(referenceSchema.Reference.Id, schemaRepository.Schemas.Keys);
        }

        [Fact]
        public void SupportsOption_UseAllOfForInheritance()
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: c => c.UseAllOfForInheritance = true
            );

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(BaseType));

            // The base type schema
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "BaseProperty" }, schema.Properties.Keys);
            // The first sub type schema
            var subType1Schema = schemaRepository.Schemas["SubType1"];
            Assert.Equal("object", subType1Schema.Type);
            Assert.NotNull(subType1Schema.AllOf);
            Assert.Equal(1, subType1Schema.AllOf.Count);
            Assert.NotNull(subType1Schema.AllOf[0].Reference);
            Assert.Equal(referenceSchema.Reference.Id, subType1Schema.AllOf[0].Reference.Id);
            Assert.Equal(new[] { "Property1" }, subType1Schema.Properties.Keys);
            // The second sub type schema
            var subType2Schema = schemaRepository.Schemas["SubType2"];
            Assert.Equal("object", subType2Schema.Type);
            Assert.NotNull(subType2Schema.AllOf);
            Assert.Equal(1, subType2Schema.AllOf.Count);
            Assert.NotNull(subType2Schema.AllOf[0].Reference);
            Assert.Equal(referenceSchema.Reference.Id, subType2Schema.AllOf[0].Reference.Id);
            Assert.Equal(new[] { "Property2" }, subType2Schema.Properties.Keys);
        }

        [Fact]
        public void SupportsOption_UseOneOfForPolymorphism()
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: c => c.UseOneOfForPolymorphism = true
            );

            var schema = schemaRepository.GetTypeSchema(typeof(BaseType));

            // The polymorphic schema
            Assert.NotNull(schema.OneOf);
            Assert.Equal(3, schema.OneOf.Count);
            // The base type schema
            Assert.NotNull(schema.OneOf[0].Reference);
            var baseTypeSchema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Equal("object", baseTypeSchema.Type);
            Assert.Equal(new[] { "BaseProperty" }, baseTypeSchema.Properties.Keys);
            // The first sub type schema
            Assert.NotNull(schema.OneOf[1].Reference);
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Equal("object", subType1Schema.Type);
            Assert.Equal(new[] { "BaseProperty", "Property1" }, subType1Schema.Properties.Keys);
            // The second sub type schema
            Assert.NotNull(schema.OneOf[2].Reference);
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[2].Reference.Id];
            Assert.Equal("object", subType2Schema.Type);
            Assert.Equal(new[] { "BaseProperty", "Property2" }, subType2Schema.Properties.Keys);
        }

        [Fact]
        public void SupportsOption_UseOneOfForInheritance_CombinedWith_UseAllOfForPolymorphism()
        {
            var schemaRepository = ConfiguredSchemaRepository(configureGenerator: c =>
            {
                c.UseAllOfForInheritance = true;
                c.UseOneOfForPolymorphism = true;
            });

            var schema = schemaRepository.GetTypeSchema(typeof(BaseType));

            // The polymorphic schema
            Assert.NotNull(schema.OneOf);
            Assert.Equal(3, schema.OneOf.Count);
            // The base type schema
            Assert.NotNull(schema.OneOf[0].Reference);
            var baseSchema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Equal("object", baseSchema.Type);
            Assert.Equal(new[] { "BaseProperty"}, baseSchema.Properties.Keys);
            // The first sub type schema
            Assert.NotNull(schema.OneOf[1].Reference);
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Equal("object", subType1Schema.Type);
            Assert.NotNull(subType1Schema.AllOf);
            Assert.Equal(1, subType1Schema.AllOf.Count);
            Assert.NotNull(subType1Schema.AllOf[0].Reference);
            Assert.Equal("BaseType", subType1Schema.AllOf[0].Reference.Id);
            Assert.Equal(new[] { "Property1" }, subType1Schema.Properties.Keys);
            // The second sub type schema
            Assert.NotNull(schema.OneOf[2].Reference);
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[2].Reference.Id];
            Assert.Equal("object", subType2Schema.Type);
            Assert.NotNull(subType2Schema.AllOf);
            Assert.Equal(1, subType2Schema.AllOf.Count);
            Assert.NotNull(subType2Schema.AllOf[0].Reference);
            Assert.Equal("BaseType", subType2Schema.AllOf[0].Reference.Id);
            Assert.Equal(new[] { "Property2" }, subType2Schema.Properties.Keys);
        }

        [Fact]
        public void SupportsOption_SubTypesResolver()
        {
            var subject = ConfiguredSchemaRepository(configureGenerator: c =>
            {
                c.UseOneOfForPolymorphism = true;
                c.SubTypesSelector = (type) => new[] { typeof(SubType1) };
            });

            var schema = subject.GetTypeSchema(typeof(BaseType));

            // The polymorphic schema
            Assert.NotNull(schema.OneOf);
            Assert.Equal(2, schema.OneOf.Count);
        }

        [Fact]
        public void SupportsOption_DiscriminatorNameSelector()
        {
            var schemaRepository = ConfiguredSchemaRepository(configureGenerator: c =>
            {
                c.UseOneOfForPolymorphism = true;
                c.DiscriminatorNameSelector = (baseType) => "TypeName";
                c.DiscriminatorValueSelector = (subType) => subType.Name;
            });

            var schema = schemaRepository.GetTypeSchema(typeof(BaseType));

            // The polymorphic schema
            Assert.NotNull(schema.Discriminator);
            Assert.Equal("TypeName", schema.Discriminator.PropertyName);
            Assert.Equal(
                new Dictionary<string, string>
                {
                    ["BaseType"] = "#/components/schemas/BaseType",
                    ["SubType1"] = "#/components/schemas/SubType1",
                    ["SubType2"] = "#/components/schemas/SubType2"
                },
                schema.Discriminator.Mapping);
            // The base type schema
            var baseTypeSchema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Contains("TypeName", baseTypeSchema.Properties.Keys);
            Assert.Contains("TypeName", baseTypeSchema.Required);
            // The first sub type schema
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Contains("TypeName", subType1Schema.Properties.Keys);
            Assert.Contains("TypeName", subType1Schema.Required);
            // The second sub type schema
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[2].Reference.Id];
            Assert.Contains("TypeName", subType2Schema.Properties.Keys);
            Assert.Contains("TypeName", subType2Schema.Required);
        }

        [Fact]
        public void SupportsOption_UseAllOfToExtendReferenceSchemas()
        {
            var subject = ConfiguredSchemaRepository(
                configureGenerator: c => c.UseAllOfToExtendReferenceSchemas = true
            );
            var propertyInfo = typeof(SelfReferencingType).GetProperty(nameof(SelfReferencingType.Another));

            var schema = subject.GetMemberSchema(propertyInfo);

            Assert.Null(schema.Reference);
            Assert.NotNull(schema.AllOf);
            Assert.Equal(1, schema.AllOf.Count);
            Assert.True(schema.Nullable);
        }

        [Fact]
        public void SupportsOption_UseInlineDefinitionsForEnums()
        {
            var subject = ConfiguredSchemaRepository(
                configureGenerator: c => c.UseInlineDefinitionsForEnums = true
            );

            var schema = subject.GetTypeSchema(typeof(IntEnum));

            Assert.Equal("integer", schema.Type);
            Assert.NotNull(schema.Enum);
        }

        [Fact]
        public void HandlesTypesWithNestedTypes()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(ContainingType));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal("NestedType", schema.Properties["Property1"].Reference.Id);
        }

        [Fact]
        public void HandlesRecursion_IfCalledAgainWithinAFilter()
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: c => c.SchemaFilters.Add(new RecursiveCallSchemaFilter())
            );

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(ComplexType));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("ComplexType", schema.Properties["Self"].Reference.Id);
        }

        [Fact]
        public void Errors_IfTypesHaveConflictingSchemaIds()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            schemaRepository.GetTypeSchema(typeof(TestSupport.Namespace1.ConflictingType));
            Assert.ThrowsAny<Exception>(() =>
            {
                schemaRepository.GetTypeSchema(typeof(TestSupport.Namespace2.ConflictingType));
            });
        }

        [Fact]
        public void HonorsSerializerOption_IgnoreReadonlyProperties()
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: c => { },
                configureSerializer: c => { c.IgnoreReadOnlyProperties = true; }
            );

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(ComplexType));

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        }

        [Fact]
        public void HonorsSerializerOption_PropertyNamingPolicy()
        {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: c => { },
                configureSerializer: c => { c.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; }
            );

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(ComplexType));

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(new[] { "property1", "property2" }, schema.Properties.Keys);
        }

        [Theory]
        [InlineData(false, new[] { "\"Value2\"", "\"Value4\"", "\"Value8\"" }, "\"Value4\"")]
        [InlineData(true, new[] { "\"value2\"", "\"value4\"", "\"value8\"" }, "\"value4\"")]
        public void HonorsSerializerOption_StringEnumConverter(
            bool camelCaseText,
            string[] expectedEnumAsJson,
            string expectedDefaultAsJson)
        {
            var subject = ConfiguredSchemaRepository(
                configureGenerator: c => { c.UseInlineDefinitionsForEnums = true; },
                configureSerializer: c => { c.Converters.Add(new JsonStringEnumConverter(namingPolicy: (camelCaseText ? JsonNamingPolicy.CamelCase : null), true)); }
            );

            var referenceSchema = subject.GetTypeSchema(typeof(TypeWithDefaultAttributeOnEnum));

            var schema = subject.Schemas[referenceSchema.Reference.Id];
            var propertySchema = schema.Properties[nameof(TypeWithDefaultAttributeOnEnum.EnumWithDefault)];
            Assert.Equal("string", propertySchema.Type);
            Assert.Equal(expectedEnumAsJson, propertySchema.Enum.Select(openApiAny => openApiAny.ToJson()));
            Assert.Equal(expectedDefaultAsJson, propertySchema.Default.ToJson());
        }

        [Fact]
        public void HonorsSerializerAttribute_StringEnumConverter()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(JsonConverterAnnotatedEnum));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "\"Value1\"", "\"Value2\"", "\"X\"" }, schema.Enum.Select(openApiAny => openApiAny.ToJson()));
        }

        [Fact]
        public void HonorsSerializerAttributes_JsonIgnore()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(JsonIgnoreAnnotatedType));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal( new[] { /* "StringWithJsonIgnore" */ "StringWithNoAnnotation" }, schema.Properties.Keys.ToArray());
        }

        [Fact]
        public void HonorsSerializerAttribute_JsonPropertyName()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(JsonPropertyNameAnnotatedType));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal( new[] { "string-with-json-property-name" }, schema.Properties.Keys.ToArray());
        }

        [Fact]
        public void HonorsSerializerAttribute_JsonExtensionData()
        {
            var schemaRepository = ConfiguredSchemaRepository();

            var referenceSchema = schemaRepository.GetTypeSchema(typeof(JsonExtensionDataAnnotatedType));

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Null(schema.AdditionalProperties.Type);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(JsonDocument))]
        [InlineData(typeof(JsonElement))]
        public void GeneratesOpenSchema_IfDynamicJsonType(Type type)
        {
            var schema = ConfiguredSchemaRepository().GetTypeSchema(type);

            Assert.Null(schema.Reference);
            Assert.Null(schema.Type);
        }

        [Fact]
        public void Supports_Custom_Mapping_With_Inline_Disposition() {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: options => {
                    options.SchemaMappingProviders.Add(new SimpleSchemaMappingProvider(
                        SchemaDisposition.Inline,
                        typeof(DateTime),
                        context => new OpenApiSchema { Type = "number", Format = "flurble" }
                    ));
                }
            );

            var schema = schemaRepository.GetTypeSchema<DateTime>();

            Assert.Null(schema.Reference);
            Assert.Equal("flurble", schema.Format);
        }

        [Fact]
        public void Supports_Custom_Mapping_With_Reference_Disposition() {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: options => {
                    options.SchemaMappingProviders.Add(new SimpleSchemaMappingProvider(
                        SchemaDisposition.Reference,
                        typeof(DateTime),
                        context => new OpenApiSchema { Type = "number", Format = "flurble" }
                    ));
                }
            );

            var reference = schemaRepository.GetTypeSchema<DateTime>();

            Assert.NotNull(reference.Reference);
            var schema = schemaRepository.Schemas[reference.Reference.Id];
            Assert.NotNull(schema);
            Assert.Equal("flurble", schema.Format);
        }

        [Fact]
        public void Supports_Custom_Mapping_For_Nullable_Value() {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: options => {
                    options.UseAllOfToExtendReferenceSchemas = true;
                    options.SchemaMappingProviders.Add(new SimpleSchemaMappingProvider(
                        SchemaDisposition.Reference,
                        typeof(DateTime),
                        context => new OpenApiSchema { Type = "number", Format = "flurble" }
                    ));
                }
            );

            var nullableSchema = schemaRepository.GetTypeSchema<Nullable<DateTime>>();

            Assert.True(nullableSchema.Nullable);
            Assert.NotNull(nullableSchema.AllOf);
            Assert.Equal(1, nullableSchema.AllOf.Count);
            Assert.NotNull(nullableSchema.AllOf[0].Reference);
            var dateTimeSchema = schemaRepository.Schemas[nullableSchema.AllOf[0].Reference.Id];
            Assert.NotNull(dateTimeSchema);
            Assert.Equal("flurble", dateTimeSchema.Format);
        }

        [Fact]
        public void Supports_Custom_SchemaMapping_For_Array_Item() {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: options => {
                    options.SchemaMappingProviders.Add(new SimpleSchemaMappingProvider(
                        SchemaDisposition.Reference,
                        typeof(ComplexType),
                        context => new OpenApiSchema { Type = "string", Format = "flurble" }
                    ));
                }
            );

            var schema = schemaRepository.GetTypeSchema(typeof(List<ComplexType>));

            Assert.Equal("array", schema.Type);
            Assert.NotNull(schema.Items);
            Assert.NotNull(schema.Items.Reference);
            Assert.Equal(schemaRepository.GetSchemaId(typeof(ComplexType)), schema.Items.Reference.Id);
            Assert.Contains(schemaRepository.GetSchemaId(typeof(ComplexType)), schemaRepository.Schemas.Keys);
            var valueSchema = schemaRepository.Schemas[schemaRepository.GetSchemaId(typeof(ComplexType))];
            Assert.NotNull(valueSchema);
            Assert.Equal("flurble", valueSchema.Format);
        }

        [Fact]
        public void Supports_Custom_SchemaMapping_For_Dictionary_Value() {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: options => {
                    options.SchemaMappingProviders.Add(new SimpleSchemaMappingProvider(
                        SchemaDisposition.Reference,
                        typeof(ComplexType),
                        context => new OpenApiSchema { Type = "string", Format = "flurble" }
                    ));
                }
            );

            var schema = schemaRepository.GetTypeSchema(typeof(Dictionary<string, ComplexType>));

            Assert.Equal("object", schema.Type);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.NotNull(schema.AdditionalProperties.Reference);
            Assert.Equal(schemaRepository.GetSchemaId(typeof(ComplexType)), schema.AdditionalProperties.Reference.Id);
            Assert.Contains(schemaRepository.GetSchemaId(typeof(ComplexType)), schemaRepository.Schemas.Keys);
            var valueSchema = schemaRepository.Schemas[schemaRepository.GetSchemaId(typeof(ComplexType))];
            Assert.NotNull(valueSchema);
            Assert.Equal("flurble", valueSchema.Format);
        }

        [Fact]
        public void Supports_Recursion_In_Custom_Mapping() {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: options => {
                    options.SchemaMappingProviders.Add(new SimpleSchemaMappingProvider(
                        SchemaDisposition.Reference,
                        typeof(SelfReferencingType),
                        context => new OpenApiSchema {
                            Type = "object",
                            Properties = { { nameof(SelfReferencingType.Another), context.SchemaRepository.GetTypeSchema(context.Type) } }
                        }
                    ));
                }
            );

            var schema = schemaRepository.GetTypeSchema(typeof(SelfReferencingType));

            Assert.NotNull(schema.Reference);
            Assert.Contains(nameof(SelfReferencingType), schemaRepository.Schemas);
            var generatedSchema = schemaRepository.Schemas[nameof(SelfReferencingType)];
            Assert.Equal("object", generatedSchema.Type);
            Assert.NotEmpty(generatedSchema.Properties);
            Assert.Equal(new[] { nameof(SelfReferencingType.Another) }, generatedSchema.Properties.Keys);
            // Property schema is a reference to the generated schema
            Assert.NotNull(generatedSchema.Properties[nameof(SelfReferencingType.Another)].Reference);
            Assert.Equal(nameof(SelfReferencingType), generatedSchema.Properties[nameof(SelfReferencingType.Another)].Reference.Id);
        }

        [Fact]
        public void Supports_Indirect_Recursion_In_Custom_Mapping() {
            var schemaRepository = ConfiguredSchemaRepository(
                configureGenerator: options => {
                    options.SchemaMappingProviders.Add(new SimpleSchemaMappingProvider(
                        SchemaDisposition.Reference, typeof(IndirectRecursion),
                        context => new OpenApiSchema {
                            Type = "object",
                            Properties = {
                                { nameof(IndirectRecursion.Intermediary), context.SchemaRepository.GetMemberSchema<IndirectRecursion>(x => x.Intermediary) }
                            },
                            Extensions = { ["flurble"] = new OpenApiBoolean(true) }
                        }
                    ));
                }
            );

            var schema = schemaRepository.GetTypeSchema(typeof(IndirectRecursion));

            Assert.NotNull(schema.Reference);
            var indirectSchema = schemaRepository.Schemas[schema.Reference.Id];
            Assert.Equal("object", indirectSchema.Type);
            Assert.NotNull(indirectSchema.Extensions);
            Assert.Contains("flurble", indirectSchema.Extensions.Keys);
            Assert.Equal(new[] { nameof(IndirectRecursion.Intermediary) }, indirectSchema.Properties.Keys);
            Assert.NotNull(indirectSchema.Properties[nameof(IndirectRecursion.Intermediary)].Reference);
            var intermediarySchema = schemaRepository.Schemas[indirectSchema.Properties[nameof(IndirectRecursion.Intermediary)].Reference.Id];
            Assert.NotNull(intermediarySchema);
            Assert.NotNull(intermediarySchema?.Properties);
            Assert.NotNull(intermediarySchema.Properties.Values.SingleOrDefault());
            Assert.NotNull(intermediarySchema.Properties.Values.Single().Reference);
            // Assert that the schema referenced by the property of the intermediary schema is the
            // same as the generated schema.
            Assert.Equal(schema.Reference.Id, intermediarySchema.Properties.Values.Single().Reference.Id);
        }

        private SchemaRepository ConfiguredSchemaRepository(
            Action<SchemaGeneratorOptions> configureGenerator = null,
            Action<JsonSerializerOptions> configureSerializer = null)
        {
            var generatorOptions = new SchemaGeneratorOptions();
            configureGenerator?.Invoke(generatorOptions);

            var serializerOptions = new JsonSerializerOptions();
            configureSerializer?.Invoke(serializerOptions);


            return new SchemaRepository(
                schemaMappingProviders: Enumerable.Empty<ISchemaMappingProvider>(),
                schemaGenerator: new SchemaGenerator(generatorOptions, new SystemTextJsonBehavior(serializerOptions)),
                generatorOptions: generatorOptions
            );
        }
    }
}
