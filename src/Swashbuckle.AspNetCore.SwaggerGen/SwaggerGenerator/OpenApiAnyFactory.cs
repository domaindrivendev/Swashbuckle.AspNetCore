using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static IOpenApiAny CreateFor(OpenApiSchema schema, object value)
        {
            if (value == null) return null;

            if (schema.Type == "integer" && schema.Format == "int64" && TryCast(value, out long longValue))
                return new OpenApiLong(longValue);

            else if (schema.Type == "integer" && TryCast(value, out int intValue))
                return new OpenApiInteger(intValue);

            else if (schema.Type == "number" && schema.Format == "double" && TryCast(value, out double doubleValue))
                return new OpenApiDouble(doubleValue);

            else if (schema.Type == "number" && TryCast(value, out float floatValue))
                return new OpenApiFloat(floatValue);

            if (schema.Type == "boolean" && TryCast(value, out bool boolValue))
                return new OpenApiBoolean(boolValue);

            else if (schema.Type == "string" && schema.Format == "date" && TryCast(value, out DateTime dateValue))
                return new OpenApiDate(dateValue);

            else if (schema.Type == "string" && schema.Format == "date-time" && TryCast(value, out DateTime dateTimeValue))
                return new OpenApiDate(dateTimeValue);

            else if (schema.Type == "string" && value.GetType().IsEnum)
                return new OpenApiString(Enum.GetName(value.GetType(), value));

            else if (schema.Type == "string")
                return new OpenApiString(value.ToString());

            else if (schema.Type == "array" && schema.Format == "string" && value.ToString().StartsWith("[") && value.ToString().EndsWith("]"))
                return ConvertAndParseArrayItems(value);

            return null;
        }

        private static bool TryCast<T>(object value, out T typedValue)
        {
            try
            {
                typedValue = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }
            catch (InvalidCastException)
            {
                typedValue = default(T);
                return false;
            }
        }

        private static IOpenApiAny ConvertAndParseArrayItems(object value)
        {
            var array = new OpenApiArray();
            var example = ((string)value).Replace("[", string.Empty).Replace("]", string.Empty);

            string[] split;

            if (example.Contains("'"))
            {
                split = example.Split(new string[] { "'" }, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<string>(split);

                while (list.Contains(","))
                {
                    list.Remove(",");
                }

                split = list.ToArray();
            }
            else
            {
                split = example.Split(',');
            }

            foreach (var item in split)
            {
                if (int.TryParse(item, out int intResult))
                {
                    array.Add(item: new OpenApiInteger(intResult));
                }
                else if (Boolean.TryParse(item, out bool boolResult))
                {
                    array.Add(item: new OpenApiBoolean(boolResult));
                }
                else if (long.TryParse(item, out long longResult))
                {
                    array.Add(item: new OpenApiLong(longResult));
                }
                else if (double.TryParse(item, out double doubleResult))
                {
                    array.Add(item: new OpenApiDouble(doubleResult));
                }
                else if (DateTime.TryParse(item, out DateTime dateTimeResult))
                {
                    //TODO: What is the difference between OpenApiDate and OpenApiDateTime?
                    array.Add(item: new OpenApiDateTime(dateTimeResult));
                }
                else if (item.StartsWith("'") && item.EndsWith("'"))
                {
                    var clean = item.Replace("'", string.Empty).Replace("'", string.Empty);
                    array.Add(item: new OpenApiString(clean));
                }
                else
                {
                    array.Add(item: new OpenApiString(item));
                }
            }

            return array;
        }
    }
}
