using Microsoft.AspNetCore.Mvc.Abstractions;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Swashbuckle.AspNetCore.TestSupport;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.ApiDescriptionExtensions
{
    public class ApiDescriptionExtensionsTests
    {
        [Fact]
        public void TryGetMethodInfo_GetsMethodInfo_IfControllerActionDescriptor()
        {
            var apiDescription = ApiDescriptionFactory.Create<FakeController>(c => nameof(c.ActionWithNoParameters), groupName: "v1", httpMethod: "POST", relativePath: "/");

            var result = apiDescription.TryGetMethodInfo(out var methodInfo);

            Assert.True(result);
            Assert.NotNull(methodInfo);
        }

        [Fact]
        public void TryGetMethodInfo_GetsMethodInfo_IfEndpointActionDescriptor()
        {
            var testMethodInfo = typeof(TestMinimalApiMethod).GetMethod("RequestDelegate");

            var actionDescriptor = new ActionDescriptor();
            actionDescriptor.EndpointMetadata = new List<object> { testMethodInfo };
            actionDescriptor.Parameters = testMethodInfo
                .GetParameters()
                .Select(p => new ParameterDescriptor
                {
                    Name = p.Name,
                    ParameterType = p.ParameterType
                })
                .ToList();

            var apiDescription = ApiDescriptionFactory.Create(actionDescriptor, testMethodInfo, groupName: "v1", httpMethod: "POST", relativePath: "/");

            var result = apiDescription.TryGetMethodInfo(out var methodInfo);

            Assert.True(result);
            Assert.NotNull(methodInfo);
        }
    }
}