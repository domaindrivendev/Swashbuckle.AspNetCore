using System.Collections;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class NewtonsoftSchemaGeneratorTests
{
    [Theory]
    [InlineData(typeof(IFormFile))]
    [InlineData(typeof(FileResult))]
    [InlineData(typeof(System.IO.Stream))]
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
        { typeof(bool?), JsonSchemaTypes.Boolean | JsonSchemaType.Null, null },
        { typeof(int?), JsonSchemaTypes.Integer | JsonSchemaType.Null, "int32" },
        { typeof(DateTime?), JsonSchemaTypes.String | JsonSchemaType.Null, "date-time" },
        { typeof(Guid?), JsonSchemaTypes.String | JsonSchemaType.Null, "uuid" },
        { typeof(DateOnly?), JsonSchemaTypes.String | JsonSchemaType.Null, "date" },
        { typeof(TimeOnly?), JsonSchemaTypes.String | JsonSchemaType.Null, "time" },
#if NET
        { typeof(Int128), JsonSchemaTypes.Integer, "int128" },
        { typeof(Int128?), JsonSchemaTypes.Integer | JsonSchemaType.Null, "int128" },
        { typeof(UInt128), JsonSchemaTypes.Integer, "int128" },
        { typeof(UInt128?), JsonSchemaTypes.Integer | JsonSchemaType.Null, "int128" },
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

    public static TheoryData<Type, JsonSchemaType, string, int> GenerateSchema_GeneratesReferencedEnumSchema_IfEnumType_Data => new()
    {
        { typeof(IntEnum), JsonSchemaTypes.Integer, "int32", 3 },
        { typeof(LongEnum), JsonSchemaTypes.Integer, "int64", 3 },
        { typeof(IntEnum?), JsonSchemaTypes.Integer | JsonSchemaType.Null, "int32", 4 },
        { typeof(LongEnum?), JsonSchemaTypes.Integer | JsonSchemaType.Null, "int64", 4 },
    };

    [Theory]
    [MemberData(nameof(GenerateSchema_GeneratesReferencedEnumSchema_IfEnumType_Data))]
    public void GenerateSchema_GeneratesReferencedEnumSchema_IfEnumOrNullableEnumType(
        Type type,
        JsonSchemaType expectedSchemaType,
        string expectedFormat,
        int expectedEnumCount)
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(type, schemaRepository));

        Assert.NotNull(referenceSchema.Reference);
        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(expectedSchemaType, schema.Type);
        Assert.Equal(expectedFormat, schema.Format);
        Assert.NotNull(schema.Enum);
        Assert.Equal(expectedEnumCount, schema.Enum.Count);
    }

    [Fact]
    public void GenerateSchema_DedupsEnumValues_IfEnumTypeHasDuplicateValues()
    {
        var enumType = typeof(HttpStatusCode);
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(enumType, schemaRepository));

        Assert.NotNull(referenceSchema.Reference);
        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(enumType.GetEnumValues().Cast<HttpStatusCode>().Distinct().Count(), schema.Enum.Count);
    }

#nullable enable
    public static TheoryData<Type, JsonSchemaType?> GenerateSchema_GeneratesDictionarySchema_IfDictionaryType_Data => new()
    {
        { typeof(IDictionary<string, int>), JsonSchemaTypes.Integer },
        { typeof(IReadOnlyDictionary<string, bool>), JsonSchemaTypes.Boolean },
        { typeof(IDictionary), null },
        { typeof(ExpandoObject), null },
    };

    [Theory]
    [MemberData(nameof(GenerateSchema_GeneratesDictionarySchema_IfDictionaryType_Data))]
    public void GenerateSchema_GeneratesDictionarySchema_IfDictionaryType(
        Type type,
        JsonSchemaType? expectedAdditionalPropertiesType)
    {
        var schema = Subject().GenerateSchema(type, new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.True(schema.AdditionalPropertiesAllowed);
        Assert.NotNull(schema.AdditionalProperties);
        Assert.Equal(expectedAdditionalPropertiesType, schema.AdditionalProperties.Type);
    }
#nullable restore

    [Fact]
    public void GenerateSchema_GeneratesObjectSchema_IfDictionaryTypeHasEnumKey()
    {
        var schema = Subject().GenerateSchema(typeof(IDictionary<IntEnum, int>), new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal(["Value2", "Value4", "Value8"], schema.Properties.Keys);
    }

    [Fact]
    public void GenerateSchema_GeneratesReferencedDictionarySchema_IfDictionaryTypeIsSelfReferencing()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(DictionaryOfSelf), schemaRepository));

        Assert.NotNull(referenceSchema.Reference);
        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.True(schema.AdditionalPropertiesAllowed);
        Assert.NotNull(schema.AdditionalProperties);
        Assert.Equal(Assert.IsType<OpenApiSchemaReference>(schema.AdditionalProperties).Reference.Id, referenceSchema.Reference.Id); // ref to self
    }

#nullable enable
    public static TheoryData<Type, JsonSchemaType?, string?> EnumerableTypesData => new()
    {
        { typeof(int[]), JsonSchemaTypes.Integer, "int32" },
        { typeof(int?[]), JsonSchemaTypes.Integer | JsonSchemaType.Null, "int32" },
        { typeof(double[]), JsonSchemaTypes.Number, "double" },
        { typeof(double?[]), JsonSchemaTypes.Number | JsonSchemaType.Null, "double" },
        { typeof(DateTime[]), JsonSchemaTypes.String, "date-time" },
        { typeof(DateTime?[]), JsonSchemaTypes.String | JsonSchemaType.Null, "date-time" },
        { typeof(IEnumerable<string>), JsonSchemaTypes.String, null },
        { typeof(int[][]), JsonSchemaTypes.Array, null },
        { typeof(IList), null, null },
    };

    [Theory]
    [MemberData(nameof(EnumerableTypesData))]
    public void GenerateSchema_GeneratesArraySchema_IfEnumerableType(
        Type type,
        JsonSchemaType? expectedItemsType,
        string? expectedItemsFormat)
    {
        var schema = Subject().GenerateSchema(type, new SchemaRepository());

        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
        Assert.NotNull(schema.Items);
        Assert.Equal(expectedItemsType, schema.Items.Type);
        Assert.Equal(expectedItemsFormat, schema.Items.Format);
    }
#nullable restore

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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(ListOfSelf), schemaRepository));

        Assert.NotNull(referenceSchema.Reference);
        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Array, schema.Type);
        Assert.Equal(Assert.IsType<OpenApiSchemaReference>(schema.Items).Reference.Id, referenceSchema.Reference.Id); // ref to self
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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(type, schemaRepository));

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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(SubType1), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal(["Property1", "BaseProperty"], schema.Properties.Keys);
    }

    [Fact]
    public void GenerateSchema_ExcludesIndexerProperties_IfComplexTypeIsIndexed()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(IndexedType), schemaRepository));

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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(declaringType, schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(expectedNullable, schema.Properties[propertyName].Type.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void GenerateSchema_DoesNotSetNullableFlag_IfReferencedEnum()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject().GenerateSchema(typeof(TypeWithNullableProperties), schemaRepository);

        var reference = Assert.IsType<OpenApiSchemaReference>(referenceSchema);
        var schema = schemaRepository.Schemas[reference.Reference.Id];

        const string propertyName = nameof(TypeWithNullableProperties.NullableIntEnumProperty);
        Assert.False(schema.Properties[propertyName].Type.HasValue);

        reference = Assert.IsType<OpenApiSchemaReference>(schema.Properties[propertyName]);
        Assert.Equal("IntEnumNullable", reference.Reference.Id);
    }

    [Fact]
    public void GenerateSchema_SetNullableFlag_IfInlineEnum()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Subject(o => o.UseInlineDefinitionsForEnums = true).GenerateSchema(typeof(TypeWithNullableProperties), schemaRepository);

        var reference = Assert.IsType<OpenApiSchemaReference>(referenceSchema);
        var schema = schemaRepository.Schemas[reference.Reference.Id];
        Assert.True(schema.Properties[nameof(TypeWithNullableProperties.NullableIntEnumProperty)].Type.Value.HasFlag(JsonSchemaType.Null));
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
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.NullableIntWithDefaultValue), "2147483647")]
    [InlineData(typeof(TypeWithDefaultAttributes), nameof(TypeWithDefaultAttributes.NullableIntWithDefaultNullValue), null)]
    public void GenerateSchema_SetsDefault_IfPropertyHasDefaultValueAttribute(
        Type declaringType,
        string propertyName,
        string expectedDefaultAsJson)
    {
        // Arrange
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var schemaRepository = new SchemaRepository();

        // Act
        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(declaringType, schemaRepository));

        // Assert
        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        var propertySchema = schema.Properties[propertyName];
        Assert.Equal(expectedDefaultAsJson, propertySchema.Default?.ToJson());
    }

    [Fact]
    public void GenerateSchema_SetsDeprecatedFlag_IfPropertyHasObsoleteAttribute()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(TypeWithObsoleteAttribute), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.True(schema.Properties["ObsoleteProperty"].Deprecated);
    }

    [Theory]
    [InlineData(typeof(TypeWithValidationAttributes))]
    [InlineData(typeof(TypeWithValidationAttributesViaMetadataType))]
    public void GenerateSchema_SetsValidationProperties_IfComplexTypeHasValidationAttributes(Type type)
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(type, schemaRepository));

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
        Assert.NotNull(schema.Properties["IntWithExclusiveRange"].ExclusiveMinimum);
        Assert.NotNull(schema.Properties["IntWithExclusiveRange"].ExclusiveMaximum);
        Assert.Equal("byte", schema.Properties["StringWithBase64"].Format);
        Assert.Equal(JsonSchemaTypes.String | JsonSchemaType.Null, schema.Properties["StringWithBase64"].Type);
