using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsRequestBodyFilter : IRequestBodyFilter
    {
        private readonly IReadOnlyDictionary<string, XPathNavigator> _xmlDocMembers;

        public XmlCommentsRequestBodyFilter(XPathDocument xmlDoc) : this(XmlCommentsDocumentHelper.CreateMemberDictionary(xmlDoc))
        {
        }
        internal XmlCommentsRequestBodyFilter(IReadOnlyDictionary<string, XPathNavigator> xmlDocMembers)
        {
            _xmlDocMembers = xmlDocMembers;
        }

        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            var bodyParameterDescription = context.BodyParameterDescription;

            if (bodyParameterDescription is not null)
            {
                var propertyInfo = bodyParameterDescription.PropertyInfo();
                if (propertyInfo is not null)
                {
                    ApplyPropertyTagsForBody(requestBody, context, propertyInfo);
                    return;
                }
                var parameterInfo = bodyParameterDescription.ParameterInfo();
                if (parameterInfo is not null)
                {
                    ApplyParamTagsForBody(requestBody, context, parameterInfo);
                    return;
                }
            }
            else
            {
                var numberOfFromForm = context.FormParameterDescriptions?.Count() ?? 0;
                if (requestBody.Content?.Count is 0 || numberOfFromForm == 0)
                {
                    return;
                }

                foreach (var formParameter in context.FormParameterDescriptions)
                {
                    if (formParameter.PropertyInfo() is not null || formParameter.Name is null)
                    {
                        continue;
                    }

                    var parameterFromForm = formParameter.ParameterInfo();
                    if (parameterFromForm is null)
                    {
                        continue;
                    }

                    foreach (var item in requestBody.Content.Values)
                    {
                        if ((item?.Schema?.Properties?.TryGetValue(formParameter.Name, out var value) ?? false)
                            || (item?.Schema?.Properties?.TryGetValue(formParameter.Name.ToCamelCase(), out value) ?? false))
                        {
                            var (summary, example) = GetParamTags(parameterFromForm);
                            value.Description = summary;
                            if (!string.IsNullOrEmpty(example))
                            {
                                value.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, value, example);
                            }
                        }
                    }
                }
            }
        }

        private (string summary, string example) GetPropertyTags(PropertyInfo propertyInfo)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);
            if (!_xmlDocMembers.TryGetValue(propertyMemberName, out var propertyNode))
            {
                return (null, null);
            }

            string summary = null;
            var summaryNode = propertyNode.SelectFirstChild("summary");
            if (summaryNode is not null)
            {
                summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }
            var exampleNode = propertyNode.SelectFirstChild("example");
            if (exampleNode is null)
            {
                return (summary, null);
            }

            return (summary, exampleNode.ToString());

        }

        private void ApplyPropertyTagsForBody(OpenApiRequestBody requestBody, RequestBodyFilterContext context, PropertyInfo propertyInfo)
        {
            var (summary, example) = GetPropertyTags(propertyInfo);

            if (summary is not null)
            {
                requestBody.Description = summary;
            }

            if (requestBody.Content?.Count is 0)
            {
                return;
            }

            foreach (var mediaType in requestBody.Content.Values)
            {
                mediaType.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, mediaType.Schema, example);
            }
        }

        private (string summary, string example) GetParamTags(ParameterInfo parameterInfo)
        {
            if (parameterInfo.Member is not MethodInfo methodInfo)
            {
                return (null, null);
            }
            var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
                ? methodInfo.GetUnderlyingGenericTypeMethod()
                : methodInfo;

            if (targetMethod is null)
            {
                return (null, null);
            }
            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(targetMethod);

            if (!_xmlDocMembers.TryGetValue(methodMemberName, out var propertyNode))
            {
                return (null, null);
            }
            var paramNode = propertyNode.SelectFirstChildWithAttribute("param", "name", parameterInfo.Name);

            if (paramNode is null)
            {
                return (null, null);
            }

            var summary = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
            var example = paramNode.GetAttribute("example");

            return (summary, example);

        }

        private void ApplyParamTagsForBody(OpenApiRequestBody requestBody, RequestBodyFilterContext context, ParameterInfo parameterInfo)
        {
            var (summary, example) = GetParamTags(parameterInfo);

            if (summary is not null)
            {
                requestBody.Description = summary;
            }

            if (requestBody.Content?.Count is 0)
            {
                return;
            }

            if (!string.IsNullOrEmpty(example))
            {
                foreach (var mediaType in requestBody.Content.Values)
                {
                    mediaType.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, mediaType.Schema, example);
                }
            }
        }
    }
}
