using Swashbuckle.AspNetCore.TestSupport;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.SchemaGenerator;

[JsonSerializable(typeof(IntEnum))]
[JsonSerializable(typeof(LongEnum))]
public partial class CustomJsonSerializerContext : JsonSerializerContext
{
}
