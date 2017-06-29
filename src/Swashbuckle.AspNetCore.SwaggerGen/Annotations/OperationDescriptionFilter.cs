using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    public class OperationDescriptionFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            var customDescriptionAttributes = controllerActionDescriptor?.MethodInfo.GetCustomAttributes<DescriptionAttribute>();
            operation.Description = customDescriptionAttributes.Select(z => z.Description).FirstOrDefault();
        }
    }
}