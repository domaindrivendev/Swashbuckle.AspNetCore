#if NET10_0_OR_GREATER
using Microsoft.OpenApi.Models;
#endif

namespace Swashbuckle.AspNetCore;

internal static class JsonSchemaTypes
{
#if NET10_0_OR_GREATER
    public static readonly JsonSchemaType Array = JsonSchemaType.Array;
    public static readonly JsonSchemaType Boolean = JsonSchemaType.Boolean;
    public static readonly JsonSchemaType Integer = JsonSchemaType.Integer;
    public static readonly JsonSchemaType Number = JsonSchemaType.Number;
    public static readonly JsonSchemaType Null = JsonSchemaType.Null;
    public static readonly JsonSchemaType Object = JsonSchemaType.Object;
    public static readonly JsonSchemaType String = JsonSchemaType.String;
#else
    public const string Array = "array";
    public const string Boolean = "boolean";
    public const string Integer = "integer";
    public const string Number = "number";
    public const string Null = "null";
    public const string Object = "object";
    public const string String = "string";
#endif
}
