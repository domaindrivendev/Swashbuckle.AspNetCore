using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Swashbuckle.IntegrationTests
{
    internal class TestAppActionModel : IActionModelConvention
    {
        private readonly Assembly _testAppAssembly;

        public TestAppActionModel(Assembly testAppAssembly)
        {
            _testAppAssembly = testAppAssembly;
        }

        public void Apply(ActionModel action)
        {
            action.ApiExplorer.IsVisible = (action.Controller.ControllerType.Assembly == _testAppAssembly);
        }
    }
}