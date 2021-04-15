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

                if (jsonElement.ValueKind == JsonValueKind.Array)
                    return CreateOpenApiArray(jsonElement);

                if (jsonElement.ValueKind == JsonValueKind.Object)
                    return CreateOpenApiObject(jsonElement);
            }
            catch { }

            return null;
        }

        private static IOpenApiAny CreateOpenApiArray(JsonElement jsonElement)
        {
            var openApiArray = new OpenApiArray();

            foreach (var item in jsonElement.EnumerateArray())
            {
                string json;
                switch (item.ValueKind)
                {
                    case JsonValueKind.String:
                        json = $"\"{item}\"";
                        break;
                    case JsonValueKind.True:
                        json = "true";
                        break;
                    case JsonValueKind.False:
                        json = "false";
                        break;
                    default:
                        json = item.ToString();
                        break;
                }

                openApiArray.Add(CreateFromJson(json));
            }

            return openApiArray;
        }

        private static IOpenApiAny CreateOpenApiObject(JsonElement jsonElement)
        {
            var openApiObject = new OpenApiObject();

            foreach (var property in jsonElement.EnumerateObject())
            {
                string valueAsJson;
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        valueAsJson = $"\"{property.Value}\"";
                        break;
                    case JsonValueKind.True:
                        valueAsJson = "true";
                        break;
                    case JsonValueKind.False:
                        valueAsJson = "false";
                        break;
                    default:
                        valueAsJson = property.Value.ToString();
                        break;
                }

                openApiObject.Add(property.Name, CreateFromJson(valueAsJson));
            }

            return openApiObject;
        }
    }
}
