using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.XPath;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Linq;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private const string NodeXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";
        private const string ExampleXPath = "example";
        private const string JsonPropertyAttributeFullName = "Newtonsoft.Json.JsonPropertyAttribute";
        private const string JsonPropertyAttributePropertyName = "PropertyName";

        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            TryApplyTypeComments(schema, context.Type);

            var propertiesToReview = new Dictionary<string, OpenApiSchema>();
            var contextMembers = context.Type.GetProperties();

            var membersWithJsonProperty = contextMembers
                .Where(x => x.CustomAttributes
                    .Any(y => JsonPropertyAttributeFullName.Equals(y?.AttributeType?.FullName)));

            foreach (var entry in schema.Properties)
            {
                var propertyName = entry.Key;
                var propertySchema = entry.Value;

                if (propertySchema.Reference != null) continue; // can't add descriptions to a reference schema

                // Try to find the backing member by name (i.e. best effort)
                var memberInfo = contextMembers
                    .FirstOrDefault(m => m.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

                if (memberInfo == null)
                {
                    propertiesToReview.Add(propertyName, propertySchema);

                    continue;
                }

                TryApplyMemberComments(propertySchema, memberInfo);
            }

            // check members using 'Newtonsoft.Json.JsonPropertyAttribute'
            foreach (var memberWithJsonProperty in membersWithJsonProperty)
            {
                // iterate custom attributes
                foreach(var attribute in memberWithJsonProperty.GetCustomAttributes())
                {
                    var property = attribute.GetType().GetProperty(JsonPropertyAttributePropertyName);
                    var propertyValue = property?.GetValue(attribute, null)?.ToString();

                    if (propertiesToReview.Any(x => x.Key.Equals(propertyValue)))
                    {
                        TryApplyMemberComments(propertiesToReview[propertyValue], memberWithJsonProperty);
                    }
                }
            }
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
            var culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                typedValue = TypeDescriptor.GetConverter(memberType).ConvertFrom(stringValue);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                CultureInfo.CurrentCulture = culture;
            }

            return OpenApiAnyFactory.CreateFor(schema, typedValue);
        }
    }
}
