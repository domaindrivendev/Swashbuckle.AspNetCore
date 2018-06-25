using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class SwaggerOperationAttributeFilterTests
    {
        [Fact]
        public void Apply_AssignsProperties_FromActionAttribute()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerOperationAttribute));

            Subject().Apply(operation, filterContext);

            Assert.Equal("CustomOperationId", operation.OperationId);
            Assert.Equal(new[] { "customTag" }, operation.Tags.ToArray());
            Assert.Equal(new[] { "customScheme" }, operation.Schemes.ToArray());
            Assert.Equal(new[] { "customType1", "customType2" }, operation.Produces.ToArray());
            Assert.Equal(new[] { "customType3", "customType4" }, operation.Consumes.ToArray());
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerAnnotatedWithFilterAttribute()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithNoAttributes));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfActionAnnotatedWithFilterAttribute()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerOperationFilterAttribute));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
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

        private SwaggerOperationAttributeFilter Subject()
        {
            return new SwaggerOperationAttributeFilter();
        }
    }
}