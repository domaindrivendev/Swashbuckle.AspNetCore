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
            var propInfo = jsonProperty.PropertyInfo();
            return propInfo != null && propInfo.GetCustomAttribute<T>() != null;
        }

        internal static PropertyInfo PropertyInfo(this JsonProperty jsonProperty)
        {
            if (jsonProperty.UnderlyingName == null) return null;

            var metadata = jsonProperty.DeclaringType.GetTypeInfo()
                .GetCustomAttributes(typeof(ModelMetadataTypeAttribute), true)
                .FirstOrDefault();

            var typeToReflect = (metadata != null)
                ? ((ModelMetadataTypeAttribute)metadata).MetadataType
                : jsonProperty.DeclaringType;

            return typeToReflect.GetProperty(jsonProperty.UnderlyingName, jsonProperty.PropertyType);
        }
    }
}
