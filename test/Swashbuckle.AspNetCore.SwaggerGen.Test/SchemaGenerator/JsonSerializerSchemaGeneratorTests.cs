using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Swashbuckle.AspNetCore.TestSupport;

using JsonSchemaType = string;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class JsonSerializerSchemaGeneratorTests
{
    [Theory]
    [InlineData(typeof(IFormFile))]
    [InlineData(typeof(FileResult))]
    [InlineData(typeof(System.IO.Stream))]
    [InlineData(typeof(System.IO.Pipelines.PipeReader))]
    public void GenerateSchema_GeneratesFileSchema_BinaryStringResultType(Type type)
    {
        var schema = Subject().GenerateSchema(type, new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.String, schema.Type);
        Assert.Equal("binary", schema.Format);
    }

    public static TheoryData<Type, JsonSchemaType, string> PrimitiveTypeData => new()
    {
        { typeof(bool), JsonSchemaTypes.Boolean, null },
        { typeof(byte), JsonSchemaTypes.Integer, "int32" },
        { typeof(sbyte), JsonSchemaTypes.Integer, "int32" },
        { typeof(short), JsonSchemaTypes.Integer, "int32" },
        { typeof(ushort), JsonSchemaTypes.Integer, "int32" },
        { typeof(int), JsonSchemaTypes.Integer, "int32" },
        { typeof(uint), JsonSchemaTypes.Integer, "int32" },
        { typeof(long), JsonSchemaTypes.Integer, "int64" },
        { typeof(ulong), JsonSchemaTypes.Integer, "int64" },
        { typeof(float), JsonSchemaTypes.Number, "float" },
        { typeof(double), JsonSchemaTypes.Number, "double" },
        { typeof(decimal), JsonSchemaTypes.Number, "double" },
        { typeof(string), JsonSchemaTypes.String, null },
        { typeof(char), JsonSchemaTypes.String, null },
        { typeof(byte[]), JsonSchemaTypes.String, "byte" },
        { typeof(DateTime), JsonSchemaTypes.String, "date-time" },
        { typeof(DateTimeOffset), JsonSchemaTypes.String, "date-time" },
        { typeof(TimeSpan), JsonSchemaTypes.String, "date-span" },
        { typeof(Guid), JsonSchemaTypes.String, "uuid" },
        { typeof(Uri), JsonSchemaTypes.String, "uri" },
        { typeof(Version), JsonSchemaTypes.String, null },
        { typeof(DateOnly), JsonSchemaTypes.String, "date" },
        { typeof(TimeOnly), JsonSchemaTypes.String, "time" },
        { typeof(bool?), JsonSchemaTypes.Boolean, null },
        { typeof(int?), JsonSchemaTypes.Integer, "int32" },
        { typeof(DateTime?), JsonSchemaTypes.String, "date-time" },
        { typeof(Guid?), JsonSchemaTypes.String, "uuid" },
        { typeof(DateOnly?), JsonSchemaTypes.String, "date" },
        { typeof(TimeOnly?), JsonSchemaTypes.String, "time" },
#if NET
        { typeof(Int128), JsonSchemaTypes.Integer, "int128" },
        { typeof(Int128?), JsonSchemaTypes.Integer, "int128" },
        { typeof(UInt128), JsonSchemaTypes.Integer, "int128" },
        { typeof(UInt128?), JsonSchemaTypes.Integer, "int128" },
#endif
    };

    [Theory]
    [MemberData(nameof(PrimitiveTypeData))]
    public void GenerateSchema_GeneratesPrimitiveSchema_IfPrimitiveOrNullablePrimitiveType(
        Type type,
        JsonSchemaType expectedSchemaType,
        string expectedFormat)
    {
        var schema = Subject().GenerateSchema(type, new SchemaRepository());

        Assert.Equal(expectedSchemaType, schema.Type);
        Assert.Equal(expectedFormat, schema.Format);
    }

    [Theory]
    [InlineData(typeof(IntEnum), "int32", "2", "4", "8")]
    [InlineData(typeof(LongEnum), "int64", "2", "4", "8")]
    [InlineData(typeof(IntEnum?), "int32", "2", "4", "8")]
    [InlineData(typeof(LongEnum?), "int64", "2", "4", "8")]
    public void GenerateSchema_GeneratesReferencedEnumSchema_IfEnumOrNullableEnumType(
        Type type,
        string expectedFormat,
        params string[] expectedEnumAsJson)
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

        Assert.NotNull(referenceSchema.Reference);
        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Integer, schema.Type);
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

    public static TheoryData<Type, JsonSchemaType> CollectionTypeData => new()
    {
        { typeof(IDictionary<string, int>), JsonSchemaTypes.Integer },
        { typeof(IDictionary<EmptyIntEnum, int>), JsonSchemaTypes.Integer },
        { typeof(IReadOnlyDictionary<string, bool>), JsonSchemaTypes.Boolean },
        { typeof(IDictionary), null },
        { typeof(ExpandoObject), null },
    };

    [Theory]
    [MemberData(nameof(CollectionTypeData))]
    public void GenerateSchema_GeneratesDictionarySchema_IfDictionaryType(
        Type type,
        JsonSchemaType expectedAdditionalPropertiesType)
    {
        var schema = Subject().GenerateSchema(type, new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
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
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.True(schema.AdditionalPropertiesAllowed);
        Assert.NotNull(schema.AdditionalProperties);
        Assert.Equal(schema.AdditionalProperties.Reference.Id, referenceSchema.Reference.Id); // ref to self
    }

    public static TheoryData<Type, JsonSchemaType, string> EnumerableTypeData => new()
    {
        { typeof(int[]), JsonSchemaTypes.Integer, "int32" },
        { typeof(IEnumerable<string>), JsonSchemaTypes.String, null },
        { typeof(DateTime?[]), JsonSchemaTypes.String, "date-time" },
        { typeof(int[][]), JsonSchemaTypes.Array, null },
        { typeof(IList), null, null },
    };

    [Theory]
    [MemberData(nameof(EnumerableTypeData))]
    public void GenerateSchema_GeneratesArraySchema_IfEnumerableType(
        Type type,
        JsonSchemaType expectedItemsType,
        string expectedItemsFormat)
    {
        var schema = Subject().GenerateSchema(type, new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
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

        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
        Assert.True(schema.UniqueItems);
    }

    [Fact]
    public void GenerateSchema_GeneratesReferencedArraySchema_IfEnumerableTypeIsSelfReferencing()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(ListOfSelf), schemaRepository);

        Assert.NotNull(referenceSchema.Reference);
        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
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
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal(expectedProperties, schema.Properties.Keys);
        Assert.False(schema.AdditionalPropertiesAllowed);
    }

    [Fact]
    public void GenerateSchema_IncludesInheritedProperties_IfComplexTypeIsDerived()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(SubType1), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal(["BaseProperty", "Property1"], schema.Properties.Keys);
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
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal(expectedPropertyNames.OrderBy(n => n), schema.Properties.Keys.OrderBy(k => k));
    }

    [Fact]
    public void GenerateSchema_KeepMostDerivedType_IfTypeIsAnInterface()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(INewBaseInterface), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Integer, schema.Properties["BaseProperty"].Type);
    }

    [Fact]
    public void GenerateSchema_ExcludesIndexerProperties_IfComplexTypeIsIndexed()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(IndexedType), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal(["Property1"], schema.Properties.Keys);
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
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.DoubleWithDefaultOfDifferentType), "1")]
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.StringWithDefault), "\"foobar\"")]
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.IntArrayWithDefault), "[\n  1,\n  2,\n  3\n]")]
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.StringArrayWithDefault), "[\n  \"foo\",\n  \"bar\"\n]")]
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.NullableIntWithDefaultNullValue), "null")]
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.NullableIntWithDefaultValue), "2147483647")]
    public void GenerateSchema_SetsDefault_IfPropertyHasDefaultValueAttribute(
        Type declaringType,
        string propertyName,
        string expectedDefaultAsJson)
    {
        // Arrange
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var schemaRepository = new SchemaRepository();

        // Act
        var referenceSchema = Subject().GenerateSchema(declaringType, schemaRepository);

        // Assert
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
#if NET
        Assert.Equal(1, schema.Properties["StringWithLength"].MinLength);
        Assert.Equal(3, schema.Properties["StringWithLength"].MaxLength);
        Assert.Equal(1, schema.Properties["ArrayWithLength"].MinItems);
        Assert.Equal(3, schema.Properties["ArrayWithLength"].MaxItems);
        Assert.Equal(true, schema.Properties["IntWithExclusiveRange"].ExclusiveMinimum);
        Assert.Equal(true, schema.Properties["IntWithExclusiveRange"].ExclusiveMaximum);
        Assert.Equal("byte", schema.Properties["StringWithBase64"].Format);
        Assert.Equal(JsonSchemaTypes.String, schema.Properties["StringWithBase64"].Type);
#endif
        Assert.Null(schema.Properties["IntWithRange"].ExclusiveMinimum);
        Assert.Null(schema.Properties["IntWithRange"].ExclusiveMaximum);
        Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
        Assert.Equal(10, schema.Properties["IntWithRange"].Maximum);
        Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
        Assert.Equal(5, schema.Properties["StringWithStringLength"].MinLength);
        Assert.Equal(10, schema.Properties["StringWithStringLength"].MaxLength);
        Assert.Equal(1, schema.Properties["StringWithRequired"].MinLength);
        Assert.False(schema.Properties["StringWithRequired"].Nullable);
        Assert.False(schema.Properties["StringWithRequiredAllowEmptyTrue"].Nullable);
        Assert.Null(schema.Properties["StringWithRequiredAllowEmptyTrue"].MinLength);
        Assert.Equal(["StringWithRequired", "StringWithRequiredAllowEmptyTrue"], schema.Required);
        Assert.Equal("Description", schema.Properties[nameof(TypeWithValidationAttributes.StringWithDescription)].Description);
        Assert.True(schema.Properties[nameof(TypeWithValidationAttributes.StringWithReadOnly)].ReadOnly);
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

#if NET
    public class TypeWithRequiredProperties
    {
        public required string RequiredString { get; set; }
        public required int RequiredInt { get; set; }
    }

    public class TypeWithRequiredPropertyAndValidationAttribute
    {
        [MinLength(1)]
        public required string RequiredProperty { get; set; }
    }

    [Fact]
    public void GenerateSchema_SetsRequired_IfPropertyHasRequiredKeyword()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(TypeWithRequiredProperties), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.True(schema.Properties["RequiredString"].Nullable);
        Assert.Contains("RequiredString", schema.Required.ToArray());
        Assert.False(schema.Properties["RequiredInt"].Nullable);
        Assert.Contains("RequiredInt", schema.Required.ToArray());
    }

    [Fact]
    public void GenerateSchema_SetsRequired_IfPropertyHasRequiredKeywordAndValidationAttribute()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(TypeWithRequiredPropertyAndValidationAttribute), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(1, schema.Properties["RequiredProperty"].MinLength);
        Assert.True(schema.Properties["RequiredProperty"].Nullable);
        Assert.Equal(["RequiredProperty"], schema.Required);
    }

