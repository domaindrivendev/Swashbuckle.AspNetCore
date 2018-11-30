using Newtonsoft.Json;

namespace NewtonsoftSupport.Models
{
    public class User
    {
        public int Id { get; }

        [JsonProperty("alias", Required = Required.Always)]
        public string Username { get; set; }

        public UserStatus Status { get; set; }
    }
}
