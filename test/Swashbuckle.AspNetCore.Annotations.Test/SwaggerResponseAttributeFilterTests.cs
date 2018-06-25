using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class SwaggerResponseAttributeFilterTests
    {
        [Fact]
        public void Apply_SetsResponses_FromAttributes()
        {
            var operation = new Operation
            {
                OperationId = "foobar"
            };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerResponseAttributes));

            Subject().Apply(operation, filterContext);

            var responses = operation.Responses;
            Assert.Equal(new[] { "204", "400" }, responses.Keys.ToArray());
            var response1 = responses["204"];
            Assert.Equal("No content is returned.", response1.Description);
            Assert.Null(response1.Schema);
            var response2 = responses["400"];
            Assert.Equal("This returns a dictionary.", response2.Description);
            Assert.NotNull(response2.Schema);
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

        private SwaggerResponseAttributeFilter Subject()
        {
            return new SwaggerResponseAttributeFilter();
        }
    }
}
