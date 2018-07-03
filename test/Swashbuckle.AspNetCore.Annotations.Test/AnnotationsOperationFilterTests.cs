using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class AnnotationsOperationFilterTests
    {
        [Fact]
        public void Apply_EnrichesOperationMetadata_IfActionDecoratedWithSwaggerOperationAttribute()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerOperationAttribute));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for ActionWithSwaggerOperationAttribute", operation.Summary);
            Assert.Equal("description for ActionWithSwaggerOperationAttribute", operation.Description);
            Assert.Equal("customOperationId", operation.OperationId);
            Assert.Equal(new[] { "customTag" }, operation.Tags.ToArray());
            Assert.Equal(new[] { "customMimeType1" }, operation.Consumes.ToArray());
            Assert.Equal(new[] { "customMimeType2" }, operation.Produces.ToArray());
            Assert.Equal(new[] { "https" }, operation.Schemes.ToArray());
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerDecoratedWithSwaggerOperationFilterAttribute()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithNoAttributes));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfActionDecoratedWithSwaggerOperationFilterAttribute()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerOperationFilterAttribute));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_EnrichesResponseMetadata_IfActionDecoratedWithSwaggerResponseAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar",
                Responses = new Dictionary<string, Response>()
                {
                    { "204", new Response { } },
                    { "400", new Response { } },
                }
            };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerResponseAttributes));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "204", "400" }, operation.Responses.Keys.ToArray());
            var response1 = operation.Responses["204"];
            Assert.Equal("description for 204 response", response1.Description);
            var response2 = operation.Responses["400"];
            Assert.Equal("description for 400 response", response2.Description);
        }

        private OperationFilterContext FilterContextFor(string fakeActionName)
        {
            var apiDescription = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(TestController).GetTypeInfo(),
                    MethodInfo = typeof(TestController).GetMethod(fakeActionName)
                }
            };

            return new OperationFilterContext(
                apiDescription,
                new SchemaRegistry(new JsonSerializerSettings()),
                (apiDescription.ActionDescriptor as ControllerActionDescriptor).MethodInfo);
        }

        private AnnotationsOperationFilter Subject()
        {
            return new AnnotationsOperationFilter();
        }
    }
}