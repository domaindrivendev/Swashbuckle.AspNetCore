using System;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Xunit;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.TestFixtures;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class XmlCommentsModelFilterTests
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

        private ModelFilterContext FilterContextFor(Type type)
        {
            var jsonObjectContract = new DefaultContractResolver().ResolveContract(type);
            return new ModelFilterContext(type, (jsonObjectContract as JsonObjectContract), null);
        }

        private XmlCommentsModelFilter Subject()
        {
            var xmlComments = GetType().GetTypeInfo()
                .Assembly
                .GetManifestResourceStream("Swashbuckle.SwaggerGen.Test.TestFixtures.XmlComments.xml");

            return new XmlCommentsModelFilter(new XPathDocument(xmlComments));
        }
    }
}