using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen.Generator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class ModelMetaDataSchemaFilter : ISchemaFilter
    {
        public ModelMetaDataSchemaFilter() { }
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            SetAttributes(schema, context.ModelMetadata);
            if (!(context.JsonContract is JsonObjectContract jsonObjectContract)) return;
            foreach (var prop in jsonObjectContract.Properties)
            {
                var ignore = context.ModelMetadata.GetAttribute<JsonIgnoreAttribute>(prop.UnderlyingName);
                if (ignore != null)
                    continue;
                SetAttributes(schema.Properties[prop.PropertyName], context.ModelMetadata, prop.UnderlyingName);
            }

        }
        private static void SetAttributes(Schema schema, ModelMetadata modelMetadata, string propertyName = null)
        {
            if(string.IsNullOrEmpty(schema.Description))
                schema.Description = modelMetadata.GetAttribute<DescriptionAttribute>(propertyName)?.Description;
            if (schema.Default == null)
                schema.Default = modelMetadata.GetAttribute<DefaultValueAttribute>(propertyName)?.Value;
            if (string.IsNullOrEmpty(schema.Pattern))
                schema.Pattern = modelMetadata.GetAttribute<RegularExpressionAttribute>(propertyName)?.Pattern;
            if (schema.Maximum == null && schema.Minimum == null)
            {
                var range = modelMetadata.GetAttribute<RangeAttribute>(propertyName);
                if (range != null)
                {
                    if (Int32.TryParse(range.Maximum.ToString(), out int maximum))
                        schema.Maximum = maximum;

                    if (Int32.TryParse(range.Minimum.ToString(), out int minimum))
                        schema.Minimum = minimum;
                }
            }
            if(schema.MinLength == null)
                schema.MinLength = modelMetadata.GetAttribute<MinLengthAttribute>(propertyName)?.Length;
            if(schema.MaxLength == null)
                schema.Maximum = modelMetadata.GetAttribute<MaxLengthAttribute>(propertyName)?.Length;
            if(schema.MinLength == null && schema.MaxLength == null)
            {
                var stringLengthAttribute = modelMetadata.GetAttribute<StringLengthAttribute>(propertyName);
                if (stringLengthAttribute != null)
                {
                    schema.MinItems = stringLengthAttribute.MinimumLength;
                    schema.MaxLength = stringLengthAttribute.MaximumLength;
                }
            }
            if (string.IsNullOrEmpty(schema.Title))
                schema.Title = modelMetadata.GetAttribute<DisplayNameAttribute>(propertyName)?.DisplayName;
            if(string.IsNullOrEmpty(schema.Format) && schema.Type == "string")
            {
                var dataTypeAttribute = modelMetadata.GetAttribute<DataTypeAttribute>(propertyName);
        
                if (dataTypeAttribute != null && DataTypeFormatMap.TryGetValue(dataTypeAttribute.DataType, out string format))
                    schema.Format = format;
            }
        }
        private static readonly Dictionary<DataType, string> DataTypeFormatMap = new Dictionary<DataType, string>
        {
            { DataType.Date, "date" },
            { DataType.DateTime, "date-time" },
            { DataType.Password, "password" }
        };
    }
}