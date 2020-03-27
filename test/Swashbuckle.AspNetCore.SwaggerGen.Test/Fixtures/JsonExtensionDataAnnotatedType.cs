using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class JsonExtensionDataAnnotatedType
    {
        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; }
    }
}
