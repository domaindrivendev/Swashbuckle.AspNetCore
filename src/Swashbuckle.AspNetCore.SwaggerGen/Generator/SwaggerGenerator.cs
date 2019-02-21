using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SwaggerGeneratorOptions _options;

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaGenerator schemaGenerator,
            IOptions<SwaggerGeneratorOptions> optionsAccessor)
            : this(apiDescriptionsProvider, schemaGenerator, optionsAccessor.Value)
        { }

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaGenerator schemaGenerator,
            SwaggerGeneratorOptions options)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaGenerator = schemaGenerator;
            _options = options ?? new SwaggerGeneratorOptions();
        }

        public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
                throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select(d => d.Key));

            var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc => _options.DocInclusionPredicate(documentName, apiDesc))
                .Where(apiDesc => !_options.IgnoreObsoleteActions || !apiDesc.IsObsolete());

            var schemaRepository = new SchemaRepository();

            var swaggerDoc = new OpenApiDocument
            {
                Info = info,
                Servers = GenerateServers(host, basePath),
                Paths = GeneratePaths(applicableApiDescriptions, schemaRepository),
                Components = new OpenApiComponents
                {
                    Schemas = schemaRepository.Schemas,
                    SecuritySchemes = _options.SecuritySchemes
                },
                SecurityRequirements = _options.SecurityRequirements
            };

            var filterContext = new DocumentFilterContext(applicableApiDescriptions, _schemaGenerator, schemaRepository);
            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private IList<OpenApiServer> GenerateServers(string host, string basePath)
        {
            return (host == null && basePath == null)
                ? new List<OpenApiServer>()
                : new List<OpenApiServer> { new OpenApiServer { Url = $"{host}{basePath}" } };
        }

        private OpenApiPaths GeneratePaths(IEnumerable<ApiDescription> apiDescriptions, SchemaRepository schemaRepository)
        {
            var apiDescriptionsByPath = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString());

            var paths = new OpenApiPaths();
            foreach (var group in apiDescriptionsByPath)
            {
                paths.Add($"/{group.Key}", GeneratePathItem(group, schemaRepository));
            };

            return paths;
        }

        private OpenApiPathItem GeneratePathItem(IEnumerable<ApiDescription> apiDescriptions, SchemaRepository schemaRepository)
        {
            return new OpenApiPathItem
            {
                Operations = GenerateOperations(apiDescriptions, schemaRepository)
            };
        }

        private IDictionary<OperationType, OpenApiOperation> GenerateOperations(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository)
        {
            var apiDescriptionsByMethod = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.HttpMethod);

            var operations = new Dictionary<OperationType, OpenApiOperation>();

            foreach (var group in apiDescriptionsByMethod)
            {
                var httpMethod = group.Key;

                if (httpMethod == null)
                    throw new NotSupportedException(string.Format(
                        "Ambiguous HTTP method for action - {0}. " +
                        "Actions require an explicit HttpMethod binding for Swagger 2.0",
                        group.First().ActionDescriptor.DisplayName));

                if (group.Count() > 1 && _options.ConflictingActionsResolver == null)
                    throw new NotSupportedException(string.Format(
                        "HTTP method \"{0}\" & path \"{1}\" overloaded by actions - {2}. " +
                        "Actions require unique method/path combination for OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                        httpMethod,
                        group.First().RelativePathSansQueryString(),
                        string.Join(",", group.Select(apiDesc => apiDesc.ActionDescriptor.DisplayName))));

                var apiDescription = (group.Count() > 1) ? _options.ConflictingActionsResolver(group) : group.Single();

                operations.Add(OperationTypeMap[httpMethod.ToUpper()], GenerateOperation(apiDescription, schemaRepository));
            };

            return operations;
        }

        private OpenApiOperation GenerateOperation(ApiDescription apiDescription, SchemaRepository schemaRepository)
        {
            apiDescription.GetAdditionalMetadata(out MethodInfo methodInfo, out IEnumerable<object> methodAttributes);

            var operation = new OpenApiOperation
            {
                Tags = GenerateTags(apiDescription),
                OperationId = _options.OperationIdSelector(apiDescription),
                Parameters = GenerateParameters(apiDescription, schemaRepository),
                RequestBody = GenerateRequestBody(apiDescription, methodAttributes, schemaRepository),
                Responses = GenerateResponses(apiDescription, methodAttributes, schemaRepository),
                Deprecated = methodAttributes.OfType<ObsoleteAttribute>().Any()
            };

            var filterContext = new OperationFilterContext(apiDescription, _schemaGenerator, schemaRepository, methodInfo);
            foreach (var filter in _options.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }

            return operation;
        }

        private IList<OpenApiTag> GenerateTags(ApiDescription apiDescription)
        {
            return _options.TagsSelector(apiDescription)
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }

        private IList<OpenApiParameter> GenerateParameters(ApiDescription apiDescription, SchemaRepository schemaRespository)
        {
            var applicableApiParameters = apiDescription.ParameterDescriptions
                .Where(apiParam =>
                {
                    return (!apiParam.IsFromBody() && !apiParam.IsFromForm())
                        && (apiParam.ModelMetadata == null || apiParam.ModelMetadata.IsBindingAllowed);
                });

            return applicableApiParameters
                .Select(paramDesc => GenerateParameter(apiDescription, paramDesc, schemaRespository))
                .ToList();
        }

        private OpenApiParameter GenerateParameter(
            ApiDescription apiDescription,
            ApiParameterDescription apiParameter,
            SchemaRepository schemaRepository)
        {
            apiParameter.GetAdditionalMetadata(
                apiDescription,
                out ParameterInfo parameterInfo,
                out PropertyInfo propertyInfo,
                out IEnumerable<object> parameterOrPropertyAttributes);

            var name = _options.DescribeAllParametersInCamelCase
                ? apiParameter.Name.ToCamelCase()
                : apiParameter.Name;

            var location = ParameterLocationMap.ContainsKey(apiParameter.Source)
                ? ParameterLocationMap[apiParameter.Source]
                : ParameterLocation.Query;

            var isRequired = (apiParameter.IsFromPath())
                || parameterOrPropertyAttributes.Any(attr => RequiredAttributeTypes.Contains(attr.GetType()));

            var schema = (apiParameter.ModelMetadata != null)
                ? _schemaGenerator.GenerateSchema(apiParameter.Type, schemaRepository)
                : new OpenApiSchema { Type = "string" };

            // If it corresponds to an optional action parameter, assign the default value
            if (parameterInfo?.DefaultValue != null && schema.Reference == null)
            {
                schema.Default = OpenApiAnyFactory.TryCreateFrom(parameterInfo.DefaultValue, out IOpenApiAny openApiAny)
                    ? openApiAny
                    : null;
            }

            var parameter = new OpenApiParameter
            {
                Name = name,
                In = location,
                Required = isRequired,
                Schema = schema
            };

            var filterContext = new ParameterFilterContext(apiParameter, _schemaGenerator, schemaRepository, parameterInfo, propertyInfo);
            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private OpenApiRequestBody GenerateRequestBody(
            ApiDescription apiDescription,
            IEnumerable<object> methodAttributes,
            SchemaRepository schemaRepository)
        {
            var requestContentTypes = InferRequestContentTypes(apiDescription, methodAttributes);
            if (!requestContentTypes.Any()) return null;

            return new OpenApiRequestBody
            {
                Content = requestContentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => GenerateRequestMediaType(contentType, apiDescription, schemaRepository)
                    )
            };
        }

        private IEnumerable<string> InferRequestContentTypes(ApiDescription apiDescription, IEnumerable<object> methodAttributes)
        {
            // If there's no api parameters from body or form, return an empty list (i.e. no content)
            if (!apiDescription.ParameterDescriptions.Any(apiParam => apiParam.IsFromBody() || apiParam.IsFromForm()))
                return Enumerable.Empty<string>();

            // If there's content types explicitly specified via ConsumesAttribute, use them
            var explicitContentTypes = methodAttributes.OfType<ConsumesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiDescription.SupportedRequestFormats
                .Select(format => format.MediaType)
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            // As a last resort, try to infer from parameter bindings
            return apiDescription.ParameterDescriptions.Any(paramDesc => paramDesc.IsFromForm())
                ? new[] { "multipart/form-data" }
                : Enumerable.Empty<string>();
        }

        private OpenApiMediaType GenerateRequestMediaType(
            string contentType,
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            // If there's form parameters, generate the form-flavoured media type  
            var apiParametersFromForm = apiDescription.ParameterDescriptions
                .Where(paramDesc => paramDesc.IsFromForm());

            if (apiParametersFromForm.Any())
            {
                var schema = GenerateSchemaFromApiParameters(apiDescription, apiParametersFromForm, schemaRepository);

                // Provide schema and corresponding encoding map to specify "form" serialization style for all properties
                // This indicates that array properties must be submitted as multiple parameters with the same name.
                // NOTE: the swagger-ui doesn't currently support arrays of files - https://github.com/swagger-api/swagger-ui/issues/4600
                // NOTE: the swagger-ui doesn't currently honor encoding metadata - https://github.com/swagger-api/swagger-ui/issues/4836 

                return new OpenApiMediaType
                {
                    Schema = schema,
                    Encoding = schema.Properties.ToDictionary(
                        entry => entry.Key,
                        entry => new OpenApiEncoding { Style = ParameterStyle.Form }
                    )
                };
            } 

            // Otherwise, generate a regular media type
            var apiParameterFromBody = apiDescription.ParameterDescriptions
                .First(paramDesc => paramDesc.IsFromBody());

            return new OpenApiMediaType
            {
                Schema = _schemaGenerator.GenerateSchema(apiParameterFromBody.Type, schemaRepository)
            };
        }

        private OpenApiSchema GenerateSchemaFromApiParameters(
            ApiDescription apiDescription,
            IEnumerable<ApiParameterDescription> apiParameters,
            SchemaRepository schemaRepository)
        {
            // First, map to a data structure that captures the pertinent metadata
            var parametersMetadata = apiParameters
                .Select(paramDesc =>
                {
                    paramDesc.GetAdditionalMetadata(
                        apiDescription,
                        out ParameterInfo parameterInfo,
                        out PropertyInfo propertyInfo,
                        out IEnumerable<object> parameterOrPropertyAttributes);

                    var name = _options.DescribeAllParametersInCamelCase ? paramDesc.Name.ToCamelCase() : paramDesc.Name;
                    var isRequired = parameterOrPropertyAttributes.Any(attr => RequiredAttributeTypes.Contains(attr.GetType()));
                    var schema = _schemaGenerator.GenerateSchema(paramDesc.Type, schemaRepository);

                    return new
                    {
                        Name = name,
                        IsRequired = isRequired,
                        Schema = schema
                    };
                });

            return new OpenApiSchema
            {
                Type = "object",
                Properties = parametersMetadata.ToDictionary(
                    metadata => metadata.Name,
                    metadata => metadata.Schema
                ),
                Required = new SortedSet<string>(parametersMetadata.Where(m => m.IsRequired).Select(m => m.Name)),
            };
        }

        private OpenApiResponses GenerateResponses(
            ApiDescription apiDescription,
            IEnumerable<object> methodAttributes,
            SchemaRepository schemaRepository)
        {
            var supportedResponseTypes = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 200 });

            var responses = new OpenApiResponses();
            foreach (var responseType in supportedResponseTypes)
            {
                var statusCode = responseType.IsDefaultResponse() ? "default" : responseType.StatusCode.ToString();
                responses.Add(statusCode, GenerateResponse(responseType, methodAttributes, schemaRepository));
            }
            return responses;
        }

        private OpenApiResponse GenerateResponse(
            ApiResponseType apiResponseType,
            IEnumerable<object> methodAttributes,
            SchemaRepository schemaRepository)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(apiResponseType.StatusCode.ToString(), entry.Key))
                .Value;

            var responseContentTypes = InferResponseContentTypes(apiResponseType, methodAttributes);

            return new OpenApiResponse
            {
                Description = description,
                Content = responseContentTypes.ToDictionary(
                    contentType => contentType,
                    contentType => CreateResponseMediaType(apiResponseType.Type, schemaRepository)
                )
            };
        }

        private IEnumerable<string> InferResponseContentTypes(ApiResponseType apiResponseType, IEnumerable<object> methodAttributes)
        {
            // If there's no associated model, return an empty list (i.e. no content)
            if (apiResponseType.ModelMetadata == null) return Enumerable.Empty<string>();

            // If there's content types explicitly specified via ProducesAttribute, use them
            var explicitContentTypes = methodAttributes.OfType<ProducesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiResponseType.ApiResponseFormats
                .Select(responseFormat => responseFormat.MediaType)
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            return Enumerable.Empty<string>();
        }

        private OpenApiMediaType CreateResponseMediaType(Type type, SchemaRepository schemaRespository)
        {
            return new OpenApiMediaType
            {
                Schema = _schemaGenerator.GenerateSchema(type, schemaRespository)
            };
        }

        private static readonly Dictionary<string, OperationType> OperationTypeMap = new Dictionary<string, OperationType>
        {
            { "GET", OperationType.Get },
            { "PUT", OperationType.Put },
            { "POST", OperationType.Post },
            { "DELETE", OperationType.Delete },
            { "OPTIONS", OperationType.Options },
            { "HEAD", OperationType.Head },
            { "PATCH", OperationType.Patch },
            { "TRACE", OperationType.Trace }
        };

        private static Dictionary<BindingSource, ParameterLocation> ParameterLocationMap = new Dictionary<BindingSource, ParameterLocation>
        {
            { BindingSource.Query, ParameterLocation.Query },
            { BindingSource.Header, ParameterLocation.Header },
            { BindingSource.Path, ParameterLocation.Path }
        };

        private static IEnumerable<Type> RequiredAttributeTypes = new[]
        {
            typeof(BindRequiredAttribute),
            typeof(RequiredAttribute)
        };

        private static readonly Dictionary<string, string> ResponseDescriptionMap = new Dictionary<string, string>
        {
            { "1\\d{2}", "Information" },
            { "2\\d{2}", "Success" },
            { "304", "Not Modified" },
            { "3\\d{2}", "Redirect" },
            { "400", "Bad Request" },
            { "401", "Unauthorized" },
            { "403", "Forbidden" },
            { "404", "Not Found" },
            { "405", "Method Not Allowed" },
            { "406", "Not Acceptable" },
            { "408", "Request Timeout" },
            { "409", "Conflict" },
            { "4\\d{2}", "Client Error" },
            { "5\\d{2}", "Server Error" }
        };
    }
}
