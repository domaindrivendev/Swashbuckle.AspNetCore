using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Gets custom attribute data.
        /// </summary>
        /// <param name="memberInfo">Member info.</param>
        /// <param name="attributeType">Attribute type.</param>
        /// <param name="inherit">Search inheritance chain.</param>
        /// <returns>Custom attribute data</returns>
        public static CustomAttributeData GetCustomAttributeData(this MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            var customAttrData = memberInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == attributeType);
            if (customAttrData == null && inherit)
            {
                var baseType = memberInfo.DeclaringType.BaseType;
                var baseMemberInfo = baseType?.GetMember(memberInfo.Name).FirstOrDefault();
                customAttrData = baseMemberInfo?.GetCustomAttributeData(attributeType, inherit);
            }
            return customAttrData;
        }
    }
}
