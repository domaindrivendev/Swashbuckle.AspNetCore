using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.OpenApi.Any;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static IOpenApiAny CreateFromJson(string json)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

                if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                    return new OpenApiBoolean(jsonElement.GetBoolean());

                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    if (jsonElement.TryGetInt32(out int intValue))
                        return new OpenApiInteger(intValue);

                    if (jsonElement.TryGetInt64(out long longValue))
                        return new OpenApiLong(longValue);

                    if (jsonElement.TryGetSingle(out float floatValue) && !float.IsInfinity(floatValue))
                        return new OpenApiFloat(floatValue);

                    if (jsonElement.TryGetDouble(out double doubleValue))
                        return new OpenApiDouble(doubleValue);
                }

                if (jsonElement.ValueKind == JsonValueKind.String)
                    return new OpenApiString(jsonElement.ToString());

                if (jsonElement.ValueKind == JsonValueKind.Null)
                    return new OpenApiNull();

                if (jsonElement.ValueKind == JsonValueKind.Array)
                    return CreateOpenApiArray(jsonElement.EnumerateArray());

            }
            catch { }

            return null;
        }

        private static IOpenApiAny CreateOpenApiArray(IEnumerable<JsonElement> jsonElements)
        {
            var openApiArray = new OpenApiArray();

            foreach (var jsonElement in jsonElements)
            {
                var json = jsonElement.ValueKind == JsonValueKind.String
                    ? $"\"{jsonElement}\""
                    : jsonElement.ToString();

                openApiArray.Add(CreateFromJson(json));
            }

            return openApiArray;
        }
    }
}
