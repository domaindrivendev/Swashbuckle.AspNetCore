using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    public class ProducesErrorResponseTypeOperationFilter : IOperationFilter
    {
        void IOperationFilter.Apply(Operation operation, OperationFilterContext context)
        {
            var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            var customDescriptionAttributes = controllerActionDescriptor?.MethodInfo.GetCustomAttributes<ProducesErrorResponseTypeAttribute>();

            foreach (var producesErrorResponseAttribute in customDescriptionAttributes)
            {
                var key = $"{producesErrorResponseAttribute.StatusCode}";

                if (operation.Responses.ContainsKey(key) && producesErrorResponseAttribute.Description != null)
                {
                    operation.Responses[key].Description = producesErrorResponseAttribute.Description;
                }
            }
        }
    }
}