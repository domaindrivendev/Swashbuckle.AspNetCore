using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiAnyFactory
    {
        public static JsonNode CreateFromJson(string json)
            => CreateFromJson(json, null);

        public static JsonNode CreateFromJson(string json, JsonSerializerOptions options)
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

        private static JsonArray CreateOpenApiArray(JsonElement jsonElement)
        {
            var openApiArray = new JsonArray();

            foreach (var item in jsonElement.EnumerateArray())
            {
                openApiArray.Add(CreateFromJsonElement(item));
            }

            return openApiArray;
        }

        private static JsonObject CreateOpenApiObject(JsonElement jsonElement)
        {
            var openApiObject = new JsonObject();

            foreach (var property in jsonElement.EnumerateObject())
            {
                openApiObject.Add(property.Name, CreateFromJsonElement(property.Value));
            }

            return openApiObject;
        }

        private static JsonNode CreateFromJsonElement(JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                return CreateOpenApiArray(jsonElement);
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                return CreateOpenApiObject(jsonElement);
            }

            return JsonValue.Create(jsonElement);
        }
    }
}
