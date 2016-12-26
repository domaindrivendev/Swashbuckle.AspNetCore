using System.Dynamic;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [JsonObject]
    public class DynamicObjectSubType : DynamicObject
    {
        public string Property1 { get; set; }
    }
}