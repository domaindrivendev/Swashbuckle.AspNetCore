using System;
using System.Globalization;
using System.IO;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.TestSupport;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
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

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolProperty), JsonSchemaType.Boolean, "true")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntProperty), JsonSchemaType.Integer, "10")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.LongProperty), JsonSchemaType.Integer, "4294967295")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.FloatProperty), JsonSchemaType.Number, "1.2")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DoubleProperty), JsonSchemaType.Number, "1.25")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DateTimeProperty), JsonSchemaType.String, "\"6/22/2022 12:00:00 AM\"")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.EnumProperty), JsonSchemaType.Integer, "2")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidProperty), JsonSchemaType.String, "\"d3966535-2637-48fa-b911-e3c27405ee09\"")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), JsonSchemaType.String, "\"Example for StringProperty\"")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.ObjectProperty), JsonSchemaType.Object, "{\n  \"prop1\": 1,\n  \"prop2\": \"foobar\"\n}")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithNullExample), JsonSchemaType.String, "null")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri), JsonSchemaType.String, "\"https://test.com/a?b=1&c=2\"")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.BoolProperty), JsonSchemaType.Boolean, "true")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.IntProperty), JsonSchemaType.Integer, "10")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.LongProperty), JsonSchemaType.Integer, "4294967295")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.FloatProperty), JsonSchemaType.Number, "1.2")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DoubleProperty), JsonSchemaType.Number, "1.25")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DateTimeProperty), JsonSchemaType.String, "\"6/22/2022 12:00:00 AM\"")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.EnumProperty), JsonSchemaType.Integer, "2")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.GuidProperty), JsonSchemaType.String, "\"d3966535-2637-48fa-b911-e3c27405ee09\"")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringProperty), JsonSchemaType.String, "\"Example for StringProperty\"")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.ObjectProperty), JsonSchemaType.Object, "{\n  \"prop1\": 1,\n  \"prop2\": \"foobar\"\n}")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithNullExample), JsonSchemaType.String, "null")]
        [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithUri), JsonSchemaType.String, "\"https://test.com/a?b=1&c=2\"")]
        [UseInvariantCulture]
        public void Apply_SetsExample_FromPropertyExampleTag(
            Type declaringType,
            string propertyName,
            JsonSchemaType schemaType,
            string expectedExampleAsJson)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);
            var schema = new OpenApiSchema { Type = schemaType };
            var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

            Subject().Apply(schema, filterContext);

            Assert.NotNull(schema.Example);
            Assert.Equal(expectedExampleAsJson, schema.Example.ToJsonString());
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.BoolProperty), JsonSchemaType.Boolean)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.IntProperty), JsonSchemaType.Integer)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.LongProperty), JsonSchemaType.Integer)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.FloatProperty), JsonSchemaType.Number)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DoubleProperty), JsonSchemaType.Number)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DateTimeProperty), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.EnumProperty), JsonSchemaType.Integer)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.GuidProperty), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringProperty), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.ObjectProperty), JsonSchemaType.Object)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithNullExample), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithUri), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.BoolProperty), JsonSchemaType.Boolean)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.IntProperty), JsonSchemaType.Integer)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.LongProperty), JsonSchemaType.Integer)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.FloatProperty), JsonSchemaType.Number)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DoubleProperty), JsonSchemaType.Number)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DateTimeProperty), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.EnumProperty), JsonSchemaType.Integer)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.GuidProperty), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringProperty), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.ObjectProperty), JsonSchemaType.Object)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithNullExample), JsonSchemaType.String)]
        [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithUri), JsonSchemaType.String)]
        [UseInvariantCulture]
        public void Apply_DoesNotSetExample_WhenPropertyExampleTagIsNotProvided(
            Type declaringType,
            string propertyName,
            JsonSchemaType schemaType)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);
            var schema = new OpenApiSchema { Type = schemaType };
            var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

            Subject().Apply(schema, filterContext);

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
            var schema = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "float" };
            var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

            var defaultCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            Subject().Apply(schema, filterContext);

            CultureInfo.CurrentCulture = defaultCulture;

            Assert.Equal(expectedValue, schema.Example.GetType().GetProperty("Value").GetValue(schema.Example));
        }

        private static XmlCommentsSchemaFilter Subject()
        {
            using var xmlComments = File.OpenText(typeof(XmlAnnotatedType).Assembly.GetName().Name + ".xml");
            return new XmlCommentsSchemaFilter(new XPathDocument(xmlComments));
        }
    }
}
