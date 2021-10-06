using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
            var operation = new OpenApiOperation();
            var methodInfo = typeof(FakeControllerWithSwaggerAnnotations)
                .GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithSwaggerOperationAttribute));
            var filterContext = new OperationFilterContext(
                apiDescription: null,
                schemaRegistry: null,
                schemaRepository: null,
                methodInfo: methodInfo);

            Subject().Apply(operation, filterContext);

            Assert.Equal("Summary for ActionWithSwaggerOperationAttribute", operation.Summary);
            Assert.Equal("Description for ActionWithSwaggerOperationAttribute", operation.Description);
            Assert.Equal("actionWithSwaggerOperationAttribute", operation.OperationId);
            Assert.Equal(new[] { "foobar" }, operation.Tags.Cast<OpenApiTag>().Select(t => t.Name));
        }

        [Fact]
        public void Apply_EnrichesResponseMetadata_IfActionDecoratedWithSwaggerResponseAttribute()
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    { "204", new OpenApiResponse { } },
                    { "400", new OpenApiResponse { } },
                }
            };
            var methodInfo = typeof(FakeControllerWithSwaggerAnnotations)
                .GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithSwaggerResponseAttributes));
            var filterContext = new OperationFilterContext(
                apiDescription: null,
                schemaRegistry: null,
                schemaRepository: null,
                methodInfo: methodInfo);

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "204", "400", "500" }, operation.Responses.Keys.ToArray());
            var response1 = operation.Responses["204"];
            Assert.Equal("Description for 204 response", response1.Description);
            var response2 = operation.Responses["400"];
            Assert.Equal("Description for 400 response", response2.Description);
            var response3 = operation.Responses["500"];
            Assert.Equal("Description for 500 response", response3.Description);
        }

        [Fact]
        public void Apply_EnrichesResponseMetadata_IfActionDecoratedWithSwaggerResponseContentTypesAttribute()
        {
            var operation = new OpenApiOperation
            {
                Responses = new OpenApiResponses
                {
                    { "200", new OpenApiResponse { } },
                }
            };
            var methodInfo = typeof(FakeControllerWithSwaggerAnnotations)
                .GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithSwaggerResponseContentTypesAttributes));
            var filterContext = new OperationFilterContext(
                apiDescription: null,
                schemaRegistry: new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions())),
                schemaRepository: new SchemaRepository(),
                methodInfo: methodInfo);

            Subject().Apply(operation, filterContext);

            Assert.Equal(new[] { "200", "500" }, operation.Responses.Keys.ToArray());
            var response1 = operation.Responses["200"];
            Assert.Equal("Description for 200 response", response1.Description);
            Assert.NotNull(response1.Content);
            Assert.Equal(2, response1.Content.Count);
            var jsonContent = response1.Content.First();
            var xmlContent = response1.Content.Last();
            Assert.Equal("application/json", jsonContent.Key);
            Assert.Equal("string", jsonContent.Value.Schema.Type);
            Assert.Equal("application/xml", xmlContent.Key);
            Assert.Equal("string", xmlContent.Value.Schema.Type);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfControllerDecoratedWithSwaggerOperationFilterAttribute()
        {
            var operation = new OpenApiOperation();
            var methodInfo = typeof(FakeControllerWithSwaggerAnnotations).GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithNoAttributes));
            var filterContext = new OperationFilterContext(
                apiDescription: null,
                schemaRegistry: null,
                schemaRepository: null,
                methodInfo: methodInfo);

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfActionDecoratedWithSwaggerOperationFilterAttribute()
        {
            var operation = new OpenApiOperation();
            var methodInfo = typeof(FakeControllerWithSwaggerAnnotations).GetMethod(nameof(FakeControllerWithSwaggerAnnotations.ActionWithSwaggerOperationFilterAttribute));
            var filterContext = new OperationFilterContext(
                apiDescription: null,
                schemaRegistry: null,
                schemaRepository: null,
                methodInfo: methodInfo);

            Subject().Apply(operation, filterContext);

            Assert.NotEmpty(operation.Extensions);
        }

        [Fact]
        public void Apply_EnrichesOperationMetadata_IfMinimalActionDecoratedWithSwaggerOperationAttribute()
        {
            var operationAttribute = new SwaggerOperationAttribute("Summary for ActionWithSwaggerOperationAttribute")
            {
                Description = "Description for ActionWithSwaggerOperationAttribute",
                OperationId = "actionWithSwaggerOperationAttribute",
                Tags = new[] { "foobar" }
            };

            var action = RequestDelegateFactory.Create((string parameter) => "{}");

            var operation = new OpenApiOperation();
            var methodInfo = action.RequestDelegate.Method;

            var apiDescription = new ApiDescription()
            {
                ActionDescriptor = new ActionDescriptor()
                {
                    EndpointMetadata = new[] { operationAttribute }
                }
            };

            var filterContext = new OperationFilterContext(
                apiDescription: apiDescription,
                schemaRegistry: null,
                schemaRepository: null,
                methodInfo: methodInfo);

            Subject().Apply(operation, filterContext);

            Assert.Equal("Summary for ActionWithSwaggerOperationAttribute", operation.Summary);
            Assert.Equal("Description for ActionWithSwaggerOperationAttribute", operation.Description);
            Assert.Equal("actionWithSwaggerOperationAttribute", operation.OperationId);
            Assert.Equal(new[] { "foobar" }, operation.Tags.Cast<OpenApiTag>().Select(t => t.Name));
        }

        private AnnotationsOperationFilter Subject()
        {
            return new AnnotationsOperationFilter();
        }
    }
}
