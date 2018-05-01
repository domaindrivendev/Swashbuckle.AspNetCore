using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class SwaggerParameterAttributeOperationFilterTest
    {
        [Fact]
        public void Apply_AssignsDescriptionAndDoesNotOverrideRequired()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerParameterAndOptionalInputParameter));

            Subject().Apply(operation, filterContext);

            var param = Assert.Single(operation.Parameters);
            Assert.Equal("This is my input no requirement changes", param.Description);
            Assert.False(param.Required);
        }

        [Fact]
        public void Apply_AssignsDescriptionAndSetsRequired()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerParameterAndOptionalInputParameterMarkedAsRequired));

            Subject().Apply(operation, filterContext);

            var param = Assert.Single(operation.Parameters);
            Assert.Equal("This is my input, it is required", param.Description);
            Assert.True(param.Required);
        }

        [Fact]
        public void Apply_AssignsDescription()
        {
            var operation = new Operation { OperationId = "foobar" };
            var filterContext = FilterContextFor(nameof(TestController.ActionWithSwaggerParameterAndRequiredInputParameter));

            Subject().Apply(operation, filterContext);

            var param = Assert.Single(operation.Parameters);
            Assert.Equal("An Id for tests", param.Description);
        }

        private OperationFilterContext FilterContextFor(string fakeActionName)
        {
            var method = typeof(TestController).GetMethod(fakeActionName);
            var parameters = method.GetParameters()
                        .Select(paramInfo => new ControllerParameterDescriptor
                        {
                            Name = paramInfo.Name,
                            ParameterType = paramInfo.ParameterType,
                            ParameterInfo = paramInfo,
                            BindingInfo = BindingInfo.GetBindingInfo(paramInfo.GetCustomAttributes(false))
                        }).Cast<ParameterDescriptor>().ToList();

            var apiDescription = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(TestController).GetTypeInfo(),
                    MethodInfo = method,
                    Parameters = parameters
                }
            };

            return new OperationFilterContext(
                apiDescription,
                new SchemaRegistry(new JsonSerializerSettings()));
        }

        private SwaggerParameterAttributeOperationFilter Subject()
        {
            return new SwaggerParameterAttributeOperationFilter();
        }
    }
}
