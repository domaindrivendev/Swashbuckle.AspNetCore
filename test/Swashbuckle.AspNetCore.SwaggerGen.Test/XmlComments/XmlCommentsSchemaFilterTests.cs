using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using System.IO;
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

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "Property", PrimitiveType.String, "property example")]
        [InlineData(typeof(XmlAnnotatedSubType), "Property", PrimitiveType.String, "property example")]
        [InlineData(typeof(XmlAnnotatedType), "Field", PrimitiveType.String, "field example")]
        [InlineData(typeof(XmlAnnotatedType), "IntProperty", PrimitiveType.Integer, "10")]
        [InlineData(typeof(XmlAnnotatedType), "LongProperty", PrimitiveType.Long, "4294967295")]
        [InlineData(typeof(XmlAnnotatedType), "DoubleProperty", PrimitiveType.Double, "1.25")]
        [InlineData(typeof(XmlAnnotatedType), "FloatProperty", PrimitiveType.Float, "1.2")]
        [InlineData(typeof(XmlAnnotatedType), "ByteProperty", PrimitiveType.Byte, "16")]
        [InlineData(typeof(XmlAnnotatedType), "BoolField", PrimitiveType.Boolean, "True")]
        [InlineData(typeof(XmlAnnotatedType), "GuidProperty", PrimitiveType.String, "d3966535-2637-48fa-b911-e3c27405ee09")]
        [InlineData(typeof(XmlAnnotatedType), "BadExampleIntProperty", PrimitiveType.String, "property bad example")]
        public void Apply_SetsPropertyExample_FromPropertyExampleTags(
            Type type,
            string propertyName,
            PrimitiveType expectedPrimitiveType,
            object expectedValueString)
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

            var openApiPrimitive = (IOpenApiPrimitive)schema.Properties[propertyName].Example;
            Assert.Equal(expectedPrimitiveType, openApiPrimitive.PrimitiveType);
            Assert.Equal(expectedValueString, GetOpenApiPrimitiveValue(openApiPrimitive).ToString());
        }

        private static object GetOpenApiPrimitiveValue(IOpenApiPrimitive primitive)
        {
            return primitive.GetType().GetProperty("Value").GetValue(primitive);
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            var jsonObjectContract = new DefaultContractResolver().ResolveContract(type);
            return new SchemaFilterContext(type, (jsonObjectContract as JsonObjectContract), new SchemaRepository(), null);
            throw new NotImplementedException();
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