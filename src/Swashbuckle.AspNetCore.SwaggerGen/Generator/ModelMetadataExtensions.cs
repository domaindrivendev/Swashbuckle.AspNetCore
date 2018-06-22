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
        internal static List<T> GetReflectedAttributes<T>(this ModelMetadata modelMetadata, string propertyName) where T : Attribute
        {
            var reflectedType = modelMetadata.GetReflectedType(propertyName);
            if (reflectedType == null) return new List<T>();
            return reflectedType.GetCustomAttributes<T>().ToList();
        }
        internal static MemberInfo GetReflectedType(this ModelMetadata modelMetadata, string propertyName)
        {
            var modelMetadataTypeAttribute = modelMetadata.ModelType.GetTypeInfo().GetCustomAttributes<ModelMetadataTypeAttribute>(false).FirstOrDefault();
            if (modelMetadataTypeAttribute == null) return null;

            var reflectedType = modelMetadataTypeAttribute.MetadataType.GetMember(propertyName).FirstOrDefault();
            return reflectedType;
        }
        internal static IList<T> GetAttributes<T>(this ModelMetadata modelMetadata) where T : Attribute
        {
            var type = modelMetadata.ModelType.GetTypeInfo();
            var attribute = type.GetCustomAttributes<T>(true).ToList();
            return attribute;
        }
        internal static IList<T> GetAttributes<T>(this ModelMetadata modelMetadata, string propertyName) where T : Attribute
        {
            if (propertyName == null || modelMetadata.Properties.Any(x => x.PropertyName == propertyName) == false) return new List<T>();
            
            var property = modelMetadata.ModelType.GetProperty(propertyName);
            var attribute = property.GetCustomAttributes<T>(true)
                            .Union(modelMetadata.GetReflectedAttributes<T>(propertyName)).ToList();
            return attribute;
        }
        internal static T GetAttribute<T>(this ModelMetadata modelMetadata, string propertyName) where T : Attribute
        {
            return GetAttributes<T>(modelMetadata, propertyName).OfType<T>().LastOrDefault();
        }
    }
}
