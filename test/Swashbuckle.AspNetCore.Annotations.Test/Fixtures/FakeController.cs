using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
    [SwaggerTag("description for TestController", "http://tempuri.org")]
    internal class TestController
    {
        public void ActionWithNoAttributes()
        { }

        [SwaggerOperation("summary for ActionWithSwaggerOperationAttribute",
            Description = "description for ActionWithSwaggerOperationAttribute",
            OperationId = "customOperationId",
            Tags = new[] { "customTag" }
        )]
        public void ActionWithSwaggerOperationAttribute()
        { }

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public void ActionWithSwaggerOperationFilterAttribute()
        { }

        [SwaggerResponse(204, "description for 204 response")]
        [SwaggerResponse(400, "description for 400 response", typeof(IDictionary<string, string>))]
        public IActionResult ActionWithSwaggerResponseAttributes()
        {
            throw new NotImplementedException();
        }

        public void ActionWithSwaggerParameterAttribute(
            [SwaggerParameter("description for param1", Required = true)]string param1)
        { }
        public void ActionWithSwaggerParameterAttributeDescriptionOnly(
            [SwaggerParameter("description for param1")]string param1)
        { }
    }
}