#nullable enable
    public class TypeWithNullableReferenceTypes
    {
        public required string? RequiredNullableString { get; set; }
        public required string RequiredNonNullableString { get; set; }
    }

    [Fact]
    public void GenerateSchema_SetsRequiredAndNullable_IfPropertyHasRequiredKeywordAndIsNullable()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject(configureGenerator: (c) => c.SupportNonNullableReferenceTypes = true).GenerateSchema(typeof(TypeWithNullableReferenceTypes), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.True(schema.Properties["RequiredNullableString"].Nullable);
        Assert.Contains("RequiredNullableString", schema.Required.ToArray());
        Assert.False(schema.Properties["RequiredNonNullableString"].Nullable);
        Assert.Contains("RequiredNonNullableString", schema.Required.ToArray());
    }
#nullable disable
#endif

    [Theory]
    [InlineData(typeof(TypeWithParameterizedConstructor), nameof(TypeWithParameterizedConstructor.Id), false)]
    [InlineData(typeof(TypeWithParameterlessAndParameterizedConstructor), nameof(TypeWithParameterlessAndParameterizedConstructor.Id), true)]
    [InlineData(typeof(TypeWithParameterlessAndJsonAnnotatedConstructor), nameof(TypeWithParameterlessAndJsonAnnotatedConstructor.Id), false)]
    public void GenerateSchema_DoesNotSetReadOnlyFlag_IfPropertyIsReadOnlyButCanBeSetViaConstructor(
        Type type,
        string propertyName,
        bool expectedReadOnly
    )
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(type, schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(expectedReadOnly, schema.Properties[propertyName].ReadOnly);
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
    [InlineData(typeof(ComplexType), typeof(ComplexType))]
    [InlineData(typeof(GenericType<int, string>), typeof(GenericType<int, string>))]
    [InlineData(typeof(GenericType<,>), typeof(GenericType<int, int>))]
    public void GenerateSchema_SupportsOption_CustomTypeMappings(
        Type mappingType,
        Type type)
    {
        var subject = Subject(
            configureGenerator: c => c.CustomTypeMappings.Add(mappingType, () => new OpenApiSchema { Type = JsonSchemaTypes.String })
        );
        var schema = subject.GenerateSchema(type, new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.String, schema.Type);
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
        Assert.NotNull(schema.AllOf);
        Assert.Equal(2, schema.AllOf.Count);
        var baseSchema = schema.AllOf[0];
        Assert.Equal("BaseType", baseSchema.Reference.Id);
        Assert.NotNull(baseSchema.Reference);
        var subSchema = schema.AllOf[1];
        Assert.Equal(["Property1"], subSchema.Properties.Keys);
        // The base type schema
        var baseTypeSchema = schemaRepository.Schemas[baseSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, baseTypeSchema.Type);
        Assert.Equal(["BaseProperty"], baseTypeSchema.Properties.Keys);
    }

    [Fact]
    public void GenerateSchema_SupportsOption_SubTypesSelector()
    {
        var subject = Subject(configureGenerator: c =>
        {
            c.UseAllOfForInheritance = true;
            c.SubTypesSelector = (type) => [typeof(SubType1)];
        });
        var schemaRepository = new SchemaRepository();

        var schema = subject.GenerateSchema(typeof(BaseType), schemaRepository);

        Assert.Equal(["SubType1", "BaseType"], schemaRepository.Schemas.Keys);

        var subSchema = schemaRepository.Schemas["SubType1"];
        Assert.NotNull(subSchema.AllOf);
        Assert.Equal(2, subSchema.AllOf.Count);
    }

    [Fact]
    public void GenerateSchema_SecondLevelInheritance_SubTypesSelector()
    {
        var subject = Subject(configureGenerator: c =>
        {
            c.UseAllOfForInheritance = true;
            c.SubTypesSelector = (type) => type == typeof(BaseSecondLevelType) ? [typeof(SubSubSecondLevelType)] : Array.Empty<Type>();
        });
        var schemaRepository = new SchemaRepository();

        var schema = subject.GenerateSchema(typeof(BaseSecondLevelType), schemaRepository);

        Assert.Equal(["SubSubSecondLevelType", "BaseSecondLevelType"], schemaRepository.Schemas.Keys);

        var subSchema = schemaRepository.Schemas["SubSubSecondLevelType"];
        Assert.NotNull(subSchema.AllOf);
        Assert.Equal(2, subSchema.AllOf.Count);
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
        Assert.Equal(JsonSchemaTypes.Object, baseSchema.Type);
        Assert.Equal(["BaseProperty"], baseSchema.Properties.Keys);
        // The first sub type schema
        Assert.NotNull(schema.OneOf[1].Reference);
        var subType1Schema = schemaRepository.Schemas[schema.OneOf[1].Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, subType1Schema.Type);
        Assert.NotNull(subType1Schema.AllOf);
        var allOf = Assert.Single(subType1Schema.AllOf);
        Assert.NotNull(allOf.Reference);
        Assert.Equal("BaseType", allOf.Reference.Id);
        Assert.Equal(["Property1"], subType1Schema.Properties.Keys);
        // The second sub type schema
        Assert.NotNull(schema.OneOf[2].Reference);
        var subType2Schema = schemaRepository.Schemas[schema.OneOf[2].Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, subType2Schema.Type);
        Assert.NotNull(subType2Schema.AllOf);
        allOf = Assert.Single(subType2Schema.AllOf);
        Assert.NotNull(allOf.Reference);
        Assert.Equal("BaseType", allOf.Reference.Id);
        Assert.Equal(["Property2"], subType2Schema.Properties.Keys);
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
        Assert.Single(schema.AllOf);
    }

    [Fact]
    public void GenerateSchema_SupportsOption_UseInlineDefinitionsForEnums()
    {
        var subject = Subject(
            configureGenerator: c => c.UseInlineDefinitionsForEnums = true
        );

        var schema = subject.GenerateSchema(typeof(IntEnum), new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.Integer, schema.Type);
        Assert.NotNull(schema.Enum);
    }

    [Fact]
    public void TypeWithNullableContextAnnotated_IsAnnotated()
    {
        // This is a sanity check to ensure that TypeWithNullableContextAnnotated
        // is annotated with NullableContext(Flag=2) by the compiler. If this is no
        // longer the case, you may need to add more of the Dummy properties to
        // coerce the compiler.

        const string Name = "System.Runtime.CompilerServices.NullableContextAttribute";

        var nullableContext = typeof(TypeWithNullableContextAnnotated)
            .GetCustomAttributes()
            .FirstOrDefault(attr => string.Equals(attr.GetType().FullName, Name));

        Assert.NotNull(nullableContext);

        var flag = nullableContext?.GetType().GetField("Flag")?.GetValue(nullableContext);

        Assert.Equal((byte)2, flag);
    }

    [Fact]
    public void TypeWithNullableContextNotAnnotated_IsNotAnnotated()
    {
        // This is a sanity check to ensure that TypeWithNullableContextNotAnnotated
        // is annotated with NullableContext(Flag=1) by the compiler. If this is no
        // longer the case, you may need to add more of the Dummy properties to
        // coerce the compiler.

        const string Name = "System.Runtime.CompilerServices.NullableContextAttribute";

        var nullableContext = typeof(TypeWithNullableContextNotAnnotated)
            .GetCustomAttributes()
            .FirstOrDefault(attr => string.Equals(attr.GetType().FullName, Name));

        Assert.NotNull(nullableContext);

        var flag = nullableContext?.GetType().GetField("Flag")?.GetValue(nullableContext);

        Assert.Equal((byte)1, flag);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableInt), true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableInt), false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableString), true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableString), false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableArray), true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableArray), false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableList), true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableList), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableInt), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableInt), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableString), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableString), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableArray), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableArray), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableList), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableList), false)]
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
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableDictionaryInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableDictionaryInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableDictionaryInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableDictionaryInNullableContent), true, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableDictionaryInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableDictionaryInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableDictionaryInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableDictionaryInNullableContent), true, true)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NullableAttribute_Compiler_Optimizations_Situations_Dictionary(
        Type declaringType,
        string propertyName,
        bool expectedNullableProperty,
        bool expectedNullableContent)
    {
        var subject = Subject(
            configureGenerator: c => c.SupportNonNullableReferenceTypes = true
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = subject.GenerateSchema(declaringType, schemaRepository);

        var propertySchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName];
        var contentSchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName].AdditionalProperties;
        Assert.Equal(expectedNullableProperty, propertySchema.Nullable);
        Assert.Equal(expectedNullableContent, contentSchema.Nullable);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIDictionaryInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIDictionaryInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIDictionaryInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIDictionaryInNullableContent), true, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIDictionaryInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIDictionaryInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIDictionaryInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIDictionaryInNullableContent), true, true)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NullableAttribute_Compiler_Optimizations_Situations_IDictionary(
        Type declaringType,
        string propertyName,
        bool expectedNullableProperty,
        bool expectedNullableContent)
    {
        var subject = Subject(
            configureGenerator: c => c.SupportNonNullableReferenceTypes = true
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = subject.GenerateSchema(declaringType, schemaRepository);

        var propertySchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName];
        var contentSchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName].AdditionalProperties;
        Assert.Equal(expectedNullableProperty, propertySchema.Nullable);
        Assert.Equal(expectedNullableContent, contentSchema.Nullable);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIReadOnlyDictionaryInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIReadOnlyDictionaryInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIReadOnlyDictionaryInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIReadOnlyDictionaryInNullableContent), true, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIReadOnlyDictionaryInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIReadOnlyDictionaryInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIReadOnlyDictionaryInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIReadOnlyDictionaryInNullableContent), true, true)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NullableAttribute_Compiler_Optimizations_Situations_IReadOnlyDictionary(
        Type declaringType,
        string propertyName,
        bool expectedNullableProperty,
        bool expectedNullableContent)
    {
        var subject = Subject(
            configureGenerator: c => c.SupportNonNullableReferenceTypes = true
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = subject.GenerateSchema(declaringType, schemaRepository);

        var propertySchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName];
        var contentSchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName].AdditionalProperties;
        Assert.Equal(expectedNullableProperty, propertySchema.Nullable);
        Assert.Equal(expectedNullableContent, contentSchema.Nullable);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableDictionaryWithValueTypeInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableDictionaryWithValueTypeInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableDictionaryWithValueTypeInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableDictionaryWithValueTypeInNullableContent), true, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableDictionaryWithValueTypeInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableDictionaryWithValueTypeInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableDictionaryWithValueTypeInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableDictionaryWithValueTypeInNullableContent), true, true)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NullableAttribute_Compiler_Optimizations_Situations_DictionaryWithValueType(
        Type declaringType,
        string propertyName,
        bool expectedNullableProperty,
        bool expectedNullableContent)
    {
        var subject = Subject(
            configureGenerator: c => c.SupportNonNullableReferenceTypes = true
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = subject.GenerateSchema(declaringType, schemaRepository);

        var propertySchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName];
        var contentSchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName].AdditionalProperties;
        Assert.Equal(expectedNullableProperty, propertySchema.Nullable);
        Assert.Equal(expectedNullableContent, contentSchema.Nullable);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIDictionaryWithValueTypeInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIDictionaryWithValueTypeInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIDictionaryWithValueTypeInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIDictionaryWithValueTypeInNullableContent), true, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIDictionaryWithValueTypeInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIDictionaryWithValueTypeInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIDictionaryWithValueTypeInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIDictionaryWithValueTypeInNullableContent), true, true)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NullableAttribute_Compiler_Optimizations_Situations_IDictionaryWithValueType(
        Type declaringType,
        string propertyName,
        bool expectedNullableProperty,
        bool expectedNullableContent)
    {
        var subject = Subject(
            configureGenerator: c => c.SupportNonNullableReferenceTypes = true
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = subject.GenerateSchema(declaringType, schemaRepository);

        var propertySchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName];
        var contentSchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName].AdditionalProperties;
        Assert.Equal(expectedNullableProperty, propertySchema.Nullable);
        Assert.Equal(expectedNullableContent, contentSchema.Nullable);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIReadOnlyDictionaryWithValueTypeInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIReadOnlyDictionaryWithValueTypeInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NonNullableIReadOnlyDictionaryWithValueTypeInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.NullableIReadOnlyDictionaryWithValueTypeInNullableContent), true, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIReadOnlyDictionaryWithValueTypeInNonNullableContent), true, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIReadOnlyDictionaryWithValueTypeInNonNullableContent), false, false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NonNullableIReadOnlyDictionaryWithValueTypeInNullableContent), false, true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.NullableIReadOnlyDictionaryWithValueTypeInNullableContent), true, true)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NullableAttribute_Compiler_Optimizations_Situations_IReadOnlyDictionaryWithValueType(
        Type declaringType,
        string propertyName,
        bool expectedNullableProperty,
        bool expectedNullableContent)
    {
        var subject = Subject(
            configureGenerator: c => c.SupportNonNullableReferenceTypes = true
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = subject.GenerateSchema(declaringType, schemaRepository);

        var propertySchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName];
        var contentSchema = schemaRepository.Schemas[referenceSchema.Reference.Id].Properties[propertyName].AdditionalProperties;
        Assert.Equal(expectedNullableProperty, propertySchema.Nullable);
        Assert.Equal(expectedNullableContent, contentSchema.Nullable);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithOneNullableContent), nameof(TypeWithNullableContextAnnotated.NullableString), true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextAnnotated.NonNullableString), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithOneNullableContent), nameof(TypeWithNullableContextNotAnnotated.NullableString), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextNotAnnotated.NonNullableString), false)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypesInDictionary_NullableAttribute_Compiler_Optimizations_Situations(
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

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithOneNullableContent), nameof(TypeWithNullableContextAnnotated.NullableString), false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextAnnotated.NonNullableString), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithOneNullableContent), nameof(TypeWithNullableContextNotAnnotated.NullableString), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextNotAnnotated.NonNullableString), true)]
    public void GenerateSchema_SupportsOption_NonNullableReferenceTypesAsRequired_RequiredAttribute_Compiler_Optimizations_Situations(
        Type declaringType,
        string subType,
        string propertyName,
        bool required)
    {
        var subject = Subject(
            configureGenerator: c => c.NonNullableReferenceTypesAsRequired = true
        );
        var schemaRepository = new SchemaRepository();

        subject.GenerateSchema(declaringType, schemaRepository);

        var propertyIsRequired = schemaRepository.Schemas[subType].Required.Contains(propertyName);
        Assert.Equal(required, propertyIsRequired);
    }

    [Obsolete($"{nameof(IOptions<MvcOptions>)} is not used.")]
    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextAnnotated.NonNullableString), false)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextAnnotated.NonNullableString), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextNotAnnotated.NonNullableString), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithOneNonNullableContent), nameof(TypeWithNullableContextNotAnnotated.NonNullableString), true)]
    public void GenerateSchema_SupportsOption_SuppressImplicitRequiredAttributeForNonNullableReferenceTypes(
        Type declaringType,
        string subType,
        string propertyName,
        bool suppress)
    {
        var subject = Subject(
            configureGenerator: c => c.NonNullableReferenceTypesAsRequired = true,
            configureMvcOptions: o => o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = suppress
        );
        var schemaRepository = new SchemaRepository();

        subject.GenerateSchema(declaringType, schemaRepository);

        var propertyIsRequired = schemaRepository.Schemas[subType].Required.Contains(propertyName);
        Assert.True(propertyIsRequired);
    }

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithNestedSubType.Nested), nameof(TypeWithNullableContextAnnotated.SubTypeWithNestedSubType.Nested.NullableString), true)]
    [InlineData(typeof(TypeWithNullableContextAnnotated), nameof(TypeWithNullableContextAnnotated.SubTypeWithNestedSubType.Nested), nameof(TypeWithNullableContextAnnotated.SubTypeWithNestedSubType.Nested.NonNullableString), false)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithNestedSubType.Nested), nameof(TypeWithNullableContextAnnotated.SubTypeWithNestedSubType.Nested.NullableString), true)]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated), nameof(TypeWithNullableContextNotAnnotated.SubTypeWithNestedSubType.Nested), nameof(TypeWithNullableContextAnnotated.SubTypeWithNestedSubType.Nested.NonNullableString), false)]
    public void GenerateSchema_SupportsOption_SupportNonNullableReferenceTypes_NestedWithinNested(
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

    [Theory]
    [InlineData(typeof(TypeWithNullableContextAnnotated))]
    [InlineData(typeof(TypeWithNullableContextNotAnnotated))]
    public void GenerateSchema_Works_IfNotProvidingMvcOptions(Type type)
    {
        var generatorOptions = new SchemaGeneratorOptions
        {
            NonNullableReferenceTypesAsRequired = true
        };

        var serializerOptions = new JsonSerializerOptions();

        var subject = new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(serializerOptions));
        var schemaRepository = new SchemaRepository();

        subject.GenerateSchema(type, schemaRepository);

        var subType = nameof(TypeWithNullableContextAnnotated.SubTypeWithOneNonNullableContent);
        var propertyName = nameof(TypeWithNullableContextAnnotated.NonNullableString);
        var propertyIsRequired = schemaRepository.Schemas[subType].Required.Contains(propertyName);
        Assert.True(propertyIsRequired);
    }

    [Fact]
    public void GenerateSchema_HandlesTypesWithNestedTypes()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(ContainingType), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal("NestedType", schema.Properties["Property1"].Reference.Id);
    }

    [Fact]
    public void GenerateSchema_HandlesSquareArray()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(string[,]), schemaRepository);

        Assert.NotNull(referenceSchema.Items);
        Assert.NotNull(referenceSchema.Items.Type);
        Assert.Equal(JsonSchemaTypes.String, referenceSchema.Items.Type);
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
        Assert.Equal(["property1", "property2"], schema.Properties.Keys);
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
        Assert.Equal(JsonSchemaTypes.String, propertySchema.Type);
        Assert.Equal(expectedEnumAsJson, propertySchema.Enum.Select(openApiAny => openApiAny.ToJson()));
        Assert.Equal(expectedDefaultAsJson, propertySchema.Default.ToJson());
    }

    [Fact]
    public void GenerateSchema_HonorsEnumDictionaryKeys_StringEnumConverter()
    {
        var subject = Subject();
        var schemaRepository = new SchemaRepository();

        var referenceSchema = subject.GenerateSchema(typeof(Dictionary<IntEnum, string>), schemaRepository);

        Assert.Equal(typeof(IntEnum).GetEnumNames(), referenceSchema.Properties.Keys);
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_StringEnumConverter()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(JsonConverterAnnotatedEnum), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.String, schema.Type);
        Assert.Equal(["\"Value1\"", "\"Value2\"", "\"X\""], schema.Enum.Select(openApiAny => openApiAny.ToJson()));
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttributes_JsonIgnore()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(JsonIgnoreAnnotatedType), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        string[] expectedKeys =
        [
            nameof(JsonIgnoreAnnotatedType.StringWithJsonIgnoreConditionNever),
            nameof(JsonIgnoreAnnotatedType.StringWithJsonIgnoreConditionWhenWritingDefault),
            nameof(JsonIgnoreAnnotatedType.StringWithJsonIgnoreConditionWhenWritingNull),
            nameof(JsonIgnoreAnnotatedType.StringWithNoAnnotation)
        ];

        Assert.Equal(expectedKeys, schema.Properties.Keys);
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_JsonPropertyName()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(JsonPropertyNameAnnotatedType), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(["string-with-json-property-name"], schema.Properties.Keys);
    }

#if NET
    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_JsonRequired()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(JsonRequiredAnnotatedType), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(["StringWithJsonRequired"], schema.Required);
        Assert.True(schema.Properties["StringWithJsonRequired"].Nullable);
    }
#endif

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

    [Fact]
    public void GenerateSchema_HonorsAttribute_SwaggerIgnore()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(SwaggerIngoreAnnotatedType), schemaRepository);

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        Assert.True(schema.Properties.ContainsKey(nameof(SwaggerIngoreAnnotatedType.NotIgnoredString)));
        Assert.False(schema.Properties.ContainsKey(nameof(SwaggerIngoreAnnotatedType.IgnoredString)));
        Assert.False(schema.Properties.ContainsKey(nameof(SwaggerIngoreAnnotatedType.IgnoredExtensionData)));
        Assert.False(schema.AdditionalPropertiesAllowed);
        Assert.Null(schema.AdditionalProperties);
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

    [Fact]
    public void GenerateSchema_GeneratesSchema_IfParameterHasMaxLengthRouteConstraint()
    {
        var maxLength = 3;
        var constraints = new List<IRouteConstraint>()
        {
            new MaxLengthRouteConstraint(maxLength)
        };
        var routeInfo = new ApiParameterRouteInfo
        {
            Constraints = constraints
        };
        var parameterInfo = typeof(FakeController)
            .GetMethod(nameof(FakeController.ActionWithParameter))
            .GetParameters()
            .First();
        var schema = Subject().GenerateSchema(typeof(string), new SchemaRepository(), parameterInfo: parameterInfo, routeInfo: routeInfo);
        Assert.Equal(maxLength, schema.MaxLength);
    }

    [Fact]
    public void GenerateSchema_GeneratesSchema_IfParameterHasMultipleConstraints()
    {
        var maxLength = 3;
        var minLength = 1;
        var constraints = new List<IRouteConstraint>()
        {
            new MaxLengthRouteConstraint(3),
            new MinLengthRouteConstraint(minLength)
        };
        var routeInfo = new ApiParameterRouteInfo
        {
            Constraints = constraints
        };
        var parameterInfo = typeof(FakeController)
            .GetMethod(nameof(FakeController.ActionWithParameter))
            .GetParameters()
            .First();
        var schema = Subject().GenerateSchema(typeof(string), new SchemaRepository(), parameterInfo: parameterInfo, routeInfo: routeInfo);
        Assert.Equal(maxLength, schema.MaxLength);
        Assert.Equal(minLength, schema.MinLength);
    }

    [Fact]
    public void GenerateSchema_GeneratesSchema_IfParameterHasTypeConstraints()
    {
        var constraints = new List<IRouteConstraint>()
        {
            new IntRouteConstraint(),
        };
        var routeInfo = new ApiParameterRouteInfo
        {
            Constraints = constraints
        };
        var parameterInfo = typeof(FakeController)
            .GetMethod(nameof(FakeController.ActionWithParameter))
            .GetParameters()
            .First();
        var schema = Subject().GenerateSchema(typeof(string), new SchemaRepository(), parameterInfo: parameterInfo, routeInfo: routeInfo);
        Assert.Equal(JsonSchemaTypes.Integer, schema.Type);
    }

    private static SchemaGenerator Subject(
        Action<SchemaGeneratorOptions> configureGenerator = null,
        Action<JsonSerializerOptions> configureSerializer = null)
    {
        var generatorOptions = new SchemaGeneratorOptions();
        configureGenerator?.Invoke(generatorOptions);

        var serializerOptions = new JsonSerializerOptions();
        configureSerializer?.Invoke(serializerOptions);

        return new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(serializerOptions));
    }

    [Obsolete($"{nameof(IOptions<MvcOptions>)} is not used.")]
    private static SchemaGenerator Subject(
        Action<SchemaGeneratorOptions> configureGenerator,
        Action<MvcOptions> configureMvcOptions)
    {
        var generatorOptions = new SchemaGeneratorOptions();
        configureGenerator?.Invoke(generatorOptions);

        var serializerOptions = new JsonSerializerOptions();

        var mvcOptions = new MvcOptions();
        configureMvcOptions?.Invoke(mvcOptions);

        return new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(serializerOptions), Options.Create(mvcOptions));
    }
}
