using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
    [SwaggerTag("TestTag", "TestDescription")]
    internal class TestController
    {
        public void ActionWithNoAttributes()
        { }

        [SwaggerOperation("CustomOperationId", Tags = new[] { "customTag" }, Schemes = new[] { "customScheme" }, Produces = new string[] { "customType1", "customType2" }, Consumes = new string[] { "customType3", "customType4" })]
        public void ActionWithSwaggerOperationAttribute()
        { }

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public void ActionWithSwaggerOperationFilterAttribute()
        { }

        [SwaggerResponse(204, typeof(void), "No content is returned.")]
        [SwaggerResponse(400, typeof(IDictionary<string, string>), "This returns a dictionary.")]
        public IActionResult ActionWithSwaggerResponseAttributes()
        {
            throw new NotImplementedException();
        }
    }
}
