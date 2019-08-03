using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class ModelMetadataExtensions
    {
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
