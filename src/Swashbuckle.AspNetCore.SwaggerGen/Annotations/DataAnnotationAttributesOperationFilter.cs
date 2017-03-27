using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class DataAnnotationAttributesOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            for (var index = 0; index < operation.Parameters.Count; index++)
            {
                var parameter = operation.Parameters[index];
                if (parameter is NonBodyParameter nonBodyParam)
                {
                    var propertyParam = context.ApiDescription.ParameterDescriptions
                       .Where(p => p.ModelMetadata?.ContainerType != null && p.ModelMetadata?.PropertyName != null)
                        .FirstOrDefault(p => parameter.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                    if (propertyParam == null)
                    {
                        continue;
                    }

                    var metadata = propertyParam.ModelMetadata;
                    var propertyInfo = metadata.ContainerType.GetTypeInfo().GetProperty(metadata.PropertyName);
                    if (propertyInfo == null)
                    {
                        continue;
                    }

                    var attributes = propertyInfo.GetCustomAttributes();

                    HandleAttribute(attributes, GetRequiredAttributeAction(nonBodyParam));
                    HandleAttribute(attributes, GetRegularExpressionAttributeAction(nonBodyParam));
                    HandleAttribute(attributes, GetDefaultValueAttributeAction(nonBodyParam));
                    HandleAttribute(attributes, GetRangeAttributeAction(nonBodyParam));
                    HandleAttribute(attributes, GetMinLengthAttributeAction(nonBodyParam));
                    HandleAttribute(attributes, GetMaxLengthAttributeAction(nonBodyParam));
                    HandleAttribute(attributes, GetStringLengthAttributeAction(nonBodyParam));
                }
            }
        }

        private void HandleAttribute<TAttribute>(IEnumerable<Attribute> attributes, Action<TAttribute> action)
            where TAttribute : Attribute
        {
            if (attributes?.LastOrDefault(x => x.GetType() == typeof(TAttribute)) is TAttribute attribute)
            {
                action(attribute);
            }
        }

        private Action<RequiredAttribute> GetRequiredAttributeAction(NonBodyParameter parameter)
        {
            return (RequiredAttribute attr) => parameter.Required = true;
        }

        private Action<RegularExpressionAttribute> GetRegularExpressionAttributeAction(NonBodyParameter parameter)
        {
            return (RegularExpressionAttribute attr) => parameter.Pattern = attr.Pattern;
        }

        private Action<DefaultValueAttribute> GetDefaultValueAttributeAction(NonBodyParameter parameter)
        {
            return (DefaultValueAttribute attr) => parameter.Default = attr.Value;
        }

        private Action<RangeAttribute> GetRangeAttributeAction(NonBodyParameter parameter)
        {
            return (RangeAttribute attr) =>
            {
                if (int.TryParse(attr.Maximum.ToString(), out int maximum))
                {
                    parameter.Maximum = maximum;
                }

                if (int.TryParse(attr.Minimum.ToString(), out int minimum))
                {
                    parameter.Minimum = minimum;
                }
            };
        }

        private Action<MinLengthAttribute> GetMinLengthAttributeAction(NonBodyParameter parameter)
        {
            return (MinLengthAttribute attr) => parameter.MinLength = attr.Length;
        }

        private Action<MaxLengthAttribute> GetMaxLengthAttributeAction(NonBodyParameter parameter)
        {
            return (MaxLengthAttribute attr) => parameter.MaxLength = attr.Length;
        }

        private Action<StringLengthAttribute> GetStringLengthAttributeAction(NonBodyParameter parameter)
        {
            return (StringLengthAttribute attr) =>
            {
                parameter.MinLength = attr.MinimumLength;
                parameter.MaxLength = attr.MaximumLength;
            };
        }
    }
}
