using System;
using System.Linq;
using System.Text;

namespace Swashbuckle.Swagger.XmlComments
{
    public static class TypeExtensions
    {
        public static string XmlLookupName(this Type type)
        {
            var builder = new StringBuilder(type.FullNameSansTypeParameters());
            return builder
                .Replace("+", ".")
                .ToString();
        }

        public static string XmlLookupNameWithTypeParameters(this Type type)
        {
            var builder = new StringBuilder(type.XmlLookupName());

            if (type.IsGenericType)
            {
                var genericArgumentIds = type.GetGenericArguments()
                    .Select(t => t.XmlLookupNameWithTypeParameters())
                    .ToArray();

                builder
                    .Replace(string.Format("`{0}", genericArgumentIds.Count()), string.Empty)
                    .Append(string.Format("{{{0}}}", string.Join(",", genericArgumentIds).TrimEnd(',')))
                    .ToString();
            }

            return builder.ToString();
        }
    }
}