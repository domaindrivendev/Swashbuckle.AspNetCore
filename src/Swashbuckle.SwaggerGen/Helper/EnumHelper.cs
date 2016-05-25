using Swashbuckle.SwaggerGen.Extensions;
using Swashbuckle.SwaggerGen.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.SwaggerGen.Helper
{
    internal static class EnumHelper
    {
        // Or - short-circuit evaluation
        internal static readonly string EnumPairSeperator = "|";

        internal static bool HasFlags(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
            return HasFlagsInternal(checkedType);
        }

        private static bool HasFlagsInternal(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
#if NET451
            FlagsAttribute attribute = type.GetCustomAttribute<FlagsAttribute>(inherit: false);
#endif
#if NETCOREAPP1_0 || NETSTANDARD1_5
            FlagsAttribute attribute = type.GetTypeInfo().GetCustomAttribute<FlagsAttribute>(inherit: false);
#endif
            return attribute != null;
        }

        // Return non-empty name specified in a [Display] attribute for the given field, if any; field's name otherwise
        private static string GetDisplayName(FieldInfo field, bool camelCase = false)
        {
            DisplayAttribute display = field.GetCustomAttribute<DisplayAttribute>(inherit: false);
            if (display != null)
            {
                string name = display.Name;
                if (!string.IsNullOrEmpty(name))
                    return camelCase ? name.ToCamelCase() : name;
            }

            return camelCase ? field.Name.ToCamelCase() : field.Name;
        }

        public static string GetDisplayName(this Enum instance, bool camelCase = false)
        {
            if (instance == null)
                return string.Empty;

            var fieldInfo = instance.GetType().GetField(instance.ToString());
            return GetDisplayName(fieldInfo, camelCase);
        }

        public static IList<IdTextPair> GetSelectList(Type type, bool camelCase = false)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            IList<IdTextPair> selectList = new List<IdTextPair>();

            Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
            if (checkedType != type)
            {
                // Underlying type was non-null so handle Nullable<T>; ensure returned list has a spot for null
                selectList.Add(new IdTextPair { Text = string.Empty, Id = string.Empty });
            }

            foreach (FieldInfo field in checkedType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static))
            {
                object fieldValue = field.GetRawConstantValue();
                selectList.Add(new IdTextPair { Text = GetDisplayName(field, camelCase), Id = fieldValue.ToString() });
            }

            return selectList.OrderBy(x => x.Id).ToList();
        }

        public static IList<object> GetEnumAsIdTextPair(Type type, bool camelCase = false)
        {
            var idPairList = GetSelectList(type, camelCase);
            IList<string> items = new List<string>();
            foreach (var item in idPairList)
                items.Add($"{item.Id}{EnumPairSeperator}{item.Text}");

            return items.ToArray();
        }

        public static IList<string> AsStringList<TEnum>(IList<TEnum> enums) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnumType())
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            IList<string> resultList = new List<string>();
            foreach (var item in enums)
            {
                var parsedEnum = Enum.Parse(typeof(TEnum), item.ToString());
                resultList.Add(EnumHelper.GetDisplayName((Enum)parsedEnum));
            }

            return resultList;
        }
    }
}
