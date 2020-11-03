using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Net;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SchemaGenerator2Tests
    {
        [Theory]
        [InlineData(typeof(bool), "boolean", null)]
        [InlineData(typeof(int), "integer", "int32")]
        [InlineData(typeof(float), "number", "float")]
        [InlineData(typeof(string), "string", null)]
        [InlineData(typeof(bool?), "boolean", null)]
        public void GenerateSchema_GeneratesPrimitiveSchema_IfTypeIsPrimitiveOrNullablePrimitive(
            Type type,
            string expectedSchemaType,
            string expectedFormat)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(IntEnum), "integer", "int32", new[] { "2", "4", "8" })]
        [InlineData(typeof(LongEnum), "integer", "int64", new[] { "2", "4", "8" })]
        [InlineData(typeof(LongEnum?), "integer", "int64", new[] { "2", "4", "8" })]
        public void GenerateSchema_GeneratesReferencedEnumSchema_IfTypeIsEnumOrNullableEnum(
            Type type,
            string expectedSchemaType,
            string expectedFormat,
            string[] expectedEnumAsJson)
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
        public void GenerateSchema_DedupsEnumValues_IfTypeIsEnum_AndHasDuplicateValues()
        {
            var enumType = typeof(HttpStatusCode);
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(enumType, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(enumType.GetEnumValues().Cast<HttpStatusCode>().Distinct().Count(), schema.Enum.Count);
        }

        [Fact]
        public void GenerateSchema_GeneratesDictionarySchema_IfTypeIsDictionary()
        {
            var schema = Subject().GenerateSchema(typeof(IDictionary<string, string>), new SchemaRepository());

            Assert.Equal("object", schema.Type);
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal("string", schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GenerateSchema_GeneratesObjectSchema_IfTypeIsDictionary_AndHasEnumKey()
        {
            var schema = Subject().GenerateSchema(typeof(IDictionary<IntEnum, int>), new SchemaRepository());

            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Value2", "Value4", "Value8" }, schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedDictionarySchema_IfTypeIsDictionary_AndSelfReferencing()
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

        [Fact]
        public void GenerateSchema_GeneratesArraySchema_IfTypeIsEnumerable()
        {
            var schema = Subject().GenerateSchema(typeof(IEnumerable<int>), new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.NotNull(schema.Items);
            Assert.Equal("integer", schema.Items.Type);
            Assert.Equal("int32", schema.Items.Format);
        }

        [Fact]
        public void GenerateSchema_SetsUniqueItems_IfTypeIsEnumerable_AndIsASet()
        {
            var schema = Subject().GenerateSchema(typeof(ISet<int>), new SchemaRepository());

            Assert.True(schema.UniqueItems);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedArraySchema_IfTypeIsEnumerable_AndSelfReferencing()
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
        public void GenerateSchema_GeneratesReferencedObjectSchema_IfTypeIsComplex(
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

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType))]
        public void GenerateSchema_SetsRequired_IfTypeIsComplex_AndHasMembersWithRequiredAttributes(
            Type type)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(new[] { "StringWithRequired" }, schema.Required);
        }

        [Theory]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.IntProperty), false)]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.NullableIntProperty), true)]
        [InlineData(typeof(TypeWithNullableProperties), nameof(TypeWithNullableProperties.StringProperty), true)]
        public void GenerateSchema_SetsNullable_IfMemberIsNullableOrReferenceType(
            Type declaringType,
            string propertyName,
            bool expectedNullable)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal(expectedNullable, schema.Nullable);
        }

        [Theory]
        [InlineData(typeof(TypeWithRestrictedProperties), nameof(TypeWithRestrictedProperties.ReadOnlyProperty), true, false)]
        [InlineData(typeof(TypeWithRestrictedProperties), nameof(TypeWithRestrictedProperties.WriteOnlyProperty), false, true)]
        [InlineData(typeof(TypeWithRestrictedProperties), nameof(TypeWithRestrictedProperties.ReadWriteProperty), false, false)]
        public void GenerateSchema_SetsReadOnlyAndWriteOnly_IfMemberWithRestrictedAccess(
            Type declaringType,
            string propertyName,
            bool expectedReadOnly,
            bool expectedWriteOnly)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal(expectedReadOnly, schema.ReadOnly);
            Assert.Equal(expectedWriteOnly, schema.WriteOnly);
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
        public void GenerateSchema_SetsDefault_IfMemberWithDefaultValueAttribute(
            Type declaringType,
            string propertyName,
            string expectedDefaultAsJson)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.NotNull(schema.Default);
            Assert.Equal(expectedDefaultAsJson, schema.Default.ToJson());
        }

        [Fact]
        public void GenerateSchema_SetsDeprecated_IfMemberWithObsoleteAttribute()
        {
            var propertyInfo = typeof(TypeWithObsoleteAttribute).GetProperty(nameof(TypeWithObsoleteAttribute.ObsoleteProperty));

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.True(schema.Deprecated);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes), nameof(TypeWithValidationAttributes.StringWithDataTypeCreditCard))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType), nameof(TypeWithValidationAttributesViaMetadataType.StringWithDataTypeCreditCard))]
        public void GenerateSchema_SetsFormat_IfMemberWithDataTypeAttribute(
            Type declaringType,
            string propertyName)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal("credit-card", schema.Format);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes), nameof(TypeWithValidationAttributes.StringWithStringLength))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType), nameof(TypeWithValidationAttributesViaMetadataType.StringWithStringLength))]
        public void GenerateSchema_SetsMinLengthAndMaxLength_IfMemberWithStringLengthAttribute(
            Type declaringType,
            string propertyName)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal(5, schema.MinLength);
            Assert.Equal(10, schema.MaxLength);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes), nameof(TypeWithValidationAttributes.StringWithMinMaxLength))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType), nameof(TypeWithValidationAttributesViaMetadataType.StringWithMinMaxLength))]
        public void GenerateSchema_SetsMinLengthAndMaxLength_IfMemberWithMinMaxLengthAttributes(
            Type declaringType,
            string propertyName)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal(1, schema.MinLength);
            Assert.Equal(3, schema.MaxLength);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes), nameof(TypeWithValidationAttributes.ArrayWithMinMaxLength))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType), nameof(TypeWithValidationAttributesViaMetadataType.ArrayWithMinMaxLength))]
        public void GenerateSchema_SetsMinItemsAndMaxItems_IfMemberWithMinMaxLengthAttributes_AndTypeIsEnumerable(
            Type declaringType,
            string propertyName)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal(1, schema.MinItems);
            Assert.Equal(3, schema.MaxItems);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes), nameof(TypeWithValidationAttributes.IntWithRange))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType), nameof(TypeWithValidationAttributesViaMetadataType.IntWithRange))]
        public void GenerateSchema_SetsMinimumAndMaximum_IfMemberWithRangeAttribute(
            Type declaringType,
            string propertyName)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal(1, schema.Minimum);
            Assert.Equal(10, schema.Maximum);
        }

        [Theory]
        [InlineData(typeof(TypeWithValidationAttributes), nameof(TypeWithValidationAttributes.StringWithRegularExpression))]
        [InlineData(typeof(TypeWithValidationAttributesViaMetadataType), nameof(TypeWithValidationAttributesViaMetadataType.StringWithRegularExpression))]
        public void GenerateSchema_SetsPattern_IfMemberWithRegularAttribute(
            Type declaringType,
            string propertyName)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);

            var schema = Subject().GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Pattern);
        }

        [Fact]
        public void GenerateSchema_SetsDefault_IfParameterWithDefaultValue()
        {
            var parameterInfo = typeof(FakeController)
                .GetMethod(nameof(FakeController.ActionWithIntParameterWithDefaultValue))
                .GetParameters()[0];

            var schema = Subject().GenerateSchema(parameterInfo.ParameterType, new SchemaRepository(), parameterInfo: parameterInfo);

            Assert.NotNull(schema.Default);
            Assert.Equal("1", schema.Default.ToJson());
        }

        [Fact]
        public void GenerateSchema_SetsDefault_IfParameterWithDefaultValueAttribute()
        {
            var parameterInfo = typeof(FakeController)
                .GetMethod(nameof(FakeController.ActionWithIntParameterWithDefaultValueAttribute))
                .GetParameters()[0];

            var schema = Subject().GenerateSchema(parameterInfo.ParameterType, new SchemaRepository(), parameterInfo: parameterInfo);

            Assert.NotNull(schema.Default);
            Assert.Equal("3", schema.Default.ToJson());
        }

        [Fact]
        public void GenerateSchema_SetsMinimumAndMaximum_IfParameterWithRangeAttribute()
        {
            var parameterInfo = typeof(FakeController)
                .GetMethod(nameof(FakeController.ActionWithIntParameterWithRangeAttribute))
                .GetParameters()[0];

            var schema = Subject().GenerateSchema(parameterInfo.ParameterType, new SchemaRepository(), parameterInfo: parameterInfo);

            Assert.Equal(1, schema.Minimum);
            Assert.Equal(12, schema.Maximum);
        }

        [Theory]
        [InlineData(typeof(IFormFile))]
        [InlineData(typeof(FileResult))]
        public void GenerateSchema_SupportsOption_DataContractResolvers(Type type)
        {
            var subject = Subject(
                configureGenerator: c => c.DataContractResolvers.Insert(0, new FileDataContractResolver())
            );
            var schema = subject.GenerateSchema(type, new SchemaRepository());

            Assert.Equal("string", schema.Type);
            Assert.Equal("binary", schema.Format);
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
        public void GenerateSchema_SupportsOption_UseOneOfForPolymorphism()
        {
            var subject = Subject(
                configureGenerator: c => c.UseOneOfForPolymorphism = true
            );
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

            // The polymorphic schema
            Assert.NotNull(schema.OneOf);
            Assert.Equal(2, schema.OneOf.Count);
            // The first sub type schema
            Assert.NotNull(schema.OneOf[0].Reference);
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Equal("object", subType1Schema.Type);
            Assert.Equal(new[] { "BaseProperty", "Property1" }, subType1Schema.Properties.Keys);
            // The second sub type schema
            Assert.NotNull(schema.OneOf[1].Reference);
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Equal("object", subType2Schema.Type);
            Assert.Equal(new[] { "BaseProperty", "Property2" }, subType2Schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_KnownTypesSelector()
        {
            var subject = Subject(configureGenerator: c =>
            {
                c.UseOneOfForPolymorphism = true;
                c.KnownTypesSelector = (baseType) =>
                {
                    // Include the baseType
                    var subTypes = baseType.Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
                    return subTypes.Any() ? subTypes.Prepend(baseType) : Enumerable.Empty<Type>();
                };
            });

            var schema = subject.GenerateSchema(typeof(BaseType), new SchemaRepository());

            // The polymorphic schema
            Assert.NotNull(schema.OneOf);
            Assert.Equal(3, schema.OneOf.Count);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_UseAllOfForInheritance()
        {
            var subject = Subject(
                configureGenerator: c => c.UseAllOfForInheritance = true
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(SubType1), schemaRepository);

            // The sub type schema
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal(new[] { "Property1" }, schema.Properties.Keys);
            Assert.Equal(1, schema.AllOf.Count);
            // The base schema
            var baseSchema = schemaRepository.Schemas[schema.AllOf[0].Reference.Id];
            Assert.Equal("object", baseSchema.Type);
            Assert.Equal(new[] { "BaseProperty" }, baseSchema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_UseOneOfForPolymorphism_And_UseAllOfForInheritance()
        {
            var subject = Subject(configureGenerator: c =>
            {
                c.UseOneOfForPolymorphism = true;
                c.UseAllOfForInheritance = true;
            });
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

            // The polymorphic schema
            Assert.NotNull(schema.OneOf);
            Assert.Equal(2, schema.OneOf.Count);
            // The first sub type schema
            Assert.NotNull(schema.OneOf[0].Reference);
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Equal("object", subType1Schema.Type);
            Assert.Equal(new[] { "Property1" }, subType1Schema.Properties.Keys);
            Assert.Equal(1, subType1Schema.AllOf.Count);
            // The second sub type schema
            Assert.NotNull(schema.OneOf[1].Reference);
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Equal("object", subType2Schema.Type);
            Assert.Equal(new[] { "Property2" }, subType2Schema.Properties.Keys);
            Assert.Equal(1, subType2Schema.AllOf.Count);
            // The base schema
            Assert.Equal(subType1Schema.AllOf[0].Reference.Id, subType2Schema.AllOf[0].Reference.Id);
            var baseSchema = schemaRepository.Schemas[subType1Schema.AllOf[0].Reference.Id];
            Assert.Equal("object", baseSchema.Type);
            Assert.Equal(new[] { "BaseProperty" }, baseSchema.Properties.Keys);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(IDictionary<string, string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(ComplexType))]
        public void GenerateSchema_SupportsOption_SchemaFilters(Type type)
        {
            var subject = Subject(
                configureGenerator: (c) => c.SchemaFilters.Add(new VendorExtensionsSchemaFilter())
            );
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(type, schemaRepository);

            if (schema.Reference == null)
                Assert.Contains("X-foo", schema.Extensions.Keys);
            else
                Assert.Contains("X-foo", schemaRepository.Schemas[schema.Reference.Id].Extensions.Keys);
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

        //[Fact]
        //public void GenerateSchema_HonorsSerializerOption_IgnoreReadonlyProperties()
        //{
        //    var subject = Subject(
        //        configureGenerator: c => { },
        //        configureSerializer: c => { c.IgnoreReadOnlyProperties = true; }
        //    );
        //    var schemaRepository = new SchemaRepository();

        //    var referenceSchema = subject.GenerateSchema(typeof(ComplexType), schemaRepository);

        //    Assert.NotNull(referenceSchema.Reference);
        //    var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        //}

        //[Fact]
        //public void GenerateSchema_HonorsSerializerOption_PropertyNamingPolicy()
        //{
        //    var subject = Subject(
        //        configureGenerator: c => { },
        //        configureSerializer: c => { c.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; }
        //    );
        //    var schemaRepository = new SchemaRepository();

        //    var referenceSchema = subject.GenerateSchema(typeof(ComplexType), schemaRepository);

        //    Assert.NotNull(referenceSchema.Reference);
        //    var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        //    Assert.Equal(new[] { "property1", "property2" }, schema.Properties.Keys);
        //}

        //[Theory]
        //[InlineData(false, new[] { "\"Value2\"", "\"Value4\"", "\"Value8\"" }, "\"Value4\"")]
        //[InlineData(true, new[] { "\"value2\"", "\"value4\"", "\"value8\"" }, "\"value4\"")]
        //public void GenerateSchema_HonorsSerializerOption_StringEnumConverter(
        //    bool camelCaseText,
        //    string[] expectedEnumAsJson,
        //    string expectedDefaultAsJson)
        //{
        //    var subject = Subject(
        //        configureGenerator: c => { c.UseInlineDefinitionsForEnums = true; },
        //        configureSerializer: c => { c.Converters.Add(new JsonStringEnumConverter(namingPolicy: (camelCaseText ? JsonNamingPolicy.CamelCase : null), true)); }
        //    );
        //    var schemaRepository = new SchemaRepository();

        //    var referenceSchema = subject.GenerateSchema(typeof(TypeWithDefaultAttributeOnEnum), schemaRepository);

        //    var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        //    var propertySchema = schema.Properties[nameof(TypeWithDefaultAttributeOnEnum.EnumWithDefault)];
        //    Assert.Equal("string", propertySchema.Type);
        //    Assert.Equal(expectedEnumAsJson, propertySchema.Enum.Select(openApiAny => openApiAny.ToJson()));
        //    Assert.Equal(expectedDefaultAsJson, propertySchema.Default.ToJson());
        //}

        //[Fact]
        //public void GenerateSchema_HonorsSerializerAttribute_StringEnumConverter()
        //{
        //    var schemaRepository = new SchemaRepository();

        //    var referenceSchema = Subject().GenerateSchema(typeof(JsonConverterAnnotatedEnum), schemaRepository);

        //    var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        //    Assert.Equal("string", schema.Type);
        //    Assert.Equal(new[] { "\"Value1\"", "\"Value2\"", "\"X\"" }, schema.Enum.Select(openApiAny => openApiAny.ToJson()));
        //}

        //[Fact]
        //public void GenerateSchema_HonorsSerializerAttributes_JsonIgnore()
        //{
        //    var schemaRepository = new SchemaRepository();

        //    var referenceSchema = Subject().GenerateSchema(typeof(JsonIgnoreAnnotatedType), schemaRepository);

        //    var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        //    Assert.Equal( new[] { /* "StringWithJsonIgnore" */ "StringWithNoAnnotation" }, schema.Properties.Keys.ToArray());
        //}

        //[Fact]
        //public void GenerateSchema_HonorsSerializerAttribute_JsonPropertyName()
        //{
        //    var schemaRepository = new SchemaRepository();

        //    var referenceSchema = Subject().GenerateSchema(typeof(JsonPropertyNameAnnotatedType), schemaRepository);

        //    var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        //    Assert.Equal( new[] { "string-with-json-property-name" }, schema.Properties.Keys.ToArray());
        //}

        //[Fact]
        //public void GenerateSchema_HonorsSerializerAttribute_JsonExtensionData()
        //{
        //    var schemaRepository = new SchemaRepository();

        //    var referenceSchema = Subject().GenerateSchema(typeof(JsonExtensionDataAnnotatedType), schemaRepository);

        //    var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        //    Assert.True(schema.AdditionalPropertiesAllowed);
        //    Assert.NotNull(schema.AdditionalProperties);
        //    Assert.Null(schema.AdditionalProperties.Type);
        //}

        //[Theory]
        //[InlineData(typeof(object))]
        //[InlineData(typeof(JsonDocument))]
        //[InlineData(typeof(JsonElement))]
        //public void GenerateSchema_GeneratesOpenSchema_IfDynamicJsonType(Type type)
        //{
        //    var schema = Subject().GenerateSchema(type, new SchemaRepository());

        //    Assert.Null(schema.Reference);
        //    Assert.Null(schema.Type);
        //}

        private SchemaGenerator2 Subject(
            Action<JsonSerializerOptions> configureSerializer = null,
            Action<SchemaGeneratorOptions> configureGenerator = null)
        {
            var serializerOptions = new JsonSerializerOptions();
            configureSerializer?.Invoke(serializerOptions);

            var generatorOptions = new SchemaGeneratorOptions();
            generatorOptions.DataContractResolvers.Add(new SystemTextJsonContractResolver(serializerOptions));
            configureGenerator?.Invoke(generatorOptions);

            return new SchemaGenerator2(generatorOptions);
        }
    }
}
