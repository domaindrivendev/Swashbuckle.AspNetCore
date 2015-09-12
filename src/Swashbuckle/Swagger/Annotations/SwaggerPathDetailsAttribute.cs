using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Swashbuckle.Swagger.Annotations {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerPathDetailsAttribute : Attribute {

        public SwaggerPathDetailsAttribute() { }//to allow someone to set properties directly
        public SwaggerPathDetailsAttribute(string summary, string implementationNotes= null) {
            Summary = summary;
            ImplementationNotes = implementationNotes;
        }
        public string Summary { get; set; }
        public string ImplementationNotes { get; set; }
    }
}
