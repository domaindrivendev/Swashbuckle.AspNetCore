using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonExtensionDataAnnotatedType
    {
        public bool Property1 { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; }
    }
}