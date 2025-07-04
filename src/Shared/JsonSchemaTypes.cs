using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore;

internal static class JsonSchemaTypes
{
    public static readonly JsonSchemaType Array = JsonSchemaType.Array;
    public static readonly JsonSchemaType Boolean = JsonSchemaType.Boolean;
    public static readonly JsonSchemaType Integer = JsonSchemaType.Integer;
    public static readonly JsonSchemaType Number = JsonSchemaType.Number;
    public static readonly JsonSchemaType Null = JsonSchemaType.Null;
    public static readonly JsonSchemaType Object = JsonSchemaType.Object;
    public static readonly JsonSchemaType String = JsonSchemaType.String;
}
