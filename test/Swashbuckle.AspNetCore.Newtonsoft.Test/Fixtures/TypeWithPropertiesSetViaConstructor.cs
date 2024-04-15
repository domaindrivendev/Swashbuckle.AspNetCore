using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class TypeWithPropertiesSetViaConstructor
    {
        public TypeWithPropertiesSetViaConstructor(int id, string desc)
        {
            Id = id;
            Description = desc;
        }

        public int Id { get; }

        [JsonProperty("Desc")]
        public string Description { get; }
    }
}
