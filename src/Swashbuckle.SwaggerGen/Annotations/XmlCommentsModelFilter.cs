using System.Xml.XPath;
using System.Reflection;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.Swagger.Model;

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
                    model.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            foreach (var entry in model.Properties)
            {
                var jsonProperty = context.JsonObjectContract.Properties[entry.Key];
                if (jsonProperty == null) continue;

                var propertyInfo = jsonProperty.PropertyInfo();
                if (propertyInfo != null)
                {
                    ApplyPropertyComments(entry.Value, propertyInfo);
                }
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
                propertySchema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }
        }
    }
}
