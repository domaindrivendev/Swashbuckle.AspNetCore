using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MemberInfoExtensions
    {
        public static IEnumerable<object> GetInlineOrMetadataTypeAttributes(this MemberInfo memberInfo)
        {
            var metadataTypeAttribute = memberInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<ModelMetadataTypeAttribute>()
                .FirstOrDefault();

            var metadataTypeMemberInfo = metadataTypeAttribute?.MetadataType.GetMember(memberInfo.Name)
                .FirstOrDefault();

            return (metadataTypeMemberInfo == null)
                ? memberInfo.GetCustomAttributes(true)
                : metadataTypeMemberInfo.GetCustomAttributes(true);
        }
    }
}