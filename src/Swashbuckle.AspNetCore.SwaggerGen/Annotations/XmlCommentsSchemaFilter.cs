using System.Xml.XPath;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(Schema schema, SchemaFilterContext context)
        {
            var jsonObjectContract = context.JsonContract as JsonObjectContract;
            if (jsonObjectContract == null) return;

            var commentId = XmlCommentsIdHelper.GetCommentIdForType(context.SystemType);
            var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, commentId));

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                    schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            if (schema.Properties == null) return;
            foreach (var entry in schema.Properties)
            {
                var jsonProperty = jsonObjectContract.Properties[entry.Key];
                if (jsonProperty == null) continue;

                var memberInfo = jsonProperty.MemberInfo();
                if (memberInfo != null)
                {
                    ApplyPropertyComments(entry.Value, memberInfo);
                }
            }
        }

        private void ApplyPropertyComments(Schema propertySchema, MemberInfo memberInfo)
        {
            var commentId = XmlCommentsIdHelper.GetCommentIdForMember(memberInfo);
            var memberNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, commentId));
            if (memberNode == null) return;

            var summaryNode = memberNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                propertySchema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }
        }
    }
}
