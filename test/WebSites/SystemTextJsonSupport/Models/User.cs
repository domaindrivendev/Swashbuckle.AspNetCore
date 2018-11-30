using System.Text.Json.Serialization;

namespace SystemTextJsonSupport.Models
{
    public class User
    {
        public int Id { get; }

        [JsonPropertyName("alias")]
        public string Username { get; set; }

        public UserStatus Status { get; set; }
    }
}
