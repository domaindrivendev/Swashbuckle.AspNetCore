using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class JsonPropertyExtensions
    {
        internal static bool TryGetMemberInfo(this JsonProperty jsonProperty, out MemberInfo memberInfo)
        {
            if (jsonProperty.UnderlyingName == null)
            {
                memberInfo = null;
                return false;
            }

            var metadataAttribute = jsonProperty.DeclaringType
                .GetCustomAttributes(typeof(ModelMetadataTypeAttribute), true)
                .FirstOrDefault();

            var typeToReflect = (metadataAttribute != null)
                ? ((ModelMetadataTypeAttribute)metadataAttribute).MetadataType
                : jsonProperty.DeclaringType;

            memberInfo = typeToReflect.GetMember(jsonProperty.UnderlyingName).FirstOrDefault();

            return (memberInfo != null);
        }
    }
}