using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ModelMetadataExtensions
    {
        public static bool IsSetType(this ModelMetadata modelMetadata)
        {
            return new[] { modelMetadata.ModelType }
                .Union(modelMetadata.ModelType.GetInterfaces())
                .Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));
        }

        public static bool TryGetDictionaryMetadata(this ModelMetadata modelMetadata, out ModelMetadata keyMetadata, out ModelMetadata valueMetadata)
        {
            // Generic form
            var constructedDictionary = new[] { modelMetadata.ModelType }
                .Union(modelMetadata.ModelType.GetInterfaces())
                .Where(i => i.IsConstructedGenericType)
                .FirstOrDefault(i => new[] { typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>) }.Contains(i.GetGenericTypeDefinition()));

            if (constructedDictionary != null)
            {
                keyMetadata = modelMetadata.GetMetadataForType(constructedDictionary.GenericTypeArguments[0]);
                valueMetadata = modelMetadata.GetMetadataForType(constructedDictionary.GenericTypeArguments[1]);
                return true;
            }

            // Non-generic form
            if (typeof(IDictionary).IsAssignableFrom(modelMetadata.ModelType))
            {
                keyMetadata = valueMetadata = modelMetadata.GetMetadataForType(typeof(object));
                return true;
            }

            keyMetadata = valueMetadata = null;
            return false;
        }

        public static MemberInfo GetMemberInfo(this ModelMetadata modelMetadata)
        {
            return (modelMetadata.ContainerType != null)
                ? modelMetadata.ContainerType.GetMember(modelMetadata.PropertyName).FirstOrDefault()
                : null;
        }

        public static IEnumerable<object> GetCustomAttributes(this ModelMetadata modelMetadata)
        {
            if (modelMetadata.ContainerType == null)
                return Enumerable.Empty<object>();

            var metadataAttribute = modelMetadata.ContainerType
                .GetCustomAttributes(true)
                .OfType<ModelMetadataTypeAttribute>()
                .FirstOrDefault();

            var typeToReflect = (metadataAttribute != null)
                ? metadataAttribute.MetadataType
                : modelMetadata.ContainerType;

            var memberInfo = typeToReflect.GetMember(modelMetadata.PropertyName)
                .FirstOrDefault();

            return (memberInfo != null)
                ? memberInfo.GetCustomAttributes(true)
                : Enumerable.Empty<object>();
        }
    }
}
