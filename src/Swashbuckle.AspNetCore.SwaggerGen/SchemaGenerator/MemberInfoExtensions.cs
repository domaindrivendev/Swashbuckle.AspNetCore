using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System;
namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MemberInfoExtensions
    {
        public static IEnumerable<object> GetInlineOrMetadataTypeAttributes(this MemberInfo memberInfo)
        {
            if (memberInfo==null) throw new NullReferenceException(nameof(memberInfo));
            var metadataTypeAttribute = memberInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<ModelMetadataTypeAttribute>()
                .FirstOrDefault();
            if (memberInfo.Name==null) throw new NullReferenceException(nameof(memberInfo.Name));

            var metadataTypeMemberInfo = metadataTypeAttribute?.MetadataType.GetMember(memberInfo.Name)
                .FirstOrDefault();

            return (metadataTypeMemberInfo == null)
                ? memberInfo.GetCustomAttributes(true)
                : metadataTypeMemberInfo.GetCustomAttributes(true);
        }
    }
}
