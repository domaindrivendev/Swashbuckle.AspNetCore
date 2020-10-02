using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.TestSupport;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class NewtonsoftSchemaGeneratorTests
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
        [InlineData(typeof(bool?), "boolean", null)]
        [InlineData(typeof(byte), "integer", "int32")]
        [InlineData(typeof(sbyte), "integer", "int32")]
        [InlineData(typeof(short), "integer", "int32")]
        [InlineData(typeof(ushort), "integer", "int32")]
        [InlineData(typeof(int), "integer", "int32")]
        [InlineData(typeof(int?), "integer", "int32")]
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
        [InlineData(typeof(DateTime?), "string", "date-time")]
        [InlineData(typeof(DateTimeOffset), "string", "date-time")]
        [InlineData(typeof(Guid), "string", "uuid")]
        [InlineData(typeof(TimeSpan), "string", "date-span")]
        [InlineData(typeof(Version), "string", null)]
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
        [InlineData(typeof(IntEnum), "integer", "int32", 3)]
        [InlineData(typeof(IntEnum?), "integer", "int32", 3)]
        [InlineData(typeof(LongEnum), "integer", "int64", 3)]
        [InlineData(typeof(LongEnum?), "integer", "int64", 3)]
        public void GenerateSchema_GeneratesReferencedEnumSchema_IfEnumOrNullableEnumType(
            Type type,
            string expectedSchemaType,
            string expectedFormat,
            int expectedEnumCount)
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(expectedSchemaType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
            Assert.NotNull(schema.Enum);
            Assert.Equal(expectedEnumCount, schema.Enum.Count);
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
        public void GenerateSchema_SetsUniqueItems_IfSetType(Type type)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Equal("array", schema.Type);
            Assert.True(schema.UniqueItems);
        }

        [Fact]
        public void GenerateSchema_GeneratesReferencedArraySchema_IfSelfReferencingEnumerableType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ListOfSelf), schemaRepository);

            Assert.NotNull(referenceSchema.Reference);
            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("array", schema.Type);
            Assert.Equal(schema.Items.Reference.Id, referenceSchema.Reference.Id); // ref to self
        }

        [Theory]
        [InlineData(typeof(ComplexType), "ComplexType", new[] { "Property1", "Property2", "Property3", "Property4" })]
        [InlineData(typeof(GenericType<bool, int>), "BooleanInt32GenericType", new[] { "Property1", "Property2" })]
        [InlineData(typeof(GenericType<bool, int[]>), "BooleanInt32ArrayGenericType", new[] { "Property1", "Property2" })]
        [InlineData(typeof(ContainingType.NestedType), "NestedType", new[] { "Property1" })]
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
        public void GenerateSchema_SetsReadOnlyAndWriteOnlyFlags_IfPropertyAccessIsRestricted()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ComplexType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.False(schema.Properties["Property1"].ReadOnly);
            Assert.False(schema.Properties["Property1"].WriteOnly);
            Assert.True(schema.Properties["Property2"].ReadOnly);
            Assert.False(schema.Properties["Property2"].WriteOnly);
            Assert.False(schema.Properties["Property3"].ReadOnly);
            Assert.True(schema.Properties["Property3"].WriteOnly);
        }

        [Fact]
        public void GenerateSchema_DoesNotSetReadOnlyFlag_IfPropertyIsSetViaConstructor()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ComplexTypeWithConstructor), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.False(schema.Properties["Property1"].ReadOnly);
        }

        [Theory]
        [InlineData(typeof(ComplexType), "Property1", false)]
        [InlineData(typeof(ComplexType), "Property4", true)]
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
            Assert.False(schema.Properties["StringWithRequired"].Nullable);
            Assert.Equal(1, schema.Properties["ArrayWithMinMaxLength"].MinItems);
            Assert.Equal(3, schema.Properties["ArrayWithMinMaxLength"].MaxItems);
        }

        [Theory]
        [InlineData(false, "Value8")]
        [InlineData(true, "value8")]
        public void GenerateSchema_EnumDefaultValue_HonorsContractCamelCase(
            bool camelCaseText,
            string expectedValue)
        {
            var subject = Subject(
                configureGenerator: c =>
                {
                    c.UseInlineDefinitionsForEnums = true;
                },
                configureSerializer: c =>
                {
                    var stringEnumConverter = (camelCaseText) ? new StringEnumConverter(new CamelCaseNamingStrategy(), false) : new StringEnumConverter();
                    c.Converters.Add(stringEnumConverter);
                }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(EnumDefaultValueAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(expectedValue, ((OpenApiString)schema.Properties["IntEnumWithDefaultValue"].Default).Value);
        }

        [Theory]
        [InlineData(false, "X-foo")]
        [InlineData(true, "X-foo")]
        public void GenerateSchema_EnumDefaultValue_HonorsEnumMemberRename(
            bool camelCaseText,
            string expectedValue)
        {
            var subject = Subject(
                configureGenerator: c =>
                {
                    c.UseInlineDefinitionsForEnums = true;
                },
                configureSerializer: c =>
                {
                    var stringEnumConverter = (camelCaseText) ? new StringEnumConverter(new CamelCaseNamingStrategy(), false) : new StringEnumConverter();
                    c.Converters.Add(stringEnumConverter);
                }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(EnumDefaultValueAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(expectedValue, ((OpenApiString)schema.Properties["AnnotatedEnumWithDefaultValue"].Default).Value);
        }

        [Fact]
        public void GenerateSchema_SetsValidationProperties_IfDataAnnotatedViaMetadataType()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(DataAnnotatedViaMetadataType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(new[] { "IntWithRequired", "StringWithRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void GenerateSchema_SupportsOption_CustomTypeMappings()
        {
            var subject = Subject(
                configureGenerator: c => c.CustomTypeMappings.Add(typeof(ComplexType), () => new OpenApiSchema { Type = "string" })
            );
            var schema = subject.GenerateSchema(typeof(ComplexType), new SchemaRepository());

            Assert.Equal("string", schema.Type);
            Assert.Empty(schema.Properties);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_CustomTypeMappings_GenericType()
        {
            var subject = Subject(
                configureGenerator: c => c.CustomTypeMappings.Add(typeof(GenericType<int, string>), () => new OpenApiSchema { Type = "string" })
            );
            var schema = subject.GenerateSchema(typeof(GenericType<int, string>), new SchemaRepository());

            Assert.Equal("string", schema.Type);
            Assert.Empty(schema.Properties);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_CustomTypeMappings_OpenGenericType()
        {
            var subject = Subject(
                configureGenerator: c => c.CustomTypeMappings.Add(typeof(GenericType<,>), () => new OpenApiSchema { Type = "string" })
            );
            var schema = subject.GenerateSchema(typeof(GenericType<int, string>), new SchemaRepository());

            Assert.Equal("string", schema.Type);
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
        public void GenerateSchema_SupportsOption_IgnoreObsoleteProperties()
        {
            var subject = Subject(
                configureGenerator: c => c.IgnoreObsoleteProperties = true
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(ObsoletePropertiesType), schemaRepository);

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
            Assert.NotNull(schema.OneOf[0].Reference);
            Assert.NotNull(schema.OneOf[1].Reference);
            // The first sub type schema
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Equal("object", subType1Schema.Type);
            Assert.Equal(new[] { "Property1", "BaseProperty" }, subType1Schema.Properties.Keys);
            // The second sub type schema
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Equal("object", subType2Schema.Type);
            Assert.Equal(new[] { "Property2", "BaseProperty" }, subType2Schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_UseAllOfForInheritance()
        {
            var subject = Subject(
                configureGenerator: c => c.UseAllOfForInheritance = true
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

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
        public void GenerateSchema_SupportsOption_UseOneOfForPolymorphism_CombinedWith_UseAllOfForInheritance()
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
            Assert.NotNull(schema.OneOf[0].Reference);
            Assert.NotNull(schema.OneOf[1].Reference);
            // The base type schema
            var baseSchema = schemaRepository.Schemas["BaseType"];
            Assert.Equal("object", baseSchema.Type);
            Assert.Equal(new[] { "BaseProperty"}, baseSchema.Properties.Keys);
            // The first sub type schema
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Equal("object", subType1Schema.Type);
            Assert.NotNull(subType1Schema.AllOf);
            Assert.Equal(1, subType1Schema.AllOf.Count);
            Assert.NotNull(subType1Schema.AllOf[0].Reference);
            Assert.Equal("BaseType", subType1Schema.AllOf[0].Reference.Id);
            Assert.Equal(new[] { "Property1" }, subType1Schema.Properties.Keys);
            // The second sub type schema
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Equal("object", subType2Schema.Type);
            Assert.NotNull(subType2Schema.AllOf);
            Assert.Equal(1, subType2Schema.AllOf.Count);
            Assert.NotNull(subType2Schema.AllOf[0].Reference);
            Assert.Equal("BaseType", subType2Schema.AllOf[0].Reference.Id);
            Assert.Equal(new[] { "Property2" }, subType2Schema.Properties.Keys);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_SubTypesResolver()
        {
            var subject = Subject(configureGenerator: c =>
            {
                c.UseOneOfForPolymorphism = true;
                c.SubTypesSelector = (type) => new[] { typeof(SubType1) };
            });

            var schema = subject.GenerateSchema(typeof(BaseType), new SchemaRepository());

            // The polymorphic schema
            Assert.NotNull(schema.OneOf);
            Assert.Equal(1, schema.OneOf.Count);
        }

        [Fact]
        public void GenerateSchema_SupportsOption_DiscriminatorNameSelector()
        {
            var subject = Subject(configureGenerator: c =>
            {
                c.UseOneOfForPolymorphism = true;
                c.DiscriminatorNameSelector = (baseType) => "TypeName";
                c.DiscriminatorValueSelector = (subType) => subType.Name;
            });

            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

            // The polymorphic schema
            Assert.NotNull(schema.Discriminator);
            Assert.Equal("TypeName", schema.Discriminator.PropertyName);
            Assert.Equal(
                new Dictionary<string, string> { ["SubType1"] = "#/components/schemas/SubType1", ["SubType2"] = "#/components/schemas/SubType2" },
                schema.Discriminator.Mapping);
            // The first sub type schema
            var subType1Schema = schemaRepository.Schemas[schema.OneOf[0].Reference.Id];
            Assert.Contains("TypeName", subType1Schema.Properties.Keys);
            Assert.Contains("TypeName", subType1Schema.Required);
            // The second sub type schema
            var subType2Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
            Assert.Contains("TypeName", subType2Schema.Properties.Keys);
            Assert.Contains("TypeName", subType2Schema.Required);
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
            Assert.True(schema.Nullable);
        }

        [Fact]
        public void GenerateSchema_UseAllOfToExtendReferenceSchemas_SupportsDefault()
        {
            var subject = Subject(
                configureGenerator: c => c.UseAllOfToExtendReferenceSchemas = true
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(EnumDefaultValueAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Null(schema.Properties["IntEnumWithDefaultValue"].Type);
            Assert.IsType<OpenApiInteger>(schema.Properties["IntEnumWithDefaultValue"].Default);
            Assert.Equal(8, ((OpenApiInteger)schema.Properties["IntEnumWithDefaultValue"].Default).Value);
        }

        [Fact]
        public void GenerateSchema_UseAllOfToExtendReferenceSchemas_SupportsStringEnumDefault()
        {
            var subject = Subject(
                configureGenerator: c => c.UseAllOfToExtendReferenceSchemas = true,
                configureSerializer: c => c.Converters.Add(new StringEnumConverter())
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(EnumDefaultValueAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Null(schema.Properties["IntEnumWithDefaultValue"].Type);
            Assert.IsType<OpenApiString>(schema.Properties["IntEnumWithDefaultValue"].Default);
            Assert.Equal("Value8", ((OpenApiString)schema.Properties["IntEnumWithDefaultValue"].Default).Value);
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
        public void GenerateSchema_HandlesTypesWithNestedTypes()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(ContainingType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal("NestedType", schema.Properties["Property1"].Reference.Id);
        }

        [Fact]
        public void GenerateSchema_HandlesTypesWithOverriddenProperties()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(TypeWithOverriddenProperty), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("object", schema.Type);
            Assert.Equal("string", schema.Properties["Property1"].Type);
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
            Assert.Throws<SchemaGeneratorException>(() =>
            {
                subject.GenerateSchema(typeof(TestSupport.Namespace2.ConflictingType), schemaRepository);
            });
        }

        [Theory]
        [InlineData(false, new[] { "Value2", "Value4", "Value8" })]
        [InlineData(true, new[] { "value2", "value4", "value8" })]
        public void GenerateSchema_HonorsSerializerSetting_StringEnumConverter(
            bool camelCaseText,
            string[] expectedEnumValues)
        {
            var subject = Subject(
                configureGenerator: c => { },
                configureSerializer: c =>
                {
                    var stringEnumConverter = (camelCaseText) ? new StringEnumConverter(new CamelCaseNamingStrategy(), false) : new StringEnumConverter();
                    c.Converters.Add(stringEnumConverter);
                }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(IntEnum), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(expectedEnumValues, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Theory]
        [InlineData(TypeNameHandling.None, TypeNameAssemblyFormatHandling.Full, false,
            null)]
        [InlineData(TypeNameHandling.Arrays, TypeNameAssemblyFormatHandling.Full, false,
            null)]
        [InlineData(TypeNameHandling.Objects, TypeNameAssemblyFormatHandling.Full, true,
            "Swashbuckle.AspNetCore.TestSupport.{0}, Swashbuckle.AspNetCore.TestSupport, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")]
        [InlineData(TypeNameHandling.All, TypeNameAssemblyFormatHandling.Full, true,
            "Swashbuckle.AspNetCore.TestSupport.{0}, Swashbuckle.AspNetCore.TestSupport, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")]
        [InlineData(TypeNameHandling.Auto, TypeNameAssemblyFormatHandling.Simple, true,
            "Swashbuckle.AspNetCore.TestSupport.{0}, Swashbuckle.AspNetCore.TestSupport")]
        public void GenerateSchema_HonorsSerializerSetting_TypeNameHandling(
            TypeNameHandling typeNameHandling,
            TypeNameAssemblyFormatHandling typeNameAssemblyFormatHandling,
            bool expectedDiscriminatorPresent,
            string expectedDiscriminatorMappingKeyFormat)
        {
            var subject = Subject(
                configureGenerator: c =>
                {
                    c.UseOneOfForPolymorphism = true;
                },
                configureSerializer: c =>
                {
                    c.TypeNameHandling = typeNameHandling;
                    c.TypeNameAssemblyFormatHandling = typeNameAssemblyFormatHandling;
                }
            );
            var schemaRepository = new SchemaRepository();

            var schema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

            if (expectedDiscriminatorPresent)
            {
                Assert.NotNull(schema.Discriminator);
                Assert.Equal("$type", schema.Discriminator.PropertyName);
                var expectedDiscriminatorMapping = schema.OneOf
                    .ToDictionary(
                        possibleSchema => string.Format(expectedDiscriminatorMappingKeyFormat, possibleSchema.Reference.Id),
                        possibleSchema => possibleSchema.Reference.ReferenceV3
                    );
                Assert.Equal(expectedDiscriminatorMapping, schema.Discriminator.Mapping);
            }
            else
            {
                Assert.Null(schema.Discriminator);
                foreach (var referenceSchema in schema.OneOf)
                {
                    Assert.DoesNotContain("$type", schemaRepository.Schemas[referenceSchema.Reference.Id].Properties.Keys);
                }
            }
        }

        [Fact]
        public void GenerateSchema_HonorsSerializeSetting_ContractResolver()
        {
            var subject = Subject(
                configureSerializer: c =>
                {
                    c.ContractResolver = new AdditionalPropertyJsonContractResolver();
                }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];

            Assert.True(schema.Properties.ContainsKey(AdditionalPropertyJsonContractResolver.AdditionalPropertyName));
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_StringEnumConverter()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonConverterAnnotatedEnum), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "Value1", "Value2", "X-foo" }, schema.Enum.Cast<OpenApiString>().Select(i => i.Value));
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_JsonIgnore()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonIgnoreAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(new[] { /* "StringWithJsonIgnore" */ "StringWithNoAnnotation" }, schema.Properties.Keys.ToArray());
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_JsonProperty()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonPropertyAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(
                new[]
                {
                    "string-with-json-property-name",
                    "IntWithRequiredDefault",
                    "StringWithRequiredDefault",
                    "StringWithRequiredDisallowNull",
                    "StringWithRequiredAlways",
                    "StringWithRequiredAllowNull",
                },
                schema.Properties.Keys.ToArray()
            );
            Assert.Equal(
                new[]
                {
                    "StringWithRequiredAllowNull",
                    "StringWithRequiredAlways"
                },
                schema.Required.ToArray()
            );
            Assert.True(schema.Properties["string-with-json-property-name"].Nullable);
            Assert.False(schema.Properties["IntWithRequiredDefault"].Nullable);
            Assert.True(schema.Properties["StringWithRequiredDefault"].Nullable);
            Assert.False(schema.Properties["StringWithRequiredDisallowNull"].Nullable);
            Assert.False(schema.Properties["StringWithRequiredAlways"].Nullable);
            Assert.True(schema.Properties["StringWithRequiredAllowNull"].Nullable);
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_JsonRequired()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonRequiredAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(new[] { "StringWithJsonRequired" }, schema.Required.ToArray());
            Assert.False(schema.Properties["StringWithJsonRequired"].Nullable);
        }

        [Fact]
        public void GenerateSchema_HonorsSerializerAttribute_JsonObject()
        {
            var schemaRepository = new SchemaRepository();

            var referenceSchema = Subject().GenerateSchema(typeof(JsonObjectAnnotatedType), schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.Equal(
                new[]
                {
                    "StringWithNoAnnotation",
                    "StringWithRequiredAllowNull",
                    "StringWithRequiredUnspecified"
                },
                schema.Required.ToArray()
            );
            Assert.False(schema.Properties["StringWithNoAnnotation"].Nullable);
            Assert.False(schema.Properties["StringWithRequiredUnspecified"].Nullable);
            Assert.True(schema.Properties["StringWithRequiredAllowNull"].Nullable);
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
        [InlineData(typeof(ProblemDetails))]
        [InlineData(typeof(ValidationProblemDetails))]
        public void GenerateSchema_HonorsSerializerSetting_ProblemDetailsConverter(Type type)
        {
            var subject = Subject(
                configureGenerator: c => { },
                configureSerializer: c =>
                {
                    c.Converters.Add(new ProblemDetailsConverter());
                }
            );
            var schemaRepository = new SchemaRepository();

            var referenceSchema = subject.GenerateSchema(type, schemaRepository);

            var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
            Assert.DoesNotContain("Extensions", schema.Properties.Keys);
            Assert.True(schema.AdditionalPropertiesAllowed);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Null(schema.AdditionalProperties.Type);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(JToken))]
        [InlineData(typeof(JObject))]
        [InlineData(typeof(JArray))]
        public void GenerateSchema_GeneratesOpenSchema_IfDynamicJsonType(Type type)
        {
            var schema = Subject().GenerateSchema(type, new SchemaRepository());

            Assert.Null(schema.Reference);
            Assert.Null(schema.Type);
        }

        private SchemaGenerator Subject(
            Action<SchemaGeneratorOptions> configureGenerator = null,
            Action<JsonSerializerSettings> configureSerializer = null)
        {
            var generatorOptions = new SchemaGeneratorOptions();
            configureGenerator?.Invoke(generatorOptions);

            var serializerSettings = new JsonSerializerSettings();
            configureSerializer?.Invoke(serializerSettings);

            return new SchemaGenerator(generatorOptions, new NewtonsoftDataContractResolver(generatorOptions, serializerSettings));
        }
    }
}
