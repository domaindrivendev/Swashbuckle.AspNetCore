using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MultipleDocuments
{
    internal class ApiExplorerGroupNameConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Use penultimate namespace as group / document name
            controller.ApiExplorer.GroupName = controller.ControllerType.Namespace.Split('.')
                .Reverse()
                .ElementAt(1)
                .ToLower();
        }
    }
}