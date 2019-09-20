using System;
using System.ComponentModel;
using System.Xml.XPath;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private const string NodeXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";
        private const string ExampleXPath = "example";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            TryApplyTypeComments(schema, context.ApiModel.Type);

            if (!(context.ApiModel is ApiObject apiObject))
                return;

            foreach (var apiProperty in apiObject.ApiProperties)
            {
                if (!schema.Properties.TryGetValue(apiProperty.ApiName, out OpenApiSchema propertySchema))
                    continue;

                if (propertySchema.Reference != null) // can't add descriptions to a reference schema
                    continue;

                if (apiProperty.MemberInfo == null)
                    continue;

                TryApplyMemberComments(propertySchema, apiProperty.MemberInfo);
            };
        }

        private void TryApplyTypeComments(OpenApiSchema schema, Type type)
        {
            var typeNodeName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
            var typeNode = _xmlNavigator.SelectSingleNode(string.Format(NodeXPath, typeNodeName));

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                    schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }
        }

        private void TryApplyMemberComments(OpenApiSchema schema, MemberInfo memberInfo)
        {
            var memberNodeName = XmlCommentsNodeNameHelper.GetNodeNameForMember(memberInfo);
            var memberNode = _xmlNavigator.SelectSingleNode(string.Format(NodeXPath, memberNodeName));
            if (memberNode == null) return;

            var summaryNode = memberNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            var exampleNode = memberNode.SelectSingleNode(ExampleXPath);
            if (exampleNode != null)
            {
                var exampleString = XmlCommentsTextHelper.Humanize(exampleNode.InnerXml);
                var memberType = (memberInfo.MemberType & MemberTypes.Field) != 0 ? ((FieldInfo) memberInfo).FieldType : ((PropertyInfo) memberInfo).PropertyType;
                schema.Example = ConvertToOpenApiType(memberType, schema, exampleString);
            }
        }

        private static IOpenApiAny ConvertToOpenApiType(Type memberType, OpenApiSchema schema, string stringValue)
        {
            object typedValue;

            try
            {
                typedValue = TypeDescriptor.GetConverter(memberType).ConvertFrom(stringValue);
            }
            catch (Exception)
            {
                return null;
            }

            return OpenApiAnyFactory.TryCreateFor(schema, typedValue, out IOpenApiAny openApiAny)
                ? openApiAny
                : null;
        }
    }
}
