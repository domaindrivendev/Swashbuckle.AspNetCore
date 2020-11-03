using System;
using System.Linq;
using System.Text.Json;

using Xunit;

using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SystemTextJsonBehaviorTests
    {
        [Theory]
        [InlineData(typeof(IBaseInterface), new[] { "BaseProperty" })]
        [InlineData(typeof(ISubInterface1), new[] { "BaseProperty", "Property1" })]
        [InlineData(typeof(ISubInterface2), new[] { "BaseProperty", "Property2" })]
        [InlineData(typeof(IMultiSubInterface), new[] { "BaseProperty", "Property1", "Property2", "Property3" })]
        public void GetDataContractForType_CreatedContracr_AllInterfaceMembersIncluded(Type type, string[] propertyNames)
        {
            var behavior = Subject();

            var contract = behavior.GetDataContractForType(type);

            var contractPropertiesNames = contract.ObjectProperties
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToList();
            Assert.Equal(propertyNames, contractPropertiesNames);
        }

        private SystemTextJsonBehavior Subject(Action<JsonSerializerOptions> configureSerializer = null)
        {
            var serializerOptions = new JsonSerializerOptions();
            configureSerializer?.Invoke(serializerOptions);

            return new SystemTextJsonBehavior(serializerOptions);
        }
    }
}
