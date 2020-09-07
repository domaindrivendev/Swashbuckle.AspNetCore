using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MemberInfoExtensions
    {
        public static IEnumerable<object> GetInlineAndMetadataAttributes(this MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(true).ToList();

            var metadataTypeAttribute = memberInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<ModelMetadataTypeAttribute>()
                .FirstOrDefault();

            var metadataMemberInfo = metadataTypeAttribute?.MetadataType.GetMember(memberInfo.Name)
                .FirstOrDefault();

            if (metadataMemberInfo != null)
            {
                // if the same attribute (or a derived version) exists on both the model class and its metadata
                // class, asp.net core uses the one on the model class - we need to conform with that behaviour
                foreach (var metadataAttribute in metadataMemberInfo.GetCustomAttributes(true))
                {
                    if (!attributes.Any(a => a.GetType() == metadataAttribute.GetType()
                                             || metadataAttribute.GetType().IsSubclassOf(a.GetType())))
                    {
                        attributes.Add(metadataAttribute);
                    }
                }
            }

            return attributes;
        }
    }
}
