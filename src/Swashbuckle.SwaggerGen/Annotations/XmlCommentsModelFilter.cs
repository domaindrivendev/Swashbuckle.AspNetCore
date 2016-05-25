using System.Xml.XPath;
using System.Reflection;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.Extensions;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class XmlCommentsModelFilter : IModelFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsModelFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(Schema model, ModelFilterContext context)
        {
            var commentId = XmlCommentsIdHelper.GetCommentIdForType(context.SystemType);
            var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, commentId));

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                    model.Description = summaryNode.ExtractContent();
            }

            foreach (var entry in model.Properties)
            {
                var jsonProperty = context.JsonObjectContract.Properties[entry.Key];
                if (jsonProperty == null) continue;

                ApplyPropertyComments(entry.Value, jsonProperty.PropertyInfo());
            }
        }

        private void ApplyPropertyComments(Schema propertySchema, PropertyInfo propertyInfo)
        {

            var commentId = XmlCommentsIdHelper.GetCommentIdForProperty(propertyInfo);
            var propertyNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, commentId));
            if (propertyNode == null) return;

            var summaryNode = propertyNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                propertySchema.Description = summaryNode.ExtractContent();
            }
        }
    }
}