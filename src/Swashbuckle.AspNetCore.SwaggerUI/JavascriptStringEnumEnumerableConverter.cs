#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

internal sealed class JavascriptStringEnumEnumerableConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>() :
    JsonConverterFactory
    where TEnum : struct, Enum
{
    private readonly JavascriptStringEnumConverter<TEnum> _enumConverter = new();

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsAssignableFrom(typeof(IEnumerable<TEnum>));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!typeToConvert.IsAssignableFrom(typeof(IEnumerable<TEnum>)))
        {
            throw new NotSupportedException($"The type {typeToConvert} is not supported.");
        }

        var valueConverter = (JsonConverter<TEnum>)_enumConverter.CreateConverter(typeof(TEnum), options);
        return new EnumEnumerableConverter(valueConverter);
    }

    private sealed class EnumEnumerableConverter(JsonConverter<TEnum> inner) : JsonConverter<IEnumerable<TEnum>>
    {
        public override IEnumerable<TEnum> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected start of a JSON array.");
            }

            var result = new List<TEnum>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return result;
                }

                result.Add(inner.Read(ref reader, typeof(TEnum), options));
            }

            throw new JsonException("JSON array is malformed.");
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<TEnum> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in value)
            {
                inner.Write(writer, item, options);
            }

            writer.WriteEndArray();
        }
    }
}
#endif
