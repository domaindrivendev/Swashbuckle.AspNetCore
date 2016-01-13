using System.Dynamic;
using Newtonsoft.Json;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    [JsonObject]
    public class DynamicObjectSubType : DynamicObject
    {
        public string Property1 { get; set; }
    }
}