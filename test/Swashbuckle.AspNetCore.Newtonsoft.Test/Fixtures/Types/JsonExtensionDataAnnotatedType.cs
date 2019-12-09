using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class JsonExtensionDataAnnotatedType
    {
        public bool Property1 { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; }
    }
}