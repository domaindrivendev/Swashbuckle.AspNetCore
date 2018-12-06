using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
using Castle.Core;
using FluentAssertions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsSchemaFilterTests
    {
        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "summary for XmlAnnotatedType")]
        [InlineData(typeof(XmlAnnotatedType.NestedType), "summary for NestedType")]
        [InlineData(typeof(XmlAnnotatedGenericType<int, string>), "summary for XmlAnnotatedGenericType")]
        public void Apply_SetsDescription_FromSummaryTag(
            Type type,
            string expectedDescription)
        {
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>()
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Description);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "Property", "summary for Property")]
        [InlineData(typeof(XmlAnnotatedType), "Field", "summary for Field")]
        [InlineData(typeof(XmlAnnotatedSubType), "Property", "summary for Property")]
        [InlineData(typeof(XmlAnnotatedGenericType<string, bool>), "GenericProperty", "Summary for GenericProperty")]
        public void Apply_SetsPropertyDescriptions_FromPropertySummaryTags(
            Type type,
            string propertyName,
            string expectedDescription)
        {
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>()
                {
                    { propertyName, new OpenApiSchema() }
                }
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Properties[propertyName].Description);
        }

        public static IEnumerable<object[]> PropertyExampleTestData =>
            new List<object[]>
            {
                new object[] {typeof(XmlAnnotatedType), "Property", new OpenApiString("property example")},
                new object[] {typeof(XmlAnnotatedSubType), "Property", new OpenApiString("property example")},
                new object[] {typeof(XmlAnnotatedType), "Field", new OpenApiString("field example")},
                new object[] {typeof(XmlAnnotatedType), "IntProperty", new OpenApiInteger(10)},
                new object[] {typeof(XmlAnnotatedType), "LongProperty", new OpenApiLong(4294967295)},
                new object[] {typeof(XmlAnnotatedType), "DoubleProperty", new OpenApiDouble(1.25)},
                new object[] {typeof(XmlAnnotatedType), "FloatProperty", new OpenApiDouble(1.2)},
                new object[] {typeof(XmlAnnotatedType), "ByteProperty", new OpenApiByte(0x10)},
                new object[] {typeof(XmlAnnotatedType), "DateTimeProperty", new OpenApiDate(new DateTime(2016, 11, 15))},
                new object[] {typeof(XmlAnnotatedType), "BoolField", new OpenApiBoolean(true)},
                new object[] {typeof(XmlAnnotatedType), "GuidProperty", new OpenApiString("d3966535-2637-48fa-b911-e3c27405ee09")},
                new object[] {typeof(XmlAnnotatedType), "BadExampleProperty", new OpenApiString("property bad example")},
            };

        [Theory]
        [MemberData(nameof(PropertyExampleTestData))]
        public void Apply_SetsPropertyExample_FromPropertyExampleTags(
            Type type,
            string propertyName,
            object expectedExample)
        {
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>()
                {
                    { propertyName, new OpenApiSchema() }
                }
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            schema.Properties[propertyName].Example.Should().BeOfType(expectedExample.GetType());
            schema.Properties[propertyName].Example.Should().BeEquivalentTo(expectedExample);
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            var jsonObjectContract = new DefaultContractResolver().ResolveContract(type);
            return new SchemaFilterContext(type, (jsonObjectContract as JsonObjectContract), null);
        }

        private XmlCommentsSchemaFilter Subject()
        {
            using (var xmlComments = File.OpenText(GetType().GetTypeInfo()
                    .Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsSchemaFilter(new XPathDocument(xmlComments));
            }
        }
    }
}