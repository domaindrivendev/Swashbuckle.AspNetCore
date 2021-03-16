using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class TypeWithPropertiesSetViaConstructor
    {
        public TypeWithPropertiesSetViaConstructor(int id, string description)
        {
            Id = id;
            Description = description;
        }

        public int Id { get; }

        [JsonProperty("Desc")]
        public string Description { get; }
    }
}
