using System.Globalization;
using System.Xml.XPath;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class XmlCommentsSchemaFilterTests
{
    [Theory]
    [InlineData(typeof(XmlAnnotatedType), "Summary for XmlAnnotatedType")]
    [InlineData(typeof(XmlAnnotatedType.NestedType), "Summary for NestedType")]
    [InlineData(typeof(XmlAnnotatedGenericType<int, string>), "Summary for XmlAnnotatedGenericType")]
    public void Apply_SetsDescription_FromTypeSummaryTag(
        Type type,
        string expectedDescription)
    {
        var schema = new OpenApiSchema { };
        var filterContext = new SchemaFilterContext(type, null, null);

        Subject().Apply(schema, filterContext);

        Assert.Equal(expectedDescription, schema.Description);
    }

    [Fact]
    public void Apply_SetsDescription_FromFieldSummaryTag()
    {
        var fieldInfo = typeof(XmlAnnotatedType).GetField(nameof(XmlAnnotatedType.BoolField));
        var schema = new OpenApiSchema { };
        var filterContext = new SchemaFilterContext(fieldInfo.FieldType, null, null, memberInfo: fieldInfo);

        Subject().Apply(schema, filterContext);

        Assert.Equal("Summary for BoolField", schema.Description);
    }

    [Theory]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "Summary for StringProperty")]
    [InlineData(typeof(XmlAnnotatedSubType), nameof(XmlAnnotatedType.StringProperty), "Summary for StringProperty")]
    [InlineData(typeof(XmlAnnotatedGenericType<string, bool>), "GenericProperty", "Summary for GenericProperty")]
    public void Apply_SetsDescription_FromPropertySummaryTag(
        Type declaringType,
        string propertyName,
        string expectedDescription)
    {
        var propertyInfo = declaringType.GetProperty(propertyName);
        var schema = new OpenApiSchema();
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        Subject().Apply(schema, filterContext);

        Assert.Equal(expectedDescription, schema.Description);
    }

    public static TheoryData<Type, string, JsonSchemaType, string, string> Apply_SetsExample_FromPropertyExampleTag_Data() => new()
    {
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolProperty), JsonSchemaTypes.Boolean, "true",null },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntProperty), JsonSchemaTypes.Integer, "10","int32" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.LongProperty), JsonSchemaTypes.Integer, "4294967295","int64" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.FloatProperty), JsonSchemaTypes.Number, "1.2", "float" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DoubleProperty), JsonSchemaTypes.Number, "1.25","double" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DateTimeProperty), JsonSchemaTypes.String, "\"6/22/2022 12:00:00 AM\"","date-time" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.EnumProperty), JsonSchemaTypes.Integer, "2","int32" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidProperty), JsonSchemaTypes.String, "\"d3966535-2637-48fa-b911-e3c27405ee09\"","uuid" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), JsonSchemaTypes.String, "\"Example for StringProperty\"",null },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.ObjectProperty), JsonSchemaTypes.Object, "{\n  \"prop1\": 1,\n  \"prop2\": \"foobar\"\n}", null },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.BoolProperty), JsonSchemaTypes.Boolean, "true",null },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.IntProperty), JsonSchemaTypes.Integer, "10" , "int32"},
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.LongProperty), JsonSchemaTypes.Integer, "4294967295","int64" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.FloatProperty), JsonSchemaTypes.Number, "1.2", "float" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DoubleProperty), JsonSchemaTypes.Number, "1.25", "double" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DateTimeProperty), JsonSchemaTypes.String, "\"6/22/2022 12:00:00 AM\"", "date-time" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.EnumProperty), JsonSchemaTypes.Integer, "2" , "int32"},
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.GuidProperty), JsonSchemaTypes.String, "\"d3966535-2637-48fa-b911-e3c27405ee09\"" , "uuid"},
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringProperty), JsonSchemaTypes.String, "\"Example for StringProperty\"", null },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.ObjectProperty), JsonSchemaTypes.Object, "{\n  \"prop1\": 1,\n  \"prop2\": \"foobar\"\n}", null },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri), JsonSchemaTypes.String, "\"https://test.com/a?b=1\\u0026c=2\"", null },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithUri), JsonSchemaTypes.String, "\"https://test.com/a?b=1\\u0026c=2\"", null },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithNullExample), JsonSchemaTypes.String, null, null },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithNullExample), JsonSchemaTypes.String, null, null },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableStringPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null",null },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableStringPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null",null },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableIntPropertyWithNotNullExample), JsonSchemaTypes.Integer | JsonSchemaType.Null, "3", "int32" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableIntPropertyWithNotNullExample), JsonSchemaTypes.Integer | JsonSchemaType.Null, "3", "int32" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableIntPropertyWithNullExample), JsonSchemaTypes.Integer | JsonSchemaType.Null, "null" , "int32"},
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableIntPropertyWithNullExample), JsonSchemaTypes.Integer | JsonSchemaType.Null, "null", "int32" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntPropertyWithNullExample), JsonSchemaTypes.Integer, null , "int32"},
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.IntPropertyWithNullExample), JsonSchemaTypes.Integer, null, "int32" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableGuidPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null", "uuid" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableGuidPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null", "uuid" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidPropertyWithNullExample), JsonSchemaTypes.String, null,"uuid"  },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.GuidPropertyWithNullExample), JsonSchemaTypes.String, null,"uuid"  },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.ObjectPropertyNullExample), JsonSchemaTypes.String, null,null },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.ObjectPropertyNullExample), JsonSchemaTypes.String, null,null },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableObjectPropertyNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null",null},
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableObjectPropertyNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null",null},
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableDateTimePropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null", "date-time" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableDateTimePropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null","date-time" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DateTimePropertyWithNullExample), JsonSchemaTypes.String, null, "date-time" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DateTimePropertyWithNullExample), JsonSchemaTypes.String, null, "date-time" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableTimeOnlyPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null","time"  },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableTimeOnlyPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null","time"  },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedRecord.TimeOnlyPropertyWithNullExample), JsonSchemaTypes.String, null,"time"  },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.TimeOnlyPropertyWithNullExample), JsonSchemaTypes.String, null,"time"  },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableTimeSpanPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null", "date-span" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableTimeSpanPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null", "date-span" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.TimeSpanPropertyWithNullExample), JsonSchemaTypes.String, null, "date-span" },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.TimeSpanPropertyWithNullExample), JsonSchemaTypes.String, null, "date-span" },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.NullableDateOnlyPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null","date"  },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.NullableDateOnlyPropertyWithNullExample), JsonSchemaTypes.String | JsonSchemaType.Null, "null","date"  },
        { typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DateOnlyPropertyWithNullExample), JsonSchemaTypes.String, null,"date"  },
        { typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DateOnlyPropertyWithNullExample), JsonSchemaTypes.String, null,"date"  }
    };

    [Theory]
    [MemberData(nameof(Apply_SetsExample_FromPropertyExampleTag_Data))]
    public void Apply_SetsExample_FromPropertyExampleTag(
        Type declaringType,
        string propertyName,
        JsonSchemaType schemaType,
        string expectedExampleAsJson,
        string format)
    {
        // Arrange
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var propertyInfo = declaringType.GetProperty(propertyName);
        var schema = new OpenApiSchema { Type = schemaType, Format = format };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        // Act
        Subject().Apply(schema, filterContext);

        // Assert
        Assert.Equal(expectedExampleAsJson, schema.Example?.ToJson());
    }

    public static TheoryData<Type, string, JsonSchemaType> Apply_DoesNotSetExample_WhenPropertyExampleTagIsNotProvided_Data => new()
    {
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.BoolProperty), JsonSchemaTypes.Boolean },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.IntProperty), JsonSchemaTypes.Integer },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.LongProperty), JsonSchemaTypes.Integer },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.FloatProperty), JsonSchemaTypes.Number },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DoubleProperty), JsonSchemaTypes.Number },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DateTimeProperty), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.EnumProperty), JsonSchemaTypes.Integer },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.GuidProperty), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringProperty), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.ObjectProperty), JsonSchemaTypes.Object },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithNullExample), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithUri), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.BoolProperty), JsonSchemaTypes.Boolean },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.IntProperty), JsonSchemaTypes.Integer },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.LongProperty), JsonSchemaTypes.Integer },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.FloatProperty), JsonSchemaTypes.Number },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DoubleProperty), JsonSchemaTypes.Number },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DateTimeProperty), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.EnumProperty), JsonSchemaTypes.Integer },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.GuidProperty), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringProperty), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.ObjectProperty), JsonSchemaTypes.Object },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithNullExample), JsonSchemaTypes.String },
        { typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithUri), JsonSchemaTypes.String },
    };

    [Theory]
    [MemberData(nameof(Apply_DoesNotSetExample_WhenPropertyExampleTagIsNotProvided_Data))]
    public void Apply_DoesNotSetExample_WhenPropertyExampleTagIsNotProvided(
        Type declaringType,
        string propertyName,
        JsonSchemaType schemaType)
    {
        // Arrange
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var propertyInfo = declaringType.GetProperty(propertyName);
        var schema = new OpenApiSchema { Type = schemaType };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        // Act
        Subject().Apply(schema, filterContext);

        // Assert
        Assert.Null(schema.Example);
    }

    [Theory]
    [InlineData("en-US", 1.2F)]
    [InlineData("sv-SE", 1.2F)]
    public void Apply_UsesInvariantCulture_WhenSettingExample(
        string cultureName,
        float expectedValue)
    {
        var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.FloatProperty));
        var schema = new OpenApiSchema { Type = JsonSchemaTypes.Number, Format = "float" };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        var defaultCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

        Subject().Apply(schema, filterContext);

        CultureInfo.CurrentCulture = defaultCulture;

        Assert.NotNull(schema.Example);
        Assert.Equal(expectedValue, schema.Example.GetValue<float>());
    }

    private static XmlCommentsSchemaFilter Subject()
    {
        using var xml = File.OpenText(typeof(FakeControllerWithXmlComments).Assembly.GetName().Name + ".xml");
        var document = new XPathDocument(xml);
        var members = XmlCommentsDocumentHelper.CreateMemberDictionary(document);
        return new(members, new());
    }
}
