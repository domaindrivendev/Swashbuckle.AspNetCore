using System;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace XmlMediaTypes.Swagger
{
    public class XmlSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var xmlRootAttribute = context.Type.GetCustomAttributes(false)
                .OfType<XmlRootAttribute>()
                .FirstOrDefault();

            if (xmlRootAttribute != null)
            {
                schema.Xml = new OpenApiXml
                {
                    Namespace = new Uri(xmlRootAttribute.Namespace)
                };
            }

            if (context.MemberInfo != null)
            {
                var xmlElementAttribute = context.MemberInfo.GetCustomAttributes(false)
                    .OfType<XmlElementAttribute>()
                    .FirstOrDefault();

                schema.Xml = new OpenApiXml
                {
                    Name = xmlElementAttribute?.ElementName ?? context.MemberInfo.Name
                };
            }
        }
    }
}