#endif
        Assert.Null(schema.Properties["IntWithRange"].ExclusiveMinimum);
        Assert.Null(schema.Properties["IntWithRange"].ExclusiveMaximum);
        Assert.Equal("1", schema.Properties["IntWithRange"].Minimum);
        Assert.Equal("10", schema.Properties["IntWithRange"].Maximum);
        Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
        Assert.Equal(5, schema.Properties["StringWithStringLength"].MinLength);
        Assert.Equal(10, schema.Properties["StringWithStringLength"].MaxLength);
        Assert.Equal(1, schema.Properties["StringWithRequired"].MinLength);
        Assert.False(schema.Properties["StringWithRequired"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["StringWithRequiredAllowEmptyTrue"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.Null(schema.Properties["StringWithRequiredAllowEmptyTrue"].MinLength);
        Assert.Equal(["NullableIntEnumWithRequired", "StringWithRequired", "StringWithRequiredAllowEmptyTrue"], schema.Required);
        Assert.Equal("Description", schema.Properties[nameof(TypeWithValidationAttributes.StringWithDescription)].Description);
        Assert.True(schema.Properties[nameof(TypeWithValidationAttributes.StringWithReadOnly)].ReadOnly);

        var reference = Assert.IsType<OpenApiSchemaReference>(schema.Properties[nameof(TypeWithValidationAttributes.NullableIntEnumWithRequired)]);
        Assert.False(reference.Type.HasValue);
        Assert.Equal(nameof(IntEnum), reference.Reference.Id);
    }

    [Fact]
    public void GenerateSchema_SetsReadOnlyAndWriteOnlyFlags_IfPropertyIsRestricted()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(TypeWithRestrictedProperties), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.False(schema.Properties["ReadWriteProperty"].ReadOnly);
        Assert.False(schema.Properties["ReadWriteProperty"].WriteOnly);
        Assert.True(schema.Properties["ReadOnlyProperty"].ReadOnly);
        Assert.False(schema.Properties["ReadOnlyProperty"].WriteOnly);
        Assert.False(schema.Properties["WriteOnlyProperty"].ReadOnly);
        Assert.True(schema.Properties["WriteOnlyProperty"].WriteOnly);
    }

    [Fact]
    public void GenerateSchema_DoesNotSetReadOnlyFlag_IfPropertyIsReadOnlyButCanBeSetViaConstructor()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(TypeWithPropertiesSetViaConstructor), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.False(schema.Properties["Id"].ReadOnly);
        Assert.False(schema.Properties["Desc"].ReadOnly);
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
        Assert.Null(schema.Properties);
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

        if (schema is OpenApiSchemaReference reference)
        {
            Assert.Contains("X-foo", schemaRepository.Schemas[reference.Reference.Id].Extensions.Keys);
        }
        else
        {
            Assert.Contains("X-foo", schema.Extensions.Keys);
        }
    }

    [Fact]
    public void GenerateSchema_SupportsOption_IgnoreObsoleteProperties()
    {
        var subject = Subject(
            configureGenerator: c => c.IgnoreObsoleteProperties = true
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(TypeWithObsoleteAttribute), schemaRepository));

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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(ComplexType), schemaRepository));

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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(SubType1), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.NotNull(schema.AllOf);
        Assert.Equal(2, schema.AllOf.Count);
        var baseSchema = Assert.IsType<OpenApiSchemaReference>(schema.AllOf[0]);
        Assert.Equal("BaseType", baseSchema.Reference.Id);
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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(BaseType), schemaRepository));

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

        var schema = Assert.IsType<OpenApiSchema>(subject.GenerateSchema(typeof(BaseType), schemaRepository));

        // The polymorphic schema
        Assert.NotNull(schema.OneOf);
        Assert.Equal(3, schema.OneOf.Count);
        // The base type schema
        var baseSchema = Assert.IsType<OpenApiSchemaReference>(schema.OneOf[0]);
        Assert.Equal("BaseType", baseSchema.Reference.Id);
        // The first sub type schema
        baseSchema = Assert.IsType<OpenApiSchemaReference>(schema.OneOf[1]);
        Assert.Equal("SubType1", baseSchema.Reference.Id);
        // The second sub type schema
        baseSchema = Assert.IsType<OpenApiSchemaReference>(schema.OneOf[2]);
        Assert.Equal("SubType2", baseSchema.Reference.Id);
    }

    [Fact]
    public void GenerateSchema_SupportsOption_UseAllOfToExtendReferenceSchemas()
    {
        var subject = Subject(
            configureGenerator: c => c.UseAllOfToExtendReferenceSchemas = true
        );
        var propertyInfo = typeof(SelfReferencingType).GetProperty(nameof(SelfReferencingType.Another));

        var schema = subject.GenerateSchema(propertyInfo.PropertyType, new SchemaRepository(), memberInfo: propertyInfo);

        Assert.IsNotType<OpenApiSchemaReference>(schema);
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
    public void GenerateSchema_HandlesTypesWithNestedTypes()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(ContainingType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal("NestedType", Assert.IsType<OpenApiSchemaReference>(schema.Properties["Property1"]).Reference.Id);
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
    public void GenerateSchema_HandlesTypesWithOverriddenProperties()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(TypeWithOverriddenProperty), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.Object, schema.Type);
        Assert.Equal(JsonSchemaTypes.String | JsonSchemaType.Null, schema.Properties["Property1"].Type);
    }

    [Fact]
    public void GenerateSchema_HandlesRecursion_IfCalledAgainWithinAFilter()
    {
        var subject = Subject(
            configureGenerator: c => c.SchemaFilters.Add(new RecursiveCallSchemaFilter())
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(ComplexType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal("ComplexType", Assert.IsType<OpenApiSchemaReference>(schema.Properties["Self"]).Reference.Id);
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

    [Theory]
    [InlineData(false, new[] { "\"Value2\"", "\"Value4\"", "\"Value8\"" }, "\"Value4\"")]
    [InlineData(true, new[] { "\"value2\"", "\"value4\"", "\"value8\"" }, "\"value4\"")]
    public void GenerateSchema_HonorsSerializerSetting_StringEnumConverter(
        bool camelCaseText,
        string[] expectedEnumAsJson,
        string expectedDefaultAsJson)
    {
        var subject = Subject(
            configureGenerator: c => { c.UseInlineDefinitionsForEnums = true; },
            configureSerializer: c =>
            {
                var stringEnumConverter = (camelCaseText) ? new StringEnumConverter(new CamelCaseNamingStrategy(), false) : new StringEnumConverter();
                c.Converters.Add(stringEnumConverter);
            }
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(TypeWithDefaultAttributeOnEnum), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        var propertySchema = schema.Properties[nameof(TypeWithDefaultAttributeOnEnum.EnumWithDefault)];
        Assert.Equal(JsonSchemaTypes.String, propertySchema.Type);
        Assert.Equal(expectedEnumAsJson, propertySchema.Enum.Select(openApiAny => openApiAny.ToJson()));
        Assert.Equal(expectedDefaultAsJson, propertySchema.Default.ToJson());
    }

    [Theory]
    [InlineData(TypeNameHandling.None, TypeNameAssemblyFormatHandling.Full, false,
        null)]
    [InlineData(TypeNameHandling.Arrays, TypeNameAssemblyFormatHandling.Full, false,
        null)]
    [InlineData(TypeNameHandling.Objects, TypeNameAssemblyFormatHandling.Full, true,
        "Swashbuckle.AspNetCore.TestSupport.{0}, Swashbuckle.AspNetCore.TestSupport, Version=1.0.0.0, Culture=neutral, PublicKeyToken=62657d7474907593")]
    [InlineData(TypeNameHandling.All, TypeNameAssemblyFormatHandling.Full, true,
        "Swashbuckle.AspNetCore.TestSupport.{0}, Swashbuckle.AspNetCore.TestSupport, Version=1.0.0.0, Culture=neutral, PublicKeyToken=62657d7474907593")]
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
                c.UseAllOfForInheritance = true;
            },
            configureSerializer: c =>
            {
                c.TypeNameHandling = typeNameHandling;
                c.TypeNameAssemblyFormatHandling = typeNameAssemblyFormatHandling;
            }
        );
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(BaseType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        if (expectedDiscriminatorPresent)
        {
            Assert.Contains("$type", schema.Properties.Keys);
            Assert.Contains("$type", schema.Required);
            Assert.NotNull(schema.Discriminator);
            Assert.Equal("$type", schema.Discriminator.PropertyName);
            Assert.Equal(
                expected: new Dictionary<string, string>
                {
                    [string.Format(expectedDiscriminatorMappingKeyFormat, "BaseType")] = "BaseType",
                    [string.Format(expectedDiscriminatorMappingKeyFormat, "SubType1")] = "SubType1",
                    [string.Format(expectedDiscriminatorMappingKeyFormat, "SubType2")] = "SubType2",
                },
                actual: schema.Discriminator.Mapping.ToDictionary((k) => k.Key, (v) => v.Value.Reference.Id));
        }
        else
        {
            Assert.DoesNotContain("$type", schema.Properties.Keys);
            Assert.DoesNotContain("$type", schema.Required);
            Assert.Null(schema.Discriminator);
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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(typeof(BaseType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        Assert.True(schema.Properties.ContainsKey(AdditionalPropertyJsonContractResolver.AdditionalPropertyName));
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_StringEnumConverter()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(JsonConverterAnnotatedEnum), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(JsonSchemaTypes.String, schema.Type);
        Assert.Equal(["\"Value1\"", "\"Value2\"", "\"X-foo\""], schema.Enum.Select(openApiAny => openApiAny.ToJson()));
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_JsonIgnore()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(JsonIgnoreAnnotatedType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(["StringWithNoAnnotation"], schema.Properties.Keys);
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_JsonProperty()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(JsonPropertyAnnotatedType), schemaRepository));

        var reference = Assert.IsType<OpenApiSchemaReference>(referenceSchema);
        var schema = schemaRepository.Schemas[reference.Reference.Id];

        Assert.Equal(
            [
                "string-with-json-property-name",
                "IntWithRequiredDefault",
                "StringWithRequiredDefault",
                "StringWithRequiredDisallowNull",
                "StringWithRequiredAlways",
                "StringWithRequiredAllowNull",
                "StringWithRequiredAlwaysButConflictingDataMember",
                "StringWithRequiredDefaultButConflictingDataMember",
                "IntEnumWithRequiredDefault",
                "IntEnumWithRequiredDisallowNull",
                "IntEnumWithRequiredAlways",
                "IntEnumWithRequiredAllowNull"
            ],
            schema.Properties.Keys
        );
        Assert.Equal(
            [
                "IntEnumWithRequiredAllowNull",
                "IntEnumWithRequiredAlways",
                "StringWithRequiredAllowNull",
                "StringWithRequiredAlways",
                "StringWithRequiredAlwaysButConflictingDataMember"
            ],
            schema.Required
        );

        Assert.True(schema.Properties["string-with-json-property-name"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["IntWithRequiredDefault"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Properties["StringWithRequiredDefault"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["StringWithRequiredDisallowNull"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["StringWithRequiredAlways"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Properties["StringWithRequiredAllowNull"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["StringWithRequiredAlwaysButConflictingDataMember"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Properties["StringWithRequiredDefaultButConflictingDataMember"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["IntEnumWithRequiredDefault"].Type.HasValue);
        Assert.False(schema.Properties["IntEnumWithRequiredAllowNull"].Type.HasValue);
        Assert.False(schema.Properties["IntEnumWithRequiredAlways"].Type.HasValue);
        Assert.False(schema.Properties["IntEnumWithRequiredDisallowNull"].Type.HasValue);
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_JsonRequired()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(JsonRequiredAnnotatedType), schemaRepository));

        var reference = Assert.IsType<OpenApiSchemaReference>(referenceSchema);
        var schema = schemaRepository.Schemas[reference.Reference.Id];

        Assert.Equal(["IntEnumWithRequired", "NullableIntEnumWithRequired", "StringWithConflictingRequired", "StringWithJsonRequired"], schema.Required);
        Assert.False(schema.Properties["StringWithJsonRequired"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["IntEnumWithRequired"].Type.HasValue);
        reference = Assert.IsType<OpenApiSchemaReference>(schema.Properties["IntEnumWithRequired"]);
        Assert.Equal(nameof(IntEnum), reference.Reference.Id);
        Assert.True(schemaRepository.TryLookupByType(typeof(IntEnum), out _));
        Assert.False(schemaRepository.TryLookupByType(typeof(IntEnum?), out _));
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_JsonObject()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(JsonObjectAnnotatedType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.Equal(
            [
                "StringWithDataMemberRequiredFalse",
                "StringWithNoAnnotation",
                "StringWithRequiredAllowNull",
                "StringWithRequiredUnspecified"
            ],
            schema.Required
        );
        Assert.False(schema.Properties["StringWithNoAnnotation"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["StringWithRequiredUnspecified"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Properties["StringWithRequiredAllowNull"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.False(schema.Properties["StringWithDataMemberRequiredFalse"].Type.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void GenerateSchema_HonorsSerializerAttribute_JsonExtensionData()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(JsonExtensionDataAnnotatedType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];
        Assert.True(schema.AdditionalPropertiesAllowed);
        Assert.NotNull(schema.AdditionalProperties);
        Assert.Null(schema.AdditionalProperties.Type);
    }

    [Fact]
    public void GenerateSchema_HonorsAttribute_SwaggerIgnore()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(SwaggerIngoreAnnotatedType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        Assert.True(schema.Properties.ContainsKey(nameof(SwaggerIngoreAnnotatedType.NotIgnoredString)));
        Assert.False(schema.Properties.ContainsKey(nameof(SwaggerIngoreAnnotatedType.IgnoredString)));
        Assert.False(schema.Properties.ContainsKey(nameof(SwaggerIngoreAnnotatedType.IgnoredExtensionData)));
        Assert.False(schema.AdditionalPropertiesAllowed);
        Assert.Null(schema.AdditionalProperties);
    }

    [Fact]
    public void GenerateSchema_HonorsDataMemberAttribute()
    {
        var schemaRepository = new SchemaRepository();

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(Subject().GenerateSchema(typeof(DataMemberAnnotatedType), schemaRepository));

        var schema = schemaRepository.Schemas[referenceSchema.Reference.Id];

        Assert.True(schema.Properties["StringWithDataMemberRequired"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Properties["StringWithDataMemberNonRequired"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Properties["RequiredWithCustomNameFromDataMember"].Type.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Properties["NonRequiredWithCustomNameFromDataMember"].Type.Value.HasFlag(JsonSchemaType.Null));

        Assert.Equal(
            [

                "StringWithDataMemberRequired",
                "StringWithDataMemberNonRequired",
                "RequiredWithCustomNameFromDataMember",
                "NonRequiredWithCustomNameFromDataMember"
            ],
            schema.Properties.Keys
        );

        Assert.Equal(
            [
                "RequiredWithCustomNameFromDataMember",
                "StringWithDataMemberRequired"
            ],
            schema.Required
        );
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

        var referenceSchema = Assert.IsType<OpenApiSchemaReference>(subject.GenerateSchema(type, schemaRepository));

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
        var schema = Assert.IsType<OpenApiSchema>(Subject().GenerateSchema(type, new SchemaRepository()));

        Assert.Null(schema.Type);
    }

    private static SchemaGenerator Subject(
        Action<SchemaGeneratorOptions> configureGenerator = null,
        Action<JsonSerializerSettings> configureSerializer = null)
    {
        var generatorOptions = new SchemaGeneratorOptions();
        configureGenerator?.Invoke(generatorOptions);

        var serializerSettings = new JsonSerializerSettings();
        configureSerializer?.Invoke(serializerSettings);

        return new SchemaGenerator(generatorOptions, new NewtonsoftDataContractResolver(serializerSettings));
    }
}
