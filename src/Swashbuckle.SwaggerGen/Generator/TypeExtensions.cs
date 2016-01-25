using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace Swashbuckle.SwaggerGen.Generator
{
    public static class TypeExtensions
    {
        public static string FriendlyId(this Type type, bool fullyQualified = false)
        {
            var typeName = fullyQualified
                ? type.FullNameSansTypeParameters().Replace("+", ".")
                : type.Name;

            if (type.GetTypeInfo().IsGenericType)
            {
                var genericArgumentIds = type.GetGenericArguments()
                    .Select(t => t.FriendlyId(fullyQualified))
                    .ToArray();

                return new StringBuilder(typeName)
                    .Replace(string.Format("`{0}", genericArgumentIds.Count()), string.Empty)
                    .Append(string.Format("[{0}]", string.Join(",", genericArgumentIds).TrimEnd(',')))
                    .ToString();
            }

            return typeName;
        }

        public static string FullNameSansTypeParameters(this Type type)
        {
            var fullName = type.FullName;
            var chopIndex = fullName.IndexOf("[[");
            return (chopIndex == -1) ? fullName : fullName.Substring(0, chopIndex);
        }

        // Need to figure out dependencies for using [EnumMemberAttribute] in Core
        //public static string[] GetEnumNamesForSerialization(this Type enumType)
        //{
        //    return enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
        //        .Select(fieldInfo =>
        //        {
        //            var memberAttribute = fieldInfo.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
        //            return (memberAttribute == null || string.IsNullOrWhiteSpace(memberAttribute.Value))
        //                ? fieldInfo.Name
        //                : memberAttribute.Value;
        //        })
        //        .ToArray();
        //}
    }
}