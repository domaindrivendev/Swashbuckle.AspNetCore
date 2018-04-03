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
            return jsonProperty.Required == Newtonsoft.Json.Required.AllowNull
                || jsonProperty.Required == Newtonsoft.Json.Required.Always
                || jsonProperty.HasAttribute<RequiredAttribute>();
        }

        internal static bool IsObsolete(this JsonProperty jsonProperty)
        {
            return jsonProperty.HasAttribute<ObsoleteAttribute>();
        }

        internal static bool HasAttribute<T>(this JsonProperty jsonProperty)
            where T : Attribute
        {
            var memberInfo = jsonProperty.MemberInfo();
            return memberInfo != null && memberInfo.GetCustomAttribute<T>() != null;
        }

        internal static MemberInfo MemberInfo(this JsonProperty jsonProperty)
        {
            if (jsonProperty.UnderlyingName == null) return null;

            var metadataAttribute = jsonProperty.DeclaringType.GetTypeInfo()
                .GetCustomAttributes(typeof(ModelMetadataTypeAttribute), true)
                .FirstOrDefault();

            var typeToReflect = (metadataAttribute != null)
                ? ((ModelMetadataTypeAttribute)metadataAttribute).MetadataType
                : jsonProperty.DeclaringType;

            return typeToReflect.GetMember(jsonProperty.UnderlyingName).FirstOrDefault();
        }
    }
}
