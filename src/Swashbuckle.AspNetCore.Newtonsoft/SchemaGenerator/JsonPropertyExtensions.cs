﻿using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public static class JsonPropertyExtensions
    {
        public static bool TryGetMemberInfo(this JsonProperty jsonProperty, out MemberInfo memberInfo)
        {
            memberInfo = jsonProperty.DeclaringType?.GetMember(jsonProperty.UnderlyingName)
                .FirstOrDefault();

            return (memberInfo != null);
        }

        public static bool IsRequiredSpecified(this JsonProperty jsonProperty)
        {
            return jsonProperty.Required != Required.Default;
        }
    }
}
