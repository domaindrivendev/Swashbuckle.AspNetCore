using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TypeWithPropertiesSetViaConstructor
    {
        public TypeWithPropertiesSetViaConstructor(int id, string desc)
        {
            Id = id;
            Description = desc;
        }

        public int Id { get; }

        [JsonPropertyName("Desc")]
        public string Description { get; }
    }
}
