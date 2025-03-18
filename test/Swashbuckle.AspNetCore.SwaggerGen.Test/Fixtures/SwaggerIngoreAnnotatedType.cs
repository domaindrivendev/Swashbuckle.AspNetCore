using System.Text.Json;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures
{
    public class SwaggerIngoreAnnotatedType
    {
        public string NotIgnoredString { get; set; }

        [SwaggerIgnore]
        public string IgnoredString { get; set; }

        [SwaggerIgnore]
        [JsonExtensionData]
        public IDictionary<string, JsonElement> IgnoredExtensionData { get; set; }
    }
}
