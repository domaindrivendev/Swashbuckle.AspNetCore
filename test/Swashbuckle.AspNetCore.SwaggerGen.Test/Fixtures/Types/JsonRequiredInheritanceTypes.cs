using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public abstract class JsonRequiredRootType
    {
        [JsonProperty(Required = Required.Default)]
        public virtual string RootValDefault { get; set; }
    }
    [JsonObject(ItemRequired = Required.AllowNull)]
    public abstract class JsonRequiredBaseType : JsonRequiredRootType
    {
        public virtual string BaseValAllowNull { get; set; }
    }
    public class JsonRequiredChildType : JsonRequiredBaseType
    {
        // Required = Required.AllowNull from inherited JsonObjectAttribute
        public string ValAllowNull { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string ValDefault { get; set; }

        // Required = Required.Default from inherited JsonPropertyAttribute
        public override string RootValDefault { get; set; }
    }
    public class JsonRequiredAlwaysChildType : JsonRequiredBaseType
    {
        [JsonProperty(Required = Required.Always)]
        public override string BaseValAllowNull { get; set; }
        [JsonProperty(Required = Required.Always)]
        public override string RootValDefault { get; set; }
    }
}