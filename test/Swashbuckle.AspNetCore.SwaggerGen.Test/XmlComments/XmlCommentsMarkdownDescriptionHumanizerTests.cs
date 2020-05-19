using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsMarkdownDescriptionHumanizerTests
    {
        [Theory]
        [MemberData(nameof(Fields))]
        public void Apply_SetsDescription_Markdown(string field)
        {
            var schema = new OpenApiSchema { };
            var fieldInfo = typeof(TypeToConvertRemarksToMarkdown).GetField(field);
            var filterContext = new SchemaFilterContext(fieldInfo.FieldType, null, null, memberInfo: fieldInfo);

            Subject().Apply(schema, filterContext);

            var expectedDescription = ((string) fieldInfo.GetValue(null)).Replace("\r", "");

            Assert.Equal(expectedDescription, schema.Description);
        }

        public static IEnumerable<object[]> Fields =>
            typeof(TypeToConvertRemarksToMarkdown).GetFields()
                .Select(x => new [] { x.Name });

        private XmlCommentsSchemaFilter Subject()
        {
            using (var xmlComments = new XmlTextReader(typeof(TypeToConvertRemarksToMarkdown).Assembly.GetName().Name + ".xml"))
            {
                return new XmlCommentsSchemaFilter(new XPathDocument(xmlComments, XmlSpace.Preserve), new XmlCommentsMarkdownDescriptionHumanizer());
            }
        }
    }
}