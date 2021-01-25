using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class JsonRequiredAnnotatedType
    {
        [JsonRequired]
        public string StringWithJsonRequired { get; set; }
    }
}