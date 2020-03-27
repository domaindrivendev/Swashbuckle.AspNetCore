using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class NewtonsoftAnnotatedType
    {
        public string StringWithNoAnnotation { get; set; }

        [JsonIgnore]
        public string StringWithJsonIgnore { get; set; }

        [JsonProperty("string-with-json-property-name")]
        public string StringWithJsonPropertyName { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NewtonsoftAnnotatedEnum
    {
        Value1 = 2,
        Value2 = 4,
        X = 8
    }
}
