﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.XPath;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsSchemaFilter : ISchemaFilter
    {
        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ExampleTag = "example";

        private readonly XPathNavigator _xmlNavigator;
        private readonly bool _includeRemarksFromXmlComments;

        public XmlCommentsSchemaFilter(XPathDocument xmlDoc, bool includeRemarksFromXmlComments = false)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
            _includeRemarksFromXmlComments = includeRemarksFromXmlComments;
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            ApplyTypeTags(schema, context.Type);

            // If it's for a C# field/property and it's NOT bound to a request parameter (e.g. via [FromRoute], [FromQuery] etc.),
            // then the field/property tags can be applied here. If it is bound to a request parameter, the field/property tags
            // will be applied a level up on the corresponding OpenApiParameter (see XmlCommentsParameterFilter).

            if (context.MemberInfo != null && context.ParameterInfo == null)
            {
                ApplyFieldOrPropertyTags(schema, context.MemberInfo);
            }
        }

        private void ApplyTypeTags(OpenApiSchema schema, Type type)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
            var typeSummaryNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{typeMemberName}']/{SummaryTag}");

            if (typeSummaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml);

                if (_includeRemarksFromXmlComments)
                {
                    var typeRemarksNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{typeMemberName}']/{RemarksTag}");
                    if (typeRemarksNode != null && !string.IsNullOrWhiteSpace(typeRemarksNode.InnerXml))
                    {
                        schema.Description +=
                            $" ({XmlCommentsTextHelper.Humanize(typeRemarksNode.InnerXml)})";
                    }
                }
            }
        }

        private void ApplyFieldOrPropertyTags(OpenApiSchema schema, MemberInfo fieldOrPropertyInfo)
        {
            var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(fieldOrPropertyInfo);
            var fieldOrPropertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{fieldOrPropertyMemberName}']");

            if (fieldOrPropertyNode == null) return;

            var summaryNode = fieldOrPropertyNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                if (_includeRemarksFromXmlComments)
                {
                    var remarksNode = fieldOrPropertyNode.SelectSingleNode(RemarksTag);
                    if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                    {
                        schema.Description +=
                            $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                    }
                }
            }

            var exampleNode = fieldOrPropertyNode.SelectSingleNode(ExampleTag);
            if (exampleNode != null)
            {
                var exampleString = XmlCommentsTextHelper.Humanize(exampleNode.InnerXml);

                if (fieldOrPropertyInfo is FieldInfo fieldInfo)
                {
                    schema.Example = ConvertToOpenApiType(fieldInfo.FieldType, schema, exampleString);
                }
                else if (fieldOrPropertyInfo is PropertyInfo propertyInfo)
                {
                    schema.Example = ConvertToOpenApiType(propertyInfo.PropertyType, schema, exampleString);
                }
            }
        }

        private static IOpenApiAny ConvertToOpenApiType(Type type, OpenApiSchema schema, string stringValue)
        {
            object typedValue;

            try
            {
                typedValue = TypeDescriptor.GetConverter(type).ConvertFrom(
                    context: null,
                    culture: CultureInfo.InvariantCulture,
                    stringValue);
            }
            catch (Exception)
            {
                return null;
            }

            return OpenApiAnyFactory.CreateFor(schema, typedValue);
        }
    }
}
