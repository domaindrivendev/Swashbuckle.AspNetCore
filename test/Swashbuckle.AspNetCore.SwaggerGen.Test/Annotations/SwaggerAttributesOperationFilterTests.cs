using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SwaggerAttributesOperationFilterTests
    {
        [Fact]
        public void Apply_AssignsProperties_FromActionAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor<FakeController>(
                nameof(FakeController.AnnotatedWithSwaggerOperation));

            Subject().Apply(operation, filterContext);

            Assert.Equal("CustomOperationId", operation.OperationId);
            Assert.Equal(new[] { "customTag" }, operation.Tags.ToArray());
            Assert.Equal(new[] { "customScheme" }, operation.Schemes.ToArray());
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerAnnotatedWithFilterAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor<SwaggerAnnotatedController>(
                nameof(SwaggerAnnotatedController.ReturnsVoid));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfActionAnnotatedWithFilterAttribute()
        {
            var operation = new Operation
            {
                OperationId = "foobar" 
            };
            var filterContext = FilterContextFor<FakeController>(
                nameof(FakeController.AnnotatedWithSwaggerOperationFilter));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        private OperationFilterContext FilterContextFor<TController>(string actionName)
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionName, typeof(TController))
                .ApiDescriptionGroups.Items.First()
                .Items.First();


            return new OperationFilterContext(
                apiDescription,
                new SchemaRegistry(new JsonSerializerSettings()),
                (apiDescription.ActionDescriptor as ControllerActionDescriptor).MethodInfo);
        }

        private SwaggerAttributesOperationFilter Subject()
        {
            return new SwaggerAttributesOperationFilter();
        }
    }
}