using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class PropertyInfoExtensions
    {
        public static bool HasAttribute<TAttribute>(this PropertyInfo property)
            where TAttribute : Attribute
        {
            return property.GetCustomAttribute<TAttribute>() != null;
        }

        public static bool IsPubliclyReadable(this PropertyInfo property)
        {
            return property.GetMethod?.IsPublic == true;
        }

        public static bool IsPubliclyWritable(this PropertyInfo property)
        {
            return property.SetMethod?.IsPublic == true;
        }

        public static IEnumerable<object> GetInlineOrMetadataTypeAttributes(this PropertyInfo property)
        {
            var metadataTypeAttribute = property.DeclaringType.GetCustomAttributes(true)
                .OfType<ModelMetadataTypeAttribute>()
                .FirstOrDefault();

            var metadataTypeMemberInfo = metadataTypeAttribute?.MetadataType.GetMember(property.Name)
                .FirstOrDefault();

            return (metadataTypeMemberInfo == null)
                ? property.GetCustomAttributes(true)
                : metadataTypeMemberInfo.GetCustomAttributes(true);
        }
    }
}
