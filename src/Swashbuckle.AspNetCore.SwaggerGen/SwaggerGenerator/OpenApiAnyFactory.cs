#if !NET10_0_OR_GREATER
using System;
using System.Text.Json;
using Microsoft.OpenApi.Any;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static IOpenApiAny CreateFromJson(string json)
            => CreateFromJson(json, null);

        public static IOpenApiAny CreateFromJson(string json, JsonSerializerOptions options)
        {
            try
            {
                var element = JsonSerializer.Deserialize<JsonElement>(json, options);
                return CreateFromJsonElement(element);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static OpenApiArray CreateOpenApiArray(JsonElement jsonElement)
        {
            var openApiArray = new OpenApiArray();

            foreach (var item in jsonElement.EnumerateArray())
            {
                openApiArray.Add(CreateFromJsonElement(item));
            }

            return openApiArray;
        }

        private static OpenApiObject CreateOpenApiObject(JsonElement jsonElement)
        {
            var openApiObject = new OpenApiObject();

            foreach (var property in jsonElement.EnumerateObject())
            {
                openApiObject.Add(property.Name, CreateFromJsonElement(property.Value));
            }

            return openApiObject;
        }

        private static IOpenApiAny CreateFromJsonElement(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
                return new OpenApiNull();

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

            if (jsonElement.ValueKind == JsonValueKind.Array)
                return CreateOpenApiArray(jsonElement);

            if (jsonElement.ValueKind == JsonValueKind.Object)
                return CreateOpenApiObject(jsonElement);

            throw new ArgumentException($"Unsupported value kind {jsonElement.ValueKind}");
        }
    }
}
#endif
