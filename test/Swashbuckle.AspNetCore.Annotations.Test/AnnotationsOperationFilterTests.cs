using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class AnnotationsOperationFilterTests
    {
        [Fact]
        public void Apply_EnrichesOperationMetadata_IfActionDecoratedWithSwaggerOperationAttribute()
        {
            var operation = new OpenApiOperation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerOperationAttribute));

            Subject().Apply(operation, filterContext);

            Assert.Equal("summary for ActionWithSwaggerOperationAttribute", operation.Summary);
            Assert.Equal("description for ActionWithSwaggerOperationAttribute", operation.Description);
            Assert.Equal("customOperationId", operation.OperationId);
            Assert.Equal(new[] { "customTag" }, operation.Tags.Cast<OpenApiTag>().Select(t => t.Name));
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerDecoratedWithSwaggerOperationFilterAttribute()
        {
            var operation = new OpenApiOperation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithNoAttributes));

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfActionDecoratedWithSwaggerOperationFilterAttributeInBaseClass()
        {
            var operation = new OpenApiOperation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestControllerBase.ActionWithNoAttributesInBaseClass));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "400" }, operation.Responses.Keys.ToArray());
            var response = operation.Responses["400"];
            Assert.Equal("description for 400 response at base controller", response.Description);
        }

        [Fact]
        public void Apply_EnrichesResponseMetadata_IfActionDecoratedWithSwaggerResponseAttribute()
        {
            var operation = new OpenApiOperation
            {
                OperationId = "foobar",
                Responses = new OpenApiResponses
                {
                    { "204", new OpenApiResponse { } },
                    { "400", new OpenApiResponse { } },
                }
            };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerResponseAttributes));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "204", "400" }, operation.Responses.Keys.ToArray());
            var response1 = operation.Responses["204"];
            Assert.Equal("description for 204 response", response1.Description);
            var response2 = operation.Responses["400"];
            Assert.Equal("description for 400 response at action", response2.Description);
        }

        [Fact]
        public void Apply_EnrichesResponseMetadata_IfActionAndControllerNotDecoratedWithSwaggerResponseAttribute()
        {
            var operation = new OpenApiOperation
            {
                OperationId = "foobar",
                Responses = new OpenApiResponses()
                {
                    { "400", new OpenApiResponse { } },
                }
            };
            var filterContext = FilterContextFor(nameof(TestController2.ActionWithNoAttributesInBaseClass));

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "400" }, operation.Responses.Keys.ToArray());
            var response = operation.Responses["400"];
            Assert.Equal("description for 400 response at base controller", response.Description);
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

            var generatorOptions = new SchemaGeneratorOptions();

            return new OperationFilterContext(
                apiDescription,
                new JsonSchemaGenerator(generatorOptions, new JsonSerializerOptions()),
                new SchemaRepository(),
                (apiDescription.ActionDescriptor as ControllerActionDescriptor).MethodInfo);
        }

        private AnnotationsOperationFilter Subject()
        {
            return new AnnotationsOperationFilter();
        }
    }
}
