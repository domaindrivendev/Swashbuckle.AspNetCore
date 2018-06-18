using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class ModelMetaDataSchemaFilter : ISchemaFilter
    {
        public ModelMetaDataSchemaFilter() { }
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            if (context.ModelMetadata is DefaultModelMetadata defaultModelMetadata)
            {
                SetAttributes(schema, defaultModelMetadata);
            }
            if (schema.Properties == null || context.ModelMetadata?.Properties == null) return;
            foreach (var prop in context.ModelMetadata.Properties)
            {
                var propMetadata = context.ModelMetadata.Properties.SingleOrDefault(x => string.Equals(x.PropertyName, prop.PropertyName, System.StringComparison.CurrentCultureIgnoreCase));
                if (!(propMetadata is DefaultModelMetadata defaultPropModelMetadata))
                {
                    continue;
                }
                string propertyName = propMetadata.PropertyName;
                var jsonProperty = defaultPropModelMetadata.Attributes.Attributes.SingleOrDefault(x=> x is JsonPropertyAttribute) as JsonPropertyAttribute;
                if (jsonProperty != null)
                {
                    propertyName = jsonProperty.PropertyName;
                }
                var propertyKeyValue = schema.Properties.SingleOrDefault(x => string.Equals(x.Key, propertyName, System.StringComparison.CurrentCultureIgnoreCase));
                if (propertyKeyValue.Equals(default(KeyValuePair<string, Schema>)) == false)
                {
                    SetAttributes(propertyKeyValue.Value, defaultPropModelMetadata);
                }
            }
            
        }

        private static void SetAttributes(Schema schema, DefaultModelMetadata modelMetadata)
        {
            if (schema.Description == null)
            {

                var attribute = modelMetadata.Attributes.Attributes.LastOrDefault(x => x is DescriptionAttribute) as DescriptionAttribute;
                if (attribute != null)
                {
                    schema.Description = attribute.Description;
                }
            }
            if(schema.Title == null)
            {
                schema.Title = modelMetadata.DisplayName;
            }
        }
    }
}
