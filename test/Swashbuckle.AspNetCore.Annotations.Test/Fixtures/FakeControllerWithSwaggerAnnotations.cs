using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
    [SwaggerResponse(500, "Description for 500 response", typeof(IDictionary<string, string>))]
    [SwaggerTag("Description for FakeControllerWithSwaggerAnnotations", "http://tempuri.org")]
    internal class FakeControllerWithSwaggerAnnotations
    {
        [SwaggerOperation("Summary for ActionWithSwaggerOperationAttribute",
            Description = "Description for ActionWithSwaggerOperationAttribute",
            OperationId = "actionWithSwaggerOperationAttribute",
            Tags = new[] { "foobar" }
        )]
        public void ActionWithSwaggerOperationAttribute()
        { }

        public void ActionWithSwaggerParameterAttribute(
            [SwaggerParameter("Description for param", Required = true)]string param)
        { }

        public void ActionWithSwaggerParameterAttributeDescriptionOnly(
            [SwaggerParameter("Description for param")]string param)
        { }

        public void ActionWithSwaggerSchemaAttribute(
            [SwaggerSchema("Description for param", Format = "date")]string param)
        { }

        public void ActionWithSwaggerRequestBodyAttribute(
            [SwaggerRequestBody("Description for param", Required = true)]string param)
        { }

        public void ActionWithSwaggerRequestbodyAttributeDescriptionOnly(
            [SwaggerRequestBody("Description for param")]string param)
        { }

        [SwaggerResponse(204, "Description for 204 response")]
        [SwaggerResponse(400, "Description for 400 response", typeof(IDictionary<string, string>))]
        public IActionResult ActionWithSwaggerResponseAttributes()
        {
            throw new NotImplementedException();
        }

        public void ActionWithNoAttributes()
        { }

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public void ActionWithSwaggerOperationFilterAttribute()
        { }
    }
}
