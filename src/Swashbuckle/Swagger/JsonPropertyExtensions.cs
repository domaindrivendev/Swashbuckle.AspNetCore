using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
{
    public static class JsonPropertyExtensions
    {
        public static bool IsRequired(this JsonProperty jsonProperty)
        {
            return jsonProperty.Required == Newtonsoft.Json.Required.AllowNull
                || jsonProperty.Required == Newtonsoft.Json.Required.Always
                || jsonProperty.HasAttribute<RequiredAttribute>();
        }

        public static bool IsObsolete(this JsonProperty jsonProperty)
        {
            return jsonProperty.HasAttribute<ObsoleteAttribute>();
        }

        public static bool HasAttribute<T>(this JsonProperty jsonProperty)
            where T : Attribute
        {
            var propInfo = jsonProperty.PropertyInfo();
            return propInfo != null && propInfo.GetCustomAttribute<T>() != null;
        }

        public static PropertyInfo PropertyInfo(this JsonProperty jsonProperty)
        {
            return jsonProperty.DeclaringType.GetProperty(jsonProperty.UnderlyingName, jsonProperty.PropertyType);
        }
    }
}
