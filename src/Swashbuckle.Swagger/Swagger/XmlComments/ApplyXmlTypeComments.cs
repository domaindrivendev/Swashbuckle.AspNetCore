using System.Xml.XPath;
using System.Reflection;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlTypeComments : IModelFilter
    {
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string SummaryExpression = "summary";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}.{1}']";

        private readonly XPathNavigator _navigator;

        public ApplyXmlTypeComments(string filePath)
        {
            _navigator = new XPathDocument(filePath).CreateNavigator();
        }

        public void Apply(Schema model, ModelFilterContext context)
        {
            var typeXPath = string.Format(TypeExpression, context.SystemType.XmlLookupName());
            var typeNode = _navigator.SelectSingleNode(typeXPath);

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryExpression);
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

        private void ApplyPropertyComments(Schema propertySchema, MemberInfo memberInfo)
        {
            var propertyXPath =
                string.Format(PropertyExpression, memberInfo.DeclaringType.XmlLookupName(), memberInfo.Name);
            var propertyNode = _navigator.SelectSingleNode(propertyXPath);
            if (propertyNode == null) return;

            var propSummaryNode = propertyNode.SelectSingleNode(SummaryExpression);
            if (propSummaryNode != null)
            {
                propertySchema.Description = propSummaryNode.ExtractContent();
            }
        }
    }
}