using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaValidator
{
    class SchemaValidator : ISchemaValidator
    {
        private const int MIN_SUCCESS_STATUS_CODE = 200;
        private const int MAX_SUCCESS_STATUS_CODE = 299;

        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly SwaggerGeneratorOptions _options;

        public SchemaValidator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            SwaggerGeneratorOptions options
        )
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _options = options;
        }

        public SchemaValidationResult ValidateControllerResponses()
        {
            var schemaValidationResult = SchemaValidationResult.Create();

            var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc => !(
                        _options.IgnoreObsoleteActions &&
                        apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any()
                    )
                );

            foreach (var apiDescription in applicableApiDescriptions)
            {

                if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    var methodReturnType = controllerActionDescriptor.MethodInfo.ReturnParameter
                        .ParameterType;

                    // Check if it is task
                    if (
                        methodReturnType.IsGenericType &&
                        methodReturnType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Task<>) &&
                        methodReturnType.GetTypeInfo().GetGenericArguments().Length > 0
                    )
                    {
                        methodReturnType = methodReturnType.GetGenericArguments().First();
                    }

                    // Check if it is ActionResult
                    if (
                        methodReturnType.IsGenericType &&
                        methodReturnType.GetTypeInfo().GetGenericTypeDefinition() == typeof(ActionResult<>)
                    )
                    {
                        methodReturnType = methodReturnType.GetGenericArguments().First();
                    }

                    // Get list of unsupported response type (e.g. From [ProducesResponseType])
                    var unsupportedSuccessResponseSystemTypes = GetSuccessSupportedResponsesTypes(
                        apiDescription
                    )
                    .Where(type => methodReturnType != type)
                    .ToList();

                    foreach (var unsupportedType in unsupportedSuccessResponseSystemTypes)
                    {
                        schemaValidationResult.AddError(
                            SchemaValidationError.Create(
                                apiDescription,
                                unsupportedType,
                                methodReturnType
                            )
                        );
                    }
                }
            }

            return schemaValidationResult;
        }

        private IEnumerable<Type> GetSuccessSupportedResponsesTypes(ApiDescription apiDescription)
            => apiDescription.SupportedResponseTypes
                .Where(supportedResponseType =>
                    CheckIfSuccessResponseType(supportedResponseType) &&
                    supportedResponseType.Type != typeof(void) // This annotation: [ProducesResponseType(200)] make supportedResponseType = System.Void
                )
                .Select(supportedResponseType => supportedResponseType.Type
                        .GetTypeInfo().ImplementedInterfaces.Contains(typeof(IConvertToActionResult))
                        ? supportedResponseType.Type.GetGenericArguments().First()
                        : supportedResponseType.Type
                )
                .DefaultIfEmpty(typeof(ActionResult)) // if list is empty, add ActionResult as default value
                .ToList();

        // Check if response status is inside range of success statuses codes
        private bool CheckIfSuccessResponseType(ApiResponseType responseType)
            => responseType.StatusCode is >= MIN_SUCCESS_STATUS_CODE and <= MAX_SUCCESS_STATUS_CODE;
    }
}