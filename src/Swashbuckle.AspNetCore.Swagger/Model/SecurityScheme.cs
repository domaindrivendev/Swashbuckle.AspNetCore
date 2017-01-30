using Newtonsoft.Json;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.Swagger
{
    public abstract class SecurityScheme
    {
        public SecurityScheme()
        {
            Extensions = new Dictionary<string, object>();
        }

        public string Type { get; set; }

        public string Description { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }
    }
}
