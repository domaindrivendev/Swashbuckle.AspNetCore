using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsSchemaFilterTests
    {
        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "summary for XmlAnnotatedType")]
        [InlineData(typeof(XmlAnnotatedWithNestedType.NestedType), "summary for NestedType")]
        [InlineData(typeof(XmlAnnotatedGenericType<string>), "summary for XmlAnnotatedGenericType")]
        public void Apply_SetsDescription_FromClassSummaryTag(
            Type type,
            string expectedDescription)
        {
            var schema = new Schema
            {
                Properties = new Dictionary<string, Schema>()
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Description);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "Property", "summary for Property")]
        [InlineData(typeof(XmlAnnotatedSubType), "BaseProperty", "summary for BaseProperty")]
        [InlineData(typeof(XmlAnnotatedGenericType<string>), "GenericProperty", "summary for GenericProperty")]
        public void Apply_SetsPropertyDescriptions_FromPropertySummaryTag(
            Type type,
            string propertyName,
            string expectedDescription)
        {
            var schema = new Schema
            {
                Properties = new Dictionary<string, Schema>()
                {
                    { propertyName, new Schema() }
                }
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            Assert.Equal(expectedDescription, schema.Properties[propertyName].Description);
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            var jsonObjectContract = new DefaultContractResolver().ResolveContract(type);
            return new SchemaFilterContext(type, (jsonObjectContract as JsonObjectContract), null);
        }

        private XmlCommentsSchemaFilter Subject()
        {
            var xmlComments = GetType().GetTypeInfo()
                .Assembly
                .GetManifestResourceStream("Swashbuckle.AspNetCore.SwaggerGen.Test.TestFixtures.XmlComments.xml");

            return new XmlCommentsSchemaFilter(new XPathDocument(xmlComments));
        }
    }
}