using System;
using Microsoft.AspNet.Mvc.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.Test.Fixtures.Extensions
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