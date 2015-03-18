using System;
using Microsoft.AspNet.Mvc.Description;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(
            SwaggerDocument swaggerDoc,
            SchemaGenerator schemaRegistry,
            ApiDescriptionGroupCollection apiDescriptionGroups)
        {
            swaggerDoc.vendorExtensions.Add("X-property1", "value");
        }
    }
}