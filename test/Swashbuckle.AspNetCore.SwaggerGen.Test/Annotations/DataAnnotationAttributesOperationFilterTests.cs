using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Annotations
{
    public class DataAnnotationAttributesOperationFilterTests
    {
        [Fact]
        public void Apply_AssignsProperties_FromActionAttribute()
        {
            var parameterNames = new[] { "p1", "p2", "p3", "p4", "p5", "p6", "p7" };
            var operation = new Operation
            {
                Parameters = parameterNames.Select(pn => new NonBodyParameter { Name = pn }).ToArray(),
            };
            var filterContext = FilterContextFor(nameof(FakeActions.AnnotatedWithDataAnnotationsOperation));

            Subject().Apply(operation, filterContext);

            var p1 = operation.Parameters[0] as NonBodyParameter;
            Assert.True(p1.Required);

            var p2 = operation.Parameters[1] as NonBodyParameter;
            Assert.False(p2.Required);

            var p3 = operation.Parameters[2] as NonBodyParameter;
            Assert.Equal("[0-9]*", p3.Pattern);

            var p4 = operation.Parameters[3] as NonBodyParameter;
            Assert.Equal(100, p4.Minimum);
            Assert.Equal(1000, p4.Maximum);
            Assert.Equal(200, p4.Default);

            var p5 = operation.Parameters[4] as NonBodyParameter;
            Assert.Equal(10, p5.MinLength);

            var p6 = operation.Parameters[5] as NonBodyParameter;
            Assert.Equal(100, p6.MaxLength);

            var p7 = operation.Parameters[6] as NonBodyParameter;
            Assert.Equal(25, p7.MinLength);
            Assert.Equal(50, p7.MaxLength);
        }

        private OperationFilterContext FilterContextFor(string actionFixtureName)
        {
            var fakeProvider = new FakeApiDescriptionGroupCollectionProvider();
            var apiDescription = fakeProvider
                .Add("GET", "collection", actionFixtureName)
                .ApiDescriptionGroups.Items.First()
                .Items.First();

            return new OperationFilterContext(apiDescription, null);
        }

        private DataAnnotationAttributesOperationFilter Subject()
        {
            return new DataAnnotationAttributesOperationFilter();
        }
    }
}
