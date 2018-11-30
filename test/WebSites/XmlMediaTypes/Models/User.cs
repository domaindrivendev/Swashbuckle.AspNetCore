using System.Xml.Serialization;

namespace XmlMediaTypes.Models
{
    [XmlRootAttribute("User", Namespace = "http://example.com/schema")]
    public class User
    {
        public int Id { get; }

        [XmlElement(ElementName = "alias")]
        public string Username { get; set; }
    }
}
