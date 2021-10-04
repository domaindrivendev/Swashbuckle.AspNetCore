using System;
using System.Globalization;
using System.Xml.XPath;
using System.IO;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsSchemaFilterTests
    {
        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "Summary for XmlAnnotatedType", null)]
        [InlineData(typeof(XmlAnnotatedType.NestedType), "Summary for NestedType", null)]
        [InlineData(typeof(XmlAnnotatedGenericType<int, string>), "Summary for XmlAnnotatedGenericType", null)]
        [InlineData(typeof(XmlAnnotatedType), "Summary для XmlAnnotatedType", "ru-RU")]
        [InlineData(typeof(XmlAnnotatedType.NestedType), "Summary для NestedType", "ru-RU")]
        [InlineData(typeof(XmlAnnotatedGenericType<int, string>), "Summary для XmlAnnotatedGenericType", "ru-RU")]
        public void Apply_SetsDescription_FromTypeSummaryTag(
            Type type,
            string expectedDescription,
            string cultureName)
        {
            var schema = new OpenApiSchema { };
            var filterContext = new SchemaFilterContext(type, null, null);

            Subject(cultureName).Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Description);
        }

        [Theory]
        [InlineData("Summary for BoolField", null)]
        [InlineData("Summary for BoolField", "en-US")]
        [InlineData("Summary для BoolField", "ru-RU")]
        public void Apply_SetsDescription_FromFieldSummaryTag(
            string expectedDescription,
            string cultureName)
        {
            var fieldInfo = typeof(XmlAnnotatedType).GetField(nameof(XmlAnnotatedType.BoolField));
            var schema = new OpenApiSchema { };
            var filterContext = new SchemaFilterContext(fieldInfo?.FieldType, null, null, memberInfo: fieldInfo);

            Subject(cultureName).Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Description);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "Summary for StringProperty", null)]
        [InlineData(typeof(XmlAnnotatedSubType), nameof(XmlAnnotatedType.StringProperty), "Summary for StringProperty", null)]
        [InlineData(typeof(XmlAnnotatedGenericType<string, bool>), "GenericProperty", "Summary for GenericProperty", null)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "Summary для StringProperty", "ru-RU")]
        [InlineData(typeof(XmlAnnotatedSubType), nameof(XmlAnnotatedType.StringProperty), "Summary для StringProperty", "ru-RU")]
        [InlineData(typeof(XmlAnnotatedGenericType<string, bool>), "GenericProperty", "Summary для GenericProperty", "ru-RU")]
        public void Apply_SetsDescription_FromPropertySummaryTag(
            Type declaringType,
            string propertyName,
            string expectedDescription,
            string cultureName)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);
            var schema = new OpenApiSchema();
            var filterContext = new SchemaFilterContext(propertyInfo?.PropertyType, null, null, memberInfo: propertyInfo);

            Subject(cultureName).Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Description);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolProperty), "boolean", "true")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntProperty), "integer", "10")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.LongProperty), "integer", "4294967295")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.FloatProperty), "number", "1.2")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DoubleProperty), "number", "1.25")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.EnumProperty), "integer", "2")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidProperty), "string", "\"d3966535-2637-48fa-b911-e3c27405ee09\"")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "string", "\"Example for StringProperty\"")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.ObjectProperty), "object", "{\n  \"prop1\": 1,\n  \"prop2\": \"foobar\"\n}")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithNullExample), "string", "null")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri), "string", "\"https://test.com/a?b=1&c=2\"")]
        [UseInvariantCulture]
        public void Apply_SetsExample_FromPropertyExampleTag(
            Type declaringType,
            string propertyName,
            string schemaType,
            string expectedExampleAsJson)
        {
            var propertyInfo = declaringType.GetProperty(propertyName);
            var schema = new OpenApiSchema { Type = schemaType };
            var filterContext = new SchemaFilterContext(propertyInfo?.PropertyType, null, null, memberInfo: propertyInfo);

            Subject().Apply(schema, filterContext);

            Assert.NotNull(schema.Example);
            Assert.Equal(expectedExampleAsJson, schema.Example.ToJson());
        }

        [Theory]
        [InlineData("en-US", 1.2F)]
        [InlineData("sv-SE", 1.2F)]
        public void Apply_UsesInvariantCulture_WhenSettingExample(
            string cultureName,
            float expectedValue)
        {
            var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.FloatProperty));
            var schema = new OpenApiSchema { Type = "number", Format = "float" };
            var filterContext = new SchemaFilterContext(propertyInfo?.PropertyType, null, null, memberInfo: propertyInfo);

            var defaultCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            Subject().Apply(schema, filterContext);

            CultureInfo.CurrentCulture = defaultCulture;

            Assert.Equal(expectedValue, schema.Example.GetType().GetProperty("Value")?.GetValue(schema.Example));
        }

        private XmlCommentsSchemaFilter Subject(string cultureName = null)
        {
            using (var xmlComments = File.OpenText(typeof(XmlAnnotatedType).Assembly.GetName().Name + ".xml"))
            {
                var culture = cultureName == null ? null : new CultureInfo(cultureName);
                return new XmlCommentsSchemaFilter(new XPathDocument(xmlComments), culture);
            }
        }
    }
}