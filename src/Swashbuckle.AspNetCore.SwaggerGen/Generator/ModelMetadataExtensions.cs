using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Swashbuckle.AspNetCore.SwaggerGen.Generator
{
    internal static class ModelMetadataExtensions
    {
        internal static T GetAttribute<T>(this ModelMetadata modelMetadata, string propertyName) where T : Attribute
        {
            if (modelMetadata.HasProperty(propertyName))
            {
                var property = modelMetadata.ModelType.GetProperty(propertyName);
                var attribute = property.GetCustomAttributes<T>(true).LastOrDefault();
                return attribute;
            }
            else
            {
                var attribute = modelMetadata.ModelType.GetTypeInfo().GetCustomAttributes<T>(true).LastOrDefault();
                return attribute;
            }
        }

        internal static bool HasProperty(this ModelMetadata modelMetadata, string propertyName)
        {
            return modelMetadata.Properties.Any(x => x.PropertyName == propertyName);
        }
    }
}
