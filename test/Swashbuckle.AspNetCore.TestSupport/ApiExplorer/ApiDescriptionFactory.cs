using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public static class ApiDescriptionFactory
    {
        public static ApiDescription Create(
            ActionDescriptor actionDescriptor,
            MethodInfo methodInfo,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resoure",
            IEnumerable<ApiParameterDescription> parameterDescriptions = null,
            IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
            IEnumerable<ApiResponseType> supportedResponseTypes = null)
        {
            var apiDescription = new ApiDescription
            {
                ActionDescriptor = actionDescriptor,
                GroupName = groupName,
                HttpMethod = httpMethod,
                RelativePath = relativePath,
            };

            if (parameterDescriptions != null)
            {
                foreach (var parameter in parameterDescriptions)
                {
                    // If the provided action has a matching parameter - use it to assign ParameterDescriptor & ModelMetadata
                    var controllerParameterDescriptor = actionDescriptor.Parameters
                        .OfType<ControllerParameterDescriptor>()
                        .FirstOrDefault(parameterDescriptor => parameterDescriptor.Name == parameter.Name);

                    if (controllerParameterDescriptor != null)
                    {
                        parameter.ParameterDescriptor = controllerParameterDescriptor;
                        parameter.ModelMetadata = ModelMetadataFactory.CreateForParameter(controllerParameterDescriptor.ParameterInfo);
                    }

                    apiDescription.ParameterDescriptions.Add(parameter);
                }
            }

            if (supportedRequestFormats != null)
            {
                foreach (var requestFormat in supportedRequestFormats)
                {
                    apiDescription.SupportedRequestFormats.Add(requestFormat);
                }
            }

            if (supportedResponseTypes != null)
            {
                foreach (var responseType in supportedResponseTypes)
                {
                    // If the provided action has a return value AND the response status is 2XX - use it to assign ModelMetadata
                    if (methodInfo.ReturnType != null && responseType.StatusCode/100 == 2)
                    {
                        responseType.ModelMetadata = ModelMetadataFactory.CreateForType(methodInfo.ReturnType);
                    }

                    apiDescription.SupportedResponseTypes.Add(responseType);
                }
            }

            return apiDescription;
        }

        public static ApiDescription Create(
            MethodInfo methodInfo,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resoure",
            IEnumerable<ApiParameterDescription> parameterDescriptions = null,
            IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
            IEnumerable<ApiResponseType> supportedResponseTypes = null)
        {

            var actionDescriptor = CreateActionDescriptor(methodInfo);

            return Create(
                actionDescriptor,
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedRequestFormats,
                supportedResponseTypes
            );
        }

        public static ApiDescription Create<TController>(
            Func<TController, string> actionNameSelector,
            string groupName = "v1",
            string httpMethod = "POST",
            string relativePath = "resoure",
            IEnumerable<ApiParameterDescription> parameterDescriptions = null,
            IEnumerable<ApiRequestFormat> supportedRequestFormats = null,
            IEnumerable<ApiResponseType> supportedResponseTypes = null)
            where TController : new()
        {
            var methodInfo = typeof(TController).GetMethod(actionNameSelector(new TController()));

            return Create(
                methodInfo,
                groupName,
                httpMethod,
                relativePath,
                parameterDescriptions,
                supportedRequestFormats,
                supportedResponseTypes
            );
        }

        private static ActionDescriptor CreateActionDescriptor(MethodInfo methodInfo)
        {
            var httpMethodAttribute = methodInfo.GetCustomAttribute<HttpMethodAttribute>();
            var attributeRouteInfo = (httpMethodAttribute != null)
                ? new AttributeRouteInfo { Template = httpMethodAttribute.Template, Name = httpMethodAttribute.Name }
                : null;

            var parameterDescriptors = methodInfo.GetParameters()
                .Select(CreateParameterDescriptor)
                .ToList();

            var routeValues = new Dictionary<string, string>
            {
                ["controller"] = methodInfo.DeclaringType.Name.Replace("Controller", string.Empty)
            };

            return new ControllerActionDescriptor
            {
                AttributeRouteInfo = attributeRouteInfo,
                ControllerTypeInfo = methodInfo.DeclaringType.GetTypeInfo(),
                ControllerName = methodInfo.DeclaringType.Name,
                MethodInfo = methodInfo,
                Parameters = parameterDescriptors,
                RouteValues = routeValues

            };
        }

        private static ParameterDescriptor CreateParameterDescriptor(ParameterInfo parameterInfo)
        {
            return new ControllerParameterDescriptor
            {
                Name = parameterInfo.Name,
                ParameterInfo = parameterInfo,
                ParameterType = parameterInfo.ParameterType,
            };
        }
    }
}
