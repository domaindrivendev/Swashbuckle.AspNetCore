using System.Collections.Generic;
using System.Xml.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlSerializerAnnotatedType
    {
        [XmlAttribute]
        public string AttributeButNotRenamed { get; set; }

        [XmlAttribute("AttributeAndRenamed")]
        public string Property1 { get; set; }

        public string ImplicitElement { get; set; }

        [XmlElement]
        public string ExplicitElementButNotRenamed { get; set; }

        [XmlElement("ExplicitElementAndRenamed")]
        public string Property2 { get; set; }

        [XmlElement(Namespace = "http://mynamespace.com")]
        public string NamespacedElement { get; set; }

        [XmlArray(ElementName = "ChildPrimatives", Namespace = "http://mynamespace.com/childprimativelist")]
        [XmlArrayItem(ElementName = "ChildPrimative", Namespace = "http://mynamespace.com/childprimative")]
        public List<string> ListOfPrimatives { get; set; }

        [XmlArray(ElementName = "ChildObjects", Namespace = "http://mynamespace.com/childobjectlist")]
        [XmlArrayItem(ElementName = "ChildObject", Namespace = "http://mynamespace.com/childobject")]
        public List<ChildXmlSerializerAnnotatedType> ListOfChildObjects { get; set; }
    }

    public class ChildXmlSerializerAnnotatedType
    {
        public string Property1 { get; set; }
    }
}