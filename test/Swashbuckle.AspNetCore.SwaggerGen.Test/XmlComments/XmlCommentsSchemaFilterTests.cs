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
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "summary for StringProperty")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringField), "summary for StringField")]
        [InlineData(typeof(XmlAnnotatedSubType), nameof(XmlAnnotatedType.StringProperty), "summary for StringProperty")]
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
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolProperty), "boolean", null, true)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntProperty), "integer", "int32", 10)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.LongProperty), "integer", "int64", 4294967295L)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.FloatProperty), "number", "float", 1.2F)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DoubleProperty), "number", "double", 1.25D)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.EnumProperty), "integer", "int32", 2)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidProperty), "string", "uuid", "d3966535-2637-48fa-b911-e3c27405ee09")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "string", null, "example for StringProperty")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BadExampleIntProperty), "integer", "int32", null)]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringField), "string", null, "example for StringField")]
        [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolField), "boolean", null, true)]
        public void Apply_SetsPropertyExample_FromPropertyExampleTags(
            Type memberType,
            string memberName,
            string schemaType,
            string schemaFormat,
            object expectedValue)
        {
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>()
                {
                    { memberName, new OpenApiSchema { Type = schemaType, Format = schemaFormat } }
                }
            };
            var filterContext = FilterContextFor(memberType);

            Subject().Apply(schema, filterContext);

            var openApiPrimitive = (IOpenApiPrimitive)schema.Properties[memberName].Example;

            if (expectedValue != null)
                Assert.Equal(expectedValue, openApiPrimitive.GetType().GetProperty("Value").GetValue(openApiPrimitive));
            else
                Assert.Null(openApiPrimitive);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "MissingStringProperty", "string")]
        [InlineData(typeof(XmlAnnotatedType), "MissingIntegerProperty", "integer")]
        public void Apply_IgnoresNonexistingProperty(Type type,
            string propertyName,
            string propertyType)
        {
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>()
                {
                    { propertyName, new OpenApiSchema() { Type = propertyType } }
                }
            };
            var filterContext = FilterContextFor(type);

            Subject().Apply(schema, filterContext);

            var openApiSchema = schema.Properties[propertyName];
            Assert.Equal(propertyType, openApiSchema.Type);
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