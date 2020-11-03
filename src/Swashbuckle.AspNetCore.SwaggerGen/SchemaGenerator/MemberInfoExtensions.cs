using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MemberInfoExtensions
    {
        public static T GetCustomOrMetadataAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomOrMetadataAttributes<T>().FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomOrMetadataAttributes<T>(this MemberInfo memberInfo) where T : Attribute
        {
            var attributes = memberInfo.GetCustomAttributes<T>()
                .ToList();

            var metadataTypeAttribute = memberInfo.DeclaringType.GetCustomAttribute<ModelMetadataTypeAttribute>();
            var metadataMemberInfo = metadataTypeAttribute?.MetadataType.GetMember(memberInfo.Name)
                .FirstOrDefault();

            if (metadataMemberInfo != null)
            {
                attributes.AddRange(metadataMemberInfo.GetCustomAttributes<T>());
            }

            return attributes;

        }

        public static bool HasCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttribute<T>() != null;
        }

        public static bool HasCustomOrMetadataAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomOrMetadataAttribute<T>() != null;
        }
    }
}
