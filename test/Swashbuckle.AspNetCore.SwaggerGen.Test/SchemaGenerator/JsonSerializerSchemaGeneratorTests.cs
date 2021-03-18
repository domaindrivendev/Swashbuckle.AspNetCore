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
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;
using Microsoft.OpenApi.Any;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonSerializerSchemaGeneratorTests
    {
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
        [InlineData(typeof(Uri), "string", "uri")]
        [InlineData(typeof(bool?), "boolean", null)]
        [InlineData(typeof(int?), "integer", "int32")]
        [InlineData(typeof(DateTime?), "string", "date-time")]
        [InlineData(typeof(Guid?), "string", "uuid")]
        public void GenerateSchema_GeneratesPrimitiveSchema_IfPrimitiveOrNullablePrimitiveType(
            Type type,
            string expectedSchemaType,
            string expectedFormat)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(IntEnum), "integer", "int32", "2", "4", "8")]
        [InlineData(typeof(LongEnum), "integer", "int64", "2", "4", "8")]
        [InlineData(typeof(IntEnum?), "integer", "int32", "2", "4", "8")]
        [InlineData(typeof(LongEnum?), "integer", "int64", "2", "4", "8")]
        public void GenerateSchema_GeneratesReferencedEnumSchema_IfEnumOrNullableEnumType(
            Type type,
            string expectedSchemaType,
            string expectedFormat,
            params string[] expectedEnumAsJson)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
            Assert.NotNull(schema.Enum);
            Assert.Equal(expectedEnumAsJson, schema.Enum.Select(openApiAny => openApiAny.ToJson()));
        }

        [Fact]
        public void GenerateSchema_DedupsEnumValues_IfEnumTypeHasDuplicateValues()
        {
            var enumType = typeof(HttpStatusCode);
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(enumType, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(enumType.GetEnumValues().Cast<HttpStatusCode>().Distinct().Count(), schema.Enum.Count);
        }

        [Theory]
        [InlineData(typeof(IDictionary<string, int>), "integer")]
        [InlineData(typeof(IReadOnlyDictionary<string, bool>), "boolean")]
        [InlineData(typeof(IDictionary), null)]
        [InlineData(typeof(ExpandoObject), null)]
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
        public void GenerateSchema_GeneratesReferencedDictionarySchema_IfDictionaryTypeIsSelfReferencing()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(DictionaryOfSelf), schemaRepository);

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

        [Theory]
        [InlineData(typeof(ISet<string>))]
        [InlineData(typeof(SortedSet<string>))]
        [InlineData(typeof(KeyedCollectionOfComplexType))]
        public void GenerateSchema_SetsUniqueItems_IfEnumerableTypeIsSetOrKeyedCollection(Type type)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.True(schema.UniqueItems);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedArraySchema_IfEnumerableTypeIsSelfReferencing()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ListOfSelf), schemaRepository);

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
        public void GenerateSchema_GeneratesReferencedObjectSchema_IfComplexType(
            Type type,
            string expectedSchemaId,
            string[] expectedProperties)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            Assert.Equal(expectedSchemaId, referenceSchema.Reference.Id);
            var schema = schemaRepository.Schemas[expectedSchemaId];
            Assert.Equal("object", schema.Type);
            Assert.Equal(expectedProperties, schema.Properties.Keys);
            Assert.False(schema.AdditionalPropertiesAllowed);
        }

        [Fact]
        public void GenerateSchema_IncludesInheritedProperties_IfComplexTypeIsDerived()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(SubType1), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "BaseProperty", "Property1" }, schema.Properties.Keys);
        }

        [Theory]
        [InlineData(typeof(IBaseInterface), new[] { "BaseProperty" })]
        [InlineData(typeof(ISubInterface1), new[] { "BaseProperty", "Property1" })]
        [InlineData(typeof(ISubInterface2), new[] { "BaseProperty", "Property2" })]
        [InlineData(typeof(IMultiSubInterface), new[] { "BaseProperty", "Property1", "Property2", "Property3" })]
        public void GenerateSchema_IncludesInheritedProperties_IfTypeIsAnInterfaceHierarchy(
            Type type,
            string[] expectedPropertyNames)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(expectedPropertyNames.OrderBy(n => n), schema.Properties.Keys.OrderBy(k => k));
        }

        [Fact]
        public void GenerateSchema_ExcludesIndexerProperties_IfComplexTypeIsIndexed()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(IndexedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1" }, schema.Properties.Keys);
        }

        [Theory]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.IntProperty), false)]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.StringProperty), true)]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.NullableIntProperty), true)]
        public void GenerateSchema_SetsNullableFlag_IfPropertyIsReferenceOrNullableType(
            Type declaringType,
            string propertyName,
            bool expectedNullable)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(declaringType, schemaRepository);

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
        [UseInvariantCulture]
        public void GenerateSchema_SetsDefault_IfPropertyHasDefaultValueAttribute(
            Type declaringType,
            string propertyName,
            string expectedDefaultAsJson)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(declaringType, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            var propertySchema = schema.Properties[propertyName];
            Assert.NotNull(propertySchema.Default);
            Assert.Equal(expectedDefaultAsJson, propertySchema.Default.ToJson());
        }

        [Fact]
        public void GenerateSchema_SetsDeprecatedFlag_IfPropertyHasObsoleteAttribute()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(TypeWithObsoleteAttribute), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.True(schema.Properties["ObsoleteProperty"].Deprecated);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType))]

        public void GenerateSchema_SetsValidationProperties_IfComplexTypeHasValidationAttributes(Type type)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

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
            Assert.False(schema.Properties["StringWithRequired"].Nullable);
            Assert.Equal(new[] { "StringWithRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void GenerateSchema_SetsReadOnlyAndWriteOnlyFlags_IfPropertyIsRestricted()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(TypeWithRestrictedProperties), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.False(schema.Properties["ReadWriteProperty"].ReadOnly);
            Assert.False(schema.Properties["ReadWriteProperty"].WriteOnly);
            Assert.True(schema.Properties["ReadOnlyProperty"].ReadOnly);
            Assert.False(schema.Properties["ReadOnlyProperty"].WriteOnly);
            Assert.False(schema.Properties["WriteOnlyProperty"].ReadOnly);
            Assert.True(schema.Properties["WriteOnlyProperty"].WriteOnly);
        }

        [Fact]
        public void GenerateSchema_SetsDefault_IfParameterHasDefaultValueAttribute()
        {
            var schemaRepository = new SchemaRepository();

            var parameterInfo = typeof(FakeController)
                .GetMethod(nameof(FakeController.ActionWithIntParameterWithDefaultValueAttribute))
                .GetParameters()
                .First();

            var schema = Subject().GenerateSchema(parameterInfo.ParameterType, schemaRepository, parameterInfo: parameterInfo);

            Assert.NotNull(schema.Default);
            Assert.Equal("3", schema.Default.ToJson());
        }

        [Theory]
        [InlineData(typeof(ComplexType), typeof(ComplexType), "string")]
        [InlineData(typeof(GenericType<int, string>), typeof(GenericType<int, string>), "string")]
        [InlineData(typeof(GenericType<,>), typeof(GenericType<int, int>), "string")]
        public void GenerateSchema_SupportsOption_CustomTypeMappings(
            Type mappingType,
            Type type,
            string expectedSchemaType)
        {
            var subject = Subject(
                configureGenerator: c => c.CustomTypeMappings.Add(mappingType, () => new OpenApiSchema { Type = "string" })
            );
            var schema = subject.GenerateSchema(type, new SchemaRepository());

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Empty(schema.Properties);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(IDictionary<string, string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(ComplexType))]
        public void GenerateSchema_SupportsOption_SchemaFilters(Type type)
        {
            var subject = Subject(
                configureGenerator: (c) => c.SchemaFilters.Add(new TestSchemaFilter())
            );
            var schemaRepository = new SchemaRepository("v1");

            var schema = subject.GenerateSchema(type, schemaRepository);

            var definitionSchema = schema.Reference == null ? schema : schemaRepository.Schemas[schema.Reference.Id];
            Assert.Contains("X-foo", definitionSchema.Extensions.Keys);
            Assert.Equal("v1", ((OpenApiString)definitionSchema.Extensions["X-docName"]).Value);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_IgnoreObsoleteProperties()
        {
            var subject = Subject(
                configureGenerator: c => c.IgnoreObsoleteProperties = true
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(TypeWithObsoleteAttribute), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.DoesNotContain("ObsoleteProperty", schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_SchemaIdSelector()
        {
            var subject = Subject(
                configureGenerator: c => c.SchemaIdSelector = (type) => type.FullName
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(ComplexType), schemaRepository);

            Assert.Equal("Swashbuckle.AspNetCore.TestSupport.ComplexType", referenceSchema.Reference.Id);
            Assert.Contains(referenceSchema.Reference.Id, schemaRepository.Schemas.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_UseAllOfForInheritance()
        {
            var subject = Subject(
                configureGenerator: c => c.UseAllOfForInheritance = true
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(SubType1), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1" }, schema.Properties.Keys);
            Assert.NotNull(schema.AllOf);
            Assert.Equal(1, schema.AllOf.Count);
            Assert.NotNull(schema.AllOf[0].Reference);
            Assert.Equal("BaseType", schema.AllOf[0].Reference.Id);
            // The base type schema
            var baseTypeSchema = schemaRepository.Schemas[schema.AllOf[0].Reference.Id];
            Assert.Equal("object", baseTypeSchema.Type);
            Assert.Equal(new[] { "BaseProperty" }, baseTypeSchema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_SubTypesSelector()
        {
            var subject = Subject(configureGenerator: c =>
            {
                c.UseAllOfForInheritance = true;
                c.SubTypesSelector = (type) => new[] { typeof(SubType1) };
            });
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

            Assert.Equal(new[] { "SubType1", "BaseType" }, schemaRepository.Schemas.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_DiscriminatorNameSelector()
        {
            var subject = Subject(configureGenerator: c =>
            {
                c.UseAllOfForInheritance = true;
                c.DiscriminatorNameSelector = (baseType) => "TypeName";
                c.DiscriminatorValueSelector = (subType) => subType.Name;
            });

            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Contains("TypeName", schema.Properties.Keys);
            Assert.Contains("TypeName", schema.Required);
            Assert.NotNull(schema.Discriminator);
            Assert.Equal("TypeName", schema.Discriminator.PropertyName);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_UseAllOfForPolymorphism()
        {
            var subject = Subject(configureGenerator: c =>
            {
                c.UseOneOfForPolymorphism = true;
            });
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

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
        public void GenerateSchema_SupportsOption_UseAllOfToExtendReferenceSchemas()
        {
            var subject = Subject(
                configureGenerator: c => c.UseAllOfToExtendReferenceSchemas = true
            );
            var propertyInfo = typeof(SelfReferencingType).GetProperty(nameof(SelfReferencingType.Another));

            var schema = subject.GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Null(schema.Reference);
            Assert.NotNull(schema.AllOf);
            Assert.Equal(1, schema.AllOf.Count);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_UseInlineDefinitionsForEnums()
        {
            var subject = Subject(
                configureGenerator: c => c.UseInlineDefinitionsForEnums = true
            );

            var schema = subject.GenerateSchema(typeof(IntEnum), new SchemaRepository());

            Assert.Equal("integer", schema.Type);
            Assert.NotNull(schema.Enum);
        }

        [Theory]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NullableInt), true)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NonNullableInt), false)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NullableString), true)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NonNullableString), false)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NullableArray), true)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NonNullableArray), false)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NullableList), true)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.NonNullableList), false)]
        public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes(
            Type declaringType,
            string propertyName,
            bool expectedNullable)
        {
            var subject = Subject(
                configureGenerator: c => c.SupportNonNullableReferenceTypes = true
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(declaringType, schemaRepository);

            var propertySchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName];
            Assert.Equal(expectedNullable, propertySchema.Nullable);
        }

        [Theory]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.SubTypeWithOneNullableContent), nameof(TypeWithNullableContext.NullableString), true)]
        [InlineData(typeof(TypeWithNullableContext), nameof(TypeWithNullableContext.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContext.NonNullableString), false)]
        public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NullableAttribute_Compiler_Optimizations_Situations(
            Type declaringType,
            string subType,
            string propertyName,
            bool expectedNullable)
        {
            var subject = Subject(
                configureGenerator: c => c.SupportNonNullableReferenceTypes = true
            );
            var schemaRepository = new SchemaRepository();

            subject.GenerateSchema(declaringType, schemaRepository);

            var propertySchema = schemaRepository.Schemas[subType].Properties[propertyName];
            Assert.Equal(expectedNullable, propertySchema.Nullable);
        }


        [Fact]
        public void GenerateSchema_HandlesTypesWithNestedTypes()
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
            var subject = Subject(
                configureGenerator: c => c.SchemaFilters.Add(new RecursiveCallSchemaFilter())
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(ComplexType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("ComplexType", schema.Properties["Self"].Reference.Id);
        }

        [Fact]
        public void GenerateSchema_Errors_IfTypesHaveConflictingSchemaIds()
        {
            var subject = Subject();
            var schemaRepository = new SchemaRepository();

            subject.GenerateSchema(typeof(TestSupport.Namespace1.ConflictingType), schemaRepository);
            Assert.Throws<InvalidOperationException>(() =>
            {
                subject.GenerateSchema(typeof(TestSupport.Namespace2.ConflictingType), schemaRepository);
            });
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerOption_IgnoreReadonlyProperties()
        {
            var subject = Subject(
                configureGenerator: c => { },
                configureSerializer: c => { c.IgnoreReadOnlyProperties = true; }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(ComplexType), schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerOption_PropertyNamingPolicy()
        {
            var subject = Subject(
                configureGenerator: c => { },
                configureSerializer: c => { c.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(ComplexType), schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(new[] { "property1", "property2" }, schema.Properties.Keys);
        }

        [Theory]
        [InlineData(false, new[] { "\"Value2\"", "\"Value4\"", "\"Value8\"" }, "\"Value4\"")]
        [InlineData(true, new[] { "\"value2\"", "\"value4\"", "\"value8\"" }, "\"value4\"")]
        public void GenerateSchema_HonorsSerializerOption_StringEnumConverter(
            bool camelCaseText,
            string[] expectedEnumAsJson,
            string expectedDefaultAsJson)
        {
            var subject = Subject(
                configureGenerator: c => { c.UseInlineDefinitionsForEnums = true; },
                configureSerializer: c => { c.Converters.Add(new JsonStringEnumConverter(namingPolicy: (camelCaseText ? JsonNamingPolicy.CamelCase : null), true)); }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(TypeWithDefaultAttributeOnEnum), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            var propertySchema = schema.Properties[nameof(TypeWithDefaultAttributeOnEnum.EnumWithDefault)];
            Assert.Equal("string", propertySchema.Type);
            Assert.Equal(expectedEnumAsJson, propertySchema.Enum.Select(openApiAny => openApiAny.ToJson()));
            Assert.Equal(expectedDefaultAsJson, propertySchema.Default.ToJson());
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_StringEnumConverter()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonConverterAnnotatedEnum), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "\"Value1\"", "\"Value2\"", "\"X\"" }, schema.Enum.Select(openApiAny => openApiAny.ToJson()));
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttributes_JsonIgnore()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonIgnoreAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];

            string[] expectedKeys =
            {
                nameof(JsonIgnoreAnnotatedType.StringWithJsonIgnoreConditionNever),
                nameof(JsonIgnoreAnnotatedType.StringWithJsonIgnoreConditionWhenWritingDefault),
                nameof(JsonIgnoreAnnotatedType.StringWithJsonIgnoreConditionWhenWritingNull),
                nameof(JsonIgnoreAnnotatedType.StringWithNoAnnotation)
            };

            Assert.Equal(expectedKeys, schema.Properties.Keys.ToArray());
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_JsonPropertyName()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonPropertyNameAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal( new[] { "string-with-json-property-name" }, schema.Properties.Keys.ToArray());
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_JsonExtensionData()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonExtensionDataAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Null(schema.AdditionalProperties.Type);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(JsonDocument))]
        [InlineData(typeof(JsonElement))]
        public void GenerateSchema_GeneratesOpenSchema_IfDynamicJsonType(Type type)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Null(schema.Reference);
            Assert.Null(schema.Type);
        }

        private SchemaGenerator Subject(
            Action<SchemaGeneratorOptions> configureGenerator = null,
            Action<JsonSerializerOptions> configureSerializer = null)
        {
            var generatorOptions = new SchemaGeneratorOptions();
            configureGenerator?.Invoke(generatorOptions);

            var serializerOptions = new JsonSerializerOptions();
            configureSerializer?.Invoke(serializerOptions);

            return new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(serializerOptions));
        }
    }
}
