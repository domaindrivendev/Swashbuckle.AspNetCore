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
            var schema = new OpenApiSchema { };
            var fieldInfo = typeof(XmlAnnotatedType).GetField(nameof(XmlAnnotatedType.BoolField));
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
            var schema = new OpenApiSchema();
            var propertyInfo = declaringType.GetProperty(propertyName);
            var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

            Subject().Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Description);
        }

        [Fact]
        public void Apply_SetsDescription_FromActionParamTag()
        {
            var schema = new OpenApiSchema();
            var parameterInfo = typeof(FakeControllerWithXmlComments)
                .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))
                .GetParameters()[0];
            var filterContext = new SchemaFilterContext(parameterInfo.ParameterType, null, null, parameterInfo: parameterInfo);

            Subject().Apply(schema, filterContext);

            Assert.Equal("Description for param1", schema.Description);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolProperty), "boolean", null, true)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntProperty), "integer", "int32", 10)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.LongProperty), "integer", "int64", 4294967295L)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.FloatProperty), "number", "float", 1.2F)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DoubleProperty), "number", "double", 1.25D)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.EnumProperty), "integer", "int32", 2)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidProperty), "string", "uuid", "d3966535-2637-48fa-b911-e3c27405ee09")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "string", null, "Example for StringProperty")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BadExampleIntProperty), "integer", "int32", null)]
        public void Apply_SetsExample_FromPropertyExampleTag(
            Type declaringType,
            string propertyName,
            string schemaType,
            string schemaFormat,
            object expectedValue)
        {
            var schema = new OpenApiSchema { Type = schemaType, Format = schemaFormat };
            var propertyInfo = declaringType.GetProperty(propertyName);
            var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

            Subject().Apply(schema, filterContext);

            if (expectedValue != null)
            {
                Assert.NotNull(schema.Example);
                Assert.Equal(expectedValue, schema.Example.GetType().GetProperty("Value").GetValue(schema.Example));
            }
            else
            {
                Assert.Null(schema.Example);
            }
        }

        [Theory]
        [InlineData("en-US", 1.2F)]
        [InlineData("sv-SE", 1.2F)]
        public void Apply_UsesInvariantCulture_WhenSettingExample(
            string cultureName,
            float expectedValue)
        {
            var schema = new OpenApiSchema { Type = "number", Format = "float" };
            var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.FloatProperty));
            var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

            var defaultCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            Subject().Apply(schema, filterContext);

            CultureInfo.CurrentCulture = defaultCulture;

            Assert.Equal(expectedValue, schema.Example.GetType().GetProperty("Value").GetValue(schema.Example));
        }

        private XmlCommentsSchemaFilter Subject()
        {
            using (var xmlComments = File.OpenText(typeof(XmlAnnotatedType).Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsSchemaFilter(new XPathDocument(xmlComments));
            }
        }
    }
}