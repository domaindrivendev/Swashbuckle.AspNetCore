using System;
using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Fixtures;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlTypeCommentsTests
    {
        [Fact]
        public void Apply_SetsDescription_FromClassSummaryTag()
        {
            var schema = new Schema
            {
                properties = new Dictionary<string, Schema>()
            };
            var filterContext = FilterContextFor<XmlAnnotatedType>();

            Subject().Apply(schema, filterContext);

            Assert.Equal("summary for XmlAnnotatedType", schema.description);
        }

        [Fact]
        public void Apply_SetsPropertyDescriptions_FromPropertySummaryTag()
        {
            var schema = new Schema
            {
                properties = new Dictionary<string, Schema>()
                {
                    { "Property", new Schema() },
                    { "NestedTypeProperty", new Schema() },
                    { "BaseProperty", new Schema() }
                }
            };
            var filterContext = FilterContextFor<XmlAnnotatedType>();

            Subject().Apply(schema, filterContext);

            Assert.Equal("summary for Property", schema.properties["Property"].description);
            Assert.Equal("summary for NestedTypeProperty", schema.properties["NestedTypeProperty"].description);
            Assert.Equal("summary for BaseProperty", schema.properties["BaseProperty"].description);
        }

        private ModelFilterContext FilterContextFor<T>()
        {
            var type = typeof(T);
            var jsonObjectContract = new DefaultContractResolver().ResolveContract(type);
            return new ModelFilterContext(type, (jsonObjectContract as JsonObjectContract), null);
        }

        private ApplyXmlTypeComments Subject()
        {
            var path = string.Format(@"{0}\Fixtures\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory);
            return new ApplyXmlTypeComments(path);
        }
    }
}