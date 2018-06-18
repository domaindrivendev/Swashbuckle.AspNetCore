using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class JsonPropertyExtensions
    {
        internal static bool IsRequired(this JsonProperty jsonProperty)
        {
            if (jsonProperty.Required == Newtonsoft.Json.Required.AllowNull)
                return true;

            if (jsonProperty.Required == Newtonsoft.Json.Required.Always)
                return true;

            if (jsonProperty.HasAttribute<RequiredAttribute>())
                return true;

            return false;
        }

        internal static bool IsObsolete(this JsonProperty jsonProperty)
        {
            return jsonProperty.HasAttribute<ObsoleteAttribute>();
        }

        internal static bool HasAttribute<T>(this JsonProperty jsonProperty)
            where T : Attribute
        {
            if (!jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo))
                return false;

            return memberInfo.GetCustomAttribute<T>() != null;
        }

        internal static bool TryGetMemberInfo(this JsonProperty jsonProperty, out MemberInfo memberInfo)
        {
            if (jsonProperty.UnderlyingName == null)
            {
                memberInfo = null;
                return false;
            }

            var metadataAttribute = jsonProperty.DeclaringType.GetTypeInfo()
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
