using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;

#if NET7_0_OR_GREATER
using Microsoft.AspNetCore.Http.Metadata;
#endif

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerGenerator : ISwaggerProvider, IAsyncSwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SwaggerGeneratorOptions _options;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public SwaggerGenerator(
            SwaggerGeneratorOptions options,
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaGenerator schemaGenerator)
        {
            _options = options ?? new SwaggerGeneratorOptions();
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaGenerator = schemaGenerator;
        }

        public SwaggerGenerator(
            SwaggerGeneratorOptions options,
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaGenerator schemaGenerator,
            IAuthenticationSchemeProvider authenticationSchemeProvider) : this(options, apiDescriptionsProvider, schemaGenerator)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        public async Task<OpenApiDocument> GetSwaggerAsync(
            string documentName,
            string host = null,
            string basePath = null)
        {
            var (filterContext, swaggerDoc) = GetSwaggerDocumentWithoutPaths(documentName, host, basePath);

            swaggerDoc.Paths = await GeneratePathsAsync(filterContext.ApiDescriptions, filterContext.SchemaRepository);
            swaggerDoc.Components.SecuritySchemes = await GetSecuritySchemesAsync();

            // NOTE: Filter processing moved here so they may effect generated security schemes
            foreach (var filter in _options.DocumentAsyncFilters)
            {
                await filter.ApplyAsync(swaggerDoc, filterContext, CancellationToken.None);
            }

            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            SortSchemas(swaggerDoc);

            return swaggerDoc;
        }

        public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
        {
            try
            {
                var (filterContext, swaggerDoc) = GetSwaggerDocumentWithoutPaths(documentName, host, basePath);

                swaggerDoc.Paths = GeneratePaths(filterContext.ApiDescriptions, filterContext.SchemaRepository);
                swaggerDoc.Components.SecuritySchemes = GetSecuritySchemesAsync().Result;

                // NOTE: Filter processing moved here so they may effect generated security schemes
                foreach (var filter in _options.DocumentFilters)
                {
                    filter.Apply(swaggerDoc, filterContext);
                }

                SortSchemas(swaggerDoc);

                return swaggerDoc;
            }
            catch (AggregateException ex)
            {
                // Unwrap any AggregateException from using async methods to run the synchronous filters
                var inner = ex.InnerException;

                while (inner is not null)
                {
                    if (inner is AggregateException)
                    {
                        inner = inner.InnerException;
                    }
                    else
                    {
                        throw inner;
                    }
                }

                throw;
            }
        }

        private void SortSchemas(OpenApiDocument document)
        {
            document.Components.Schemas = new SortedDictionary<string, OpenApiSchema>(document.Components.Schemas, _options.SchemaComparer);
        }

        private (DocumentFilterContext, OpenApiDocument) GetSwaggerDocumentWithoutPaths(string documentName, string host = null, string basePath = null)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
            {
                throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select(d => d.Key));
            }

            var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc =>
                {
                    var attributes = apiDesc.CustomAttributes().ToList();
                    return !(_options.IgnoreObsoleteActions && attributes.OfType<ObsoleteAttribute>().Any()) &&
                           !attributes.OfType<SwaggerIgnoreAttribute>().Any() &&
                           _options.DocInclusionPredicate(documentName, apiDesc);
                });

            var schemaRepository = new SchemaRepository(documentName);

            var swaggerDoc = new OpenApiDocument
            {
                Info = info,
                Servers = GenerateServers(host, basePath),
                Components = new OpenApiComponents
                {
                    Schemas = schemaRepository.Schemas,
                },
                SecurityRequirements = new List<OpenApiSecurityRequirement>(_options.SecurityRequirements)
            };

            return (new DocumentFilterContext(applicableApiDescriptions, _schemaGenerator, schemaRepository), swaggerDoc);
        }

        private async Task<IDictionary<string, OpenApiSecurityScheme>> GetSecuritySchemesAsync()
        {
            if (!_options.InferSecuritySchemes)
            {
                return new Dictionary<string, OpenApiSecurityScheme>(_options.SecuritySchemes);
            }

            var authenticationSchemes = (_authenticationSchemeProvider is not null)
                ? await _authenticationSchemeProvider.GetAllSchemesAsync()
                : [];

            if (_options.SecuritySchemesSelector != null)
            {
                return _options.SecuritySchemesSelector(authenticationSchemes);
            }

            // Default implementation, currently only supports JWT Bearer scheme
            return authenticationSchemes
                .Where(authScheme => authScheme.Name == "Bearer")
                .ToDictionary(
                    (authScheme) => authScheme.Name,
                    (authScheme) => new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer", // "bearer" refers to the header name here
                        In = ParameterLocation.Header,
                        BearerFormat = "Json Web Token"
                    });
        }

        private List<OpenApiServer> GenerateServers(string host, string basePath)
        {
            if (_options.Servers.Count > 0)
            {
                return new List<OpenApiServer>(_options.Servers);
            }

            return (host == null && basePath == null)
                ? []
                : [new() { Url = $"{host}{basePath}" }];
        }

        private async Task<OpenApiPaths> GeneratePathsAsync(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository,
            Func<IGrouping<string, ApiDescription>, SchemaRepository, Task<Dictionary<OperationType, OpenApiOperation>>> operationsGenerator)
        {
            var apiDescriptionsByPath = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.RelativePathSansParameterConstraints());

            var paths = new OpenApiPaths();
            foreach (var group in apiDescriptionsByPath)
            {
                paths.Add($"/{group.Key}",
                    new OpenApiPathItem
                    {
                        Operations = await operationsGenerator(group, schemaRepository)
                    });
            };

            return paths;
        }

        private OpenApiPaths GeneratePaths(IEnumerable<ApiDescription> apiDescriptions, SchemaRepository schemaRepository)
        {
            return GeneratePathsAsync(
                apiDescriptions,
                schemaRepository,
                (group, schemaRepository) => Task.FromResult(GenerateOperations(group, schemaRepository))).Result;
        }

        private async Task<OpenApiPaths> GeneratePathsAsync(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository)
        {
            return await GeneratePathsAsync(
                apiDescriptions,
                schemaRepository,
                GenerateOperationsAsync);
        }

        private IEnumerable<(OperationType, ApiDescription)> GetOperationsGroupedByMethod(
            IEnumerable<ApiDescription> apiDescriptions)
        {
            return apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.HttpMethod)
                .Select(PrepareGenerateOperation);
        }

        private Dictionary<OperationType, OpenApiOperation> GenerateOperations(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository)
        {
            var apiDescriptionsByMethod = GetOperationsGroupedByMethod(apiDescriptions);
            var operations = new Dictionary<OperationType, OpenApiOperation>();

            foreach ((var operationType, var description) in apiDescriptionsByMethod)
            {
                operations.Add(operationType, GenerateOperation(description, schemaRepository));
            }

            return operations;
        }

        private async Task<Dictionary<OperationType, OpenApiOperation>> GenerateOperationsAsync(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository)
        {
            var apiDescriptionsByMethod = GetOperationsGroupedByMethod(apiDescriptions);
            var operations = new Dictionary<OperationType, OpenApiOperation>();

            foreach ((var operationType, var description) in apiDescriptionsByMethod)
            {
                operations.Add(operationType, await GenerateOperationAsync(description, schemaRepository));
            }

            return operations;
        }

        private (OperationType OperationType, ApiDescription ApiDescription) PrepareGenerateOperation(IGrouping<string, ApiDescription> group)
        {
            var httpMethod = group.Key ?? throw new SwaggerGeneratorException(string.Format(
                "Ambiguous HTTP method for action - {0}. " +
                "Actions require an explicit HttpMethod binding for Swagger/OpenAPI 3.0",
                group.First().ActionDescriptor.DisplayName));

            var count = group.Count();

            if (count > 1 && _options.ConflictingActionsResolver == null)
            {
                throw new SwaggerGeneratorException(string.Format(
                    "Conflicting method/path combination \"{0} {1}\" for actions - {2}. " +
                    "Actions require a unique method/path combination for Swagger/OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                    httpMethod,
                    group.First().RelativePath,
                    string.Join(",", group.Select(apiDesc => apiDesc.ActionDescriptor.DisplayName))));
            }

            var apiDescription = (count > 1) ? _options.ConflictingActionsResolver(group) : group.Single();

            var normalizedMethod = httpMethod.ToUpperInvariant();
            if (!OperationTypeMap.TryGetValue(normalizedMethod, out var operationType))
            {
                // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2600 and
                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2740.
                throw new SwaggerGeneratorException($"The \"{httpMethod}\" HTTP method is not supported.");
            }

            return (operationType, apiDescription);
        }

        private async Task<OpenApiOperation> GenerateOperationAsync(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            Func<ApiDescription, SchemaRepository, Task<List<OpenApiParameter>>> parametersGenerator,
            Func<ApiDescription, SchemaRepository, Task<OpenApiRequestBody>> bodyGenerator,
            Func<OpenApiOperation, OperationFilterContext, Task> applyFilters)
        {
            OpenApiOperation operation =
#if NET6_0_OR_GREATER
                await GenerateOpenApiOperationFromMetadataAsync(apiDescription, schemaRepository);
#else
                null;
#endif

            try
            {
                operation ??= new OpenApiOperation
                {
                    Tags = GenerateOperationTags(apiDescription),
                    OperationId = _options.OperationIdSelector(apiDescription),
                    Parameters = await parametersGenerator(apiDescription, schemaRepository),
                    RequestBody = await bodyGenerator(apiDescription, schemaRepository),
                    Responses = GenerateResponses(apiDescription, schemaRepository),
                    Deprecated = apiDescription.CustomAttributes().OfType<ObsoleteAttribute>().Any(),
#if NET7_0_OR_GREATER
                    Summary = GenerateSummary(apiDescription),
                    Description = GenerateDescription(apiDescription),
#endif
                };

                apiDescription.TryGetMethodInfo(out MethodInfo methodInfo);
                var filterContext = new OperationFilterContext(apiDescription, _schemaGenerator, schemaRepository, methodInfo);

                await applyFilters(operation, filterContext);

                return operation;
            }
            catch (Exception ex)
            {
                throw new SwaggerGeneratorException(
                    message: $"Failed to generate Operation for action - {apiDescription.ActionDescriptor.DisplayName}. See inner exception",
                    innerException: ex);
            }
        }

        private OpenApiOperation GenerateOperation(ApiDescription apiDescription, SchemaRepository schemaRepository)
        {
            return GenerateOperationAsync(
                apiDescription,
                schemaRepository,
                (description, repository) => Task.FromResult(GenerateParameters(description, repository)),
                (description, repository) => Task.FromResult(GenerateRequestBody(description, repository)),
                (operation, filterContext) =>
                {
                    foreach (var filter in _options.OperationFilters)
                    {
                        filter.Apply(operation, filterContext);
                    }

                    return Task.CompletedTask;
                }).Result;
        }

        private async Task<OpenApiOperation> GenerateOperationAsync(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            return await GenerateOperationAsync(
                apiDescription,
                schemaRepository,
                GenerateParametersAsync,
                GenerateRequestBodyAsync,
                async (operation, filterContext) =>
                {
                    foreach (var filter in _options.OperationAsyncFilters)
                    {
                        await filter.ApplyAsync(operation, filterContext, CancellationToken.None);
                    }

                    foreach (var filter in _options.OperationFilters)
                    {
                        filter.Apply(operation, filterContext);
                    }
                });
        }

#if NET6_0_OR_GREATER
        private async Task<OpenApiOperation> GenerateOpenApiOperationFromMetadataAsync(ApiDescription apiDescription, SchemaRepository schemaRepository)
        {
            var metadata = apiDescription.ActionDescriptor?.EndpointMetadata;
            var operation = metadata?.OfType<OpenApiOperation>().SingleOrDefault();

            if (operation is null)
            {
                return null;
            }

            // Schemas will be generated via Swashbuckle by default.
            foreach (var parameter in operation.Parameters)
            {
                var apiParameter = apiDescription.ParameterDescriptions.SingleOrDefault(desc => desc.Name == parameter.Name && !desc.IsFromBody() && !desc.IsFromForm() && !desc.IsIllegalHeaderParameter());
                if (apiParameter is not null)
                {
                    var (parameterAndContext, filterContext) = GenerateParameterAndContext(apiParameter, schemaRepository);
                    parameter.Schema = parameterAndContext.Schema;
                    parameter.Description ??= parameterAndContext.Description;

                    foreach (var filter in _options.ParameterAsyncFilters)
                    {
                        await filter.ApplyAsync(parameter, filterContext, CancellationToken.None);
                    }

                    foreach (var filter in _options.ParameterFilters)
                    {
                        filter.Apply(parameter, filterContext);
                    }
                }
            }

            var requestContentTypes = operation.RequestBody?.Content?.Values;
            if (requestContentTypes is not null)
            {
                foreach (var content in requestContentTypes)
                {
                    content.Encoding = new Dictionary<string, OpenApiEncoding>();
                    var requestParameters = apiDescription.ParameterDescriptions.Where(desc => desc.IsFromBody() || desc.IsFromForm());
                    var countOfParameters = requestParameters.Count();
                    if (countOfParameters > 0)
                    {
                        ApiParameterDescription bodyParameterDescription = null;
                        if (countOfParameters == 1)
                        {
                            var requestParameter = requestParameters.First();
                            content.Schema = GetSchemaForFormParameter(requestParameter, GenerateSchema(
                                requestParameter.ModelMetadata.ModelType,
                                schemaRepository,
                                requestParameter.PropertyInfo(),
                                requestParameter.ParameterInfo()), content, true, out _);

                            bodyParameterDescription = requestParameter.IsFromBody() ? requestParameter : null;
                        }
                        else
                        {
                            content.Schema = new OpenApiSchema()
                            {
                                AllOf = requestParameters.Select(s =>
                                    GetSchemaForFormParameter(s, GenerateSchema(
                                    s.ModelMetadata.ModelType,
                                    schemaRepository,
                                    s.PropertyInfo(),
                                    s.ParameterInfo()), content, true, out _))
                                .ToList()
                            };
                        }

                        var filterContext = new RequestBodyFilterContext(
                            bodyParameterDescription: bodyParameterDescription,
                            formParameterDescriptions: bodyParameterDescription is null ? requestParameters : null,
                            schemaGenerator: _schemaGenerator,
                            schemaRepository: schemaRepository);

                        foreach (var filter in _options.RequestBodyAsyncFilters)
                        {
                            await filter.ApplyAsync(operation.RequestBody, filterContext, CancellationToken.None);
                        }

                        foreach (var filter in _options.RequestBodyFilters)
                        {
                            filter.Apply(operation.RequestBody, filterContext);
                        }
                    }
                }
            }

            foreach (var kvp in operation.Responses)
            {
                var response = kvp.Value;
                var responseModel = apiDescription.SupportedResponseTypes.SingleOrDefault(desc => desc.StatusCode.ToString() == kvp.Key);
                if (responseModel is not null)
                {
                    var responseContentTypes = response?.Content?.Values;
                    if (responseContentTypes is not null)
                    {
                        foreach (var content in responseContentTypes)
                        {
                            content.Schema = GenerateSchema(responseModel.Type, schemaRepository);
                        }
                    }
                }
            }

            return operation;
        }
#endif

        private List<OpenApiTag> GenerateOperationTags(ApiDescription apiDescription)
        {
            return _options.TagsSelector(apiDescription)
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }

        private static async Task<List<OpenApiParameter>> GenerateParametersAsync(
            ApiDescription apiDescription,
            SchemaRepository schemaRespository,
            Func<ApiParameterDescription, SchemaRepository, Task<OpenApiParameter>> parameterGenerator)
        {
            if (apiDescription.ParameterDescriptions.Any(IsFromFormAttributeUsedWithIFormFile))
            {
                throw new SwaggerGeneratorException(string.Format(
                       "Error reading parameter(s) for action {0} as [FromForm] attribute used with IFormFile. " +
                       "Please refer to https://github.com/domaindrivendev/Swashbuckle.AspNetCore#handle-forms-and-file-uploads for more information",
                       apiDescription.ActionDescriptor.DisplayName));
            }

            var applicableApiParameters = apiDescription.ParameterDescriptions
                .Where(apiParam =>
                {
                    return !apiParam.IsFromBody() && !apiParam.IsFromForm()
                        && (!apiParam.CustomAttributes().OfType<BindNeverAttribute>().Any())
                        && (!apiParam.CustomAttributes().OfType<SwaggerIgnoreAttribute>().Any())
                        && (apiParam.ModelMetadata == null || apiParam.ModelMetadata.IsBindingAllowed)
                        && !apiParam.IsIllegalHeaderParameter();
                });

            var parameters = new List<OpenApiParameter>();

            foreach (var parameter in applicableApiParameters)
            {
                parameters.Add(await parameterGenerator(parameter, schemaRespository));
            }

            return parameters;
        }

        private List<OpenApiParameter> GenerateParameters(ApiDescription apiDescription, SchemaRepository schemaRespository)
        {
            return GenerateParametersAsync(
                apiDescription,
                schemaRespository,
                (parameter, schemaRespository) => Task.FromResult(GenerateParameter(parameter, schemaRespository))).Result;
        }

        private async Task<List<OpenApiParameter>> GenerateParametersAsync(
            ApiDescription apiDescription,
            SchemaRepository schemaRespository)
        {
            return await GenerateParametersAsync(
                apiDescription,
                schemaRespository,
                GenerateParameterAsync);
        }

        private OpenApiParameter GenerateParameterWithoutFilter(
            ApiParameterDescription apiParameter,
            SchemaRepository schemaRepository)
        {
            var name = _options.DescribeAllParametersInCamelCase
                ? apiParameter.Name.ToCamelCase()
                : apiParameter.Name;

            var location = apiParameter.Source != null &&
                            ParameterLocationMap.TryGetValue(apiParameter.Source, out var value)
                ? value
                : ParameterLocation.Query;

            var isRequired = apiParameter.IsRequiredParameter();

            var type = apiParameter.ModelMetadata?.ModelType;

            if (type is not null
                && type == typeof(string)
                && apiParameter.Type is not null
                && (Nullable.GetUnderlyingType(apiParameter.Type) ?? apiParameter.Type).IsEnum)
            {
                type = apiParameter.Type;
            }

            var schema = (type != null)
                ? GenerateSchema(
                    type,
                    schemaRepository,
                    apiParameter.PropertyInfo(),
                    apiParameter.ParameterInfo(),
                    apiParameter.RouteInfo)
                : new OpenApiSchema { Type = "string" };

            var description = schema.Description;
            if (string.IsNullOrEmpty(description)
                && !string.IsNullOrEmpty(schema?.Reference?.Id)
                && schemaRepository.Schemas.TryGetValue(schema.Reference.Id, out var openApiSchema))
            {
                description = openApiSchema.Description;
            }

            return new OpenApiParameter
            {
                Name = name,
                In = location,
                Required = isRequired,
                Schema = schema,
                Description = description
            };
        }

        private (OpenApiParameter, ParameterFilterContext) GenerateParameterAndContext(
            ApiParameterDescription apiParameter,
            SchemaRepository schemaRepository)
        {
            var parameter = GenerateParameterWithoutFilter(apiParameter, schemaRepository);

            var context = new ParameterFilterContext(
                apiParameter,
                _schemaGenerator,
                schemaRepository,
                apiParameter.PropertyInfo(),
                apiParameter.ParameterInfo());

            return (parameter, context);
        }

        private OpenApiParameter GenerateParameter(
            ApiParameterDescription apiParameter,
            SchemaRepository schemaRepository)
        {
            var (parameter, filterContext) = GenerateParameterAndContext(apiParameter, schemaRepository);

            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private async Task<OpenApiParameter> GenerateParameterAsync(
            ApiParameterDescription apiParameter,
            SchemaRepository schemaRepository)
        {
            var (parameter, filterContext) = GenerateParameterAndContext(apiParameter, schemaRepository);

            foreach (var filter in _options.ParameterAsyncFilters)
            {
                await filter.ApplyAsync(parameter, filterContext, CancellationToken.None);
            }

            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            PropertyInfo propertyInfo = null,
            ParameterInfo parameterInfo = null,
            ApiParameterRouteInfo routeInfo = null)
        {
            try
            {
                return _schemaGenerator.GenerateSchema(type, schemaRepository, propertyInfo, parameterInfo, routeInfo);
            }
            catch (Exception ex)
            {
                throw new SwaggerGeneratorException(
                    message: $"Failed to generate schema for type - {type}. See inner exception",
                    innerException: ex);
            }
        }

        private (OpenApiRequestBody RequestBody, RequestBodyFilterContext FilterContext) GenerateRequestBodyAndFilterContext(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            OpenApiRequestBody requestBody = null;
            RequestBodyFilterContext filterContext = null;

            var bodyParameter = apiDescription.ParameterDescriptions
                .FirstOrDefault(paramDesc => paramDesc.IsFromBody());

            var formParameters = apiDescription.ParameterDescriptions
                .Where(paramDesc => paramDesc.IsFromForm())
                .ToList();

            if (bodyParameter != null)
            {
                requestBody = GenerateRequestBodyFromBodyParameter(apiDescription, schemaRepository, bodyParameter);

                filterContext = new RequestBodyFilterContext(
                    bodyParameterDescription: bodyParameter,
                    formParameterDescriptions: null,
                    schemaGenerator: _schemaGenerator,
                    schemaRepository: schemaRepository);
            }
            else if (formParameters.Count > 0)
            {
                requestBody = GenerateRequestBodyFromFormParameters(apiDescription, schemaRepository, formParameters);

                filterContext = new RequestBodyFilterContext(
                    bodyParameterDescription: null,
                    formParameterDescriptions: formParameters,
                    schemaGenerator: _schemaGenerator,
                    schemaRepository: schemaRepository);
            }

            return (requestBody, filterContext);
        }

        private OpenApiRequestBody GenerateRequestBody(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            var (requestBody, filterContext) = GenerateRequestBodyAndFilterContext(apiDescription, schemaRepository);

            if (requestBody != null)
            {
                foreach (var filter in _options.RequestBodyFilters)
                {
                    filter.Apply(requestBody, filterContext);
                }
            }

            return requestBody;
        }

        private async Task<OpenApiRequestBody> GenerateRequestBodyAsync(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            var (requestBody, filterContext) = GenerateRequestBodyAndFilterContext(apiDescription, schemaRepository);

            if (requestBody != null)
            {
                foreach (var filter in _options.RequestBodyAsyncFilters)
                {
                    await filter.ApplyAsync(requestBody, filterContext, CancellationToken.None);
                }

                foreach (var filter in _options.RequestBodyFilters)
                {
                    filter.Apply(requestBody, filterContext);
                }
            }

            return requestBody;
        }

        private OpenApiRequestBody GenerateRequestBodyFromBodyParameter(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            ApiParameterDescription bodyParameter)
        {
            var contentTypes = InferRequestContentTypes(apiDescription);

            var isRequired = bodyParameter.IsRequiredParameter();

            var schema = GenerateSchema(
                bodyParameter.ModelMetadata.ModelType,
                schemaRepository,
                bodyParameter.PropertyInfo(),
                bodyParameter.ParameterInfo());

            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema
                        }
                    ),
                Required = isRequired
            };
        }

        private static IEnumerable<string> InferRequestContentTypes(ApiDescription apiDescription)
        {
            // If there's content types explicitly specified via ConsumesAttribute, use them
            var explicitContentTypes = apiDescription.CustomAttributes().OfType<ConsumesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            return apiDescription.SupportedRequestFormats
                .Select(format => format.MediaType)
                .Where(x => x != null)
                .Distinct();
        }

        private OpenApiRequestBody GenerateRequestBodyFromFormParameters(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            IEnumerable<ApiParameterDescription> formParameters)
        {
            var contentTypes = InferRequestContentTypes(apiDescription);
            contentTypes = contentTypes.Any() ? contentTypes : ["multipart/form-data"];

            var schema = GenerateSchemaFromFormParameters(formParameters, schemaRepository);

            var totalProperties = schema.AllOf
                    ?.FirstOrDefault(s => s.Properties.Count > 0)
                    ?.Properties ?? schema.Properties;

            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema,
                            Encoding = totalProperties.ToDictionary(
                                entry => entry.Key,
                                entry => new OpenApiEncoding { Style = ParameterStyle.Form }
                            )
                        }
                    )
            };
        }

        private OpenApiSchema GenerateSchemaFromFormParameters(
            IEnumerable<ApiParameterDescription> formParameters,
            SchemaRepository schemaRepository)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredPropertyNames = new List<string>();
            var ownSchemas = new List<OpenApiSchema>();
            foreach (var formParameter in formParameters)
            {
                var propertyInfo = formParameter.PropertyInfo();
                if (!propertyInfo?.HasAttribute<SwaggerIgnoreAttribute>() ?? true)
                {
                    var schema = (formParameter.ModelMetadata != null)
                    ? GenerateSchema(
                        formParameter.ModelMetadata.ModelType,
                        schemaRepository,
                        propertyInfo,
                        formParameter.ParameterInfo())
                    : new OpenApiSchema { Type = "string" };

                    GetSchemaForFormParameter(formParameter, schema, null, false, out bool isParameter);
                    if (isParameter)
                    {
                        var name = _options.DescribeAllParametersInCamelCase
                            ? formParameter.Name.ToCamelCase()
                            : formParameter.Name;
                        properties.Add(name, schema);
                        if (formParameter.IsRequiredParameter())
                        {
                            requiredPropertyNames.Add(name);
                        }
                    }
                    else
                    {
                        ownSchemas.Add(schema);
                    }
                }
            }

            if (ownSchemas.Count > 0)
            {
                bool isAllOf = ownSchemas.Count > 1 || (ownSchemas.Count >= 1 && properties.Count >= 1);
                if (isAllOf)
                {
                    var allOfSchema = new OpenApiSchema()
                    {
                        AllOf = ownSchemas
                    };
                    if (properties.Count > 0)
                    {
                        allOfSchema.AllOf.Add(GenerateSchemaForProperties(properties, requiredPropertyNames));
                    }
                    return allOfSchema;
                }
                return ownSchemas.First();
            }

            return GenerateSchemaForProperties(properties, requiredPropertyNames);

            static OpenApiSchema GenerateSchemaForProperties(Dictionary<string, OpenApiSchema> properties, List<string> requiredPropertyNames) =>
                 new()
                 {
                     Type = "object",
                     Properties = properties,
                     Required = new SortedSet<string>(requiredPropertyNames)
                 };
        }

        private OpenApiResponses GenerateResponses(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            var supportedResponseTypes = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 200 });

            var responses = new OpenApiResponses();
            foreach (var responseType in supportedResponseTypes)
            {
                var statusCode = responseType.IsDefaultResponse() ? "default" : responseType.StatusCode.ToString();
                responses.Add(statusCode, GenerateResponse(apiDescription, schemaRepository, statusCode, responseType));
            }
            return responses;
        }

        private OpenApiResponse GenerateResponse(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            string statusCode,
            ApiResponseType apiResponseType)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(statusCode, entry.Key))
                .Value;

            var responseContentTypes = InferResponseContentTypes(apiDescription, apiResponseType);

            return new OpenApiResponse
            {
                Description = description,
                Content = responseContentTypes.ToDictionary(
                    contentType => contentType,
                    contentType => CreateResponseMediaType(apiResponseType.ModelMetadata?.ModelType ?? apiResponseType.Type, schemaRepository)
                )
            };
        }

        private static IEnumerable<string> InferResponseContentTypes(ApiDescription apiDescription, ApiResponseType apiResponseType)
        {
            // If there's no associated model type, return an empty list (i.e. no content)
            if (apiResponseType.ModelMetadata == null && (apiResponseType.Type == null || apiResponseType.Type == typeof(void)))
            {
                return [];
            }

            // If there's content types explicitly specified via ProducesAttribute, use them
            var explicitContentTypes = apiDescription.CustomAttributes().OfType<ProducesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiResponseType.ApiResponseFormats
                .Select(responseFormat => responseFormat.MediaType)
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            return [];
        }

        private OpenApiMediaType CreateResponseMediaType(Type modelType, SchemaRepository schemaRespository)
        {
            return new OpenApiMediaType
            {
                Schema = GenerateSchema(modelType, schemaRespository)
            };
        }

        private static bool IsFromFormAttributeUsedWithIFormFile(ApiParameterDescription apiParameter)
        {
            var parameterInfo = apiParameter.ParameterInfo();
            var fromFormAttribute = parameterInfo?.GetCustomAttribute<FromFormAttribute>();

            return fromFormAttribute != null && parameterInfo?.ParameterType == typeof(IFormFile);
        }

        private static readonly Dictionary<string, OperationType> OperationTypeMap = new()
        {
            ["GET"] = OperationType.Get,
            ["PUT"] = OperationType.Put,
            ["POST"] = OperationType.Post,
            ["DELETE"] = OperationType.Delete,
            ["OPTIONS"] = OperationType.Options,
            ["HEAD"] = OperationType.Head,
            ["PATCH"] = OperationType.Patch,
            ["TRACE"] = OperationType.Trace,
        };

        private static readonly Dictionary<BindingSource, ParameterLocation> ParameterLocationMap = new()
        {
            [BindingSource.Query] = ParameterLocation.Query,
            [BindingSource.Header] = ParameterLocation.Header,
            [BindingSource.Path] = ParameterLocation.Path,
        };

        private static readonly IReadOnlyCollection<KeyValuePair<string, string>> ResponseDescriptionMap =
        [
            new KeyValuePair<string, string>("100", "Continue"),
            new KeyValuePair<string, string>("101", "Switching Protocols"),
            new KeyValuePair<string, string>("1\\d{2}", "Information"),

            new KeyValuePair<string, string>("200", "OK"),
            new KeyValuePair<string, string>("201", "Created"),
            new KeyValuePair<string, string>("202", "Accepted"),
            new KeyValuePair<string, string>("203", "Non-Authoritative Information"),
            new KeyValuePair<string, string>("204", "No Content"),
            new KeyValuePair<string, string>("205", "Reset Content"),
            new KeyValuePair<string, string>("206", "Partial Content"),
            new KeyValuePair<string, string>("2\\d{2}", "Success"),

            new KeyValuePair<string, string>("300", "Multiple Choices"),
            new KeyValuePair<string, string>("301", "Moved Permanently"),
            new KeyValuePair<string, string>("302", "Found"),
            new KeyValuePair<string, string>("303", "See Other"),
            new KeyValuePair<string, string>("304", "Not Modified"),
            new KeyValuePair<string, string>("305", "Use Proxy"),
            new KeyValuePair<string, string>("307", "Temporary Redirect"),
            new KeyValuePair<string, string>("308", "Permanent Redirect"),
            new KeyValuePair<string, string>("3\\d{2}", "Redirect"),

            new KeyValuePair<string, string>("400", "Bad Request"),
            new KeyValuePair<string, string>("401", "Unauthorized"),
            new KeyValuePair<string, string>("402", "Payment Required"),
            new KeyValuePair<string, string>("403", "Forbidden"),
            new KeyValuePair<string, string>("404", "Not Found"),
            new KeyValuePair<string, string>("405", "Method Not Allowed"),
            new KeyValuePair<string, string>("406", "Not Acceptable"),
            new KeyValuePair<string, string>("407", "Proxy Authentication Required"),
            new KeyValuePair<string, string>("408", "Request Timeout"),
            new KeyValuePair<string, string>("409", "Conflict"),
            new KeyValuePair<string, string>("410", "Gone"),
            new KeyValuePair<string, string>("411", "Length Required"),
            new KeyValuePair<string, string>("412", "Precondition Failed"),
            new KeyValuePair<string, string>("413", "Content Too Large"),
            new KeyValuePair<string, string>("414", "URI Too Long"),
            new KeyValuePair<string, string>("415", "Unsupported Media Type"),
            new KeyValuePair<string, string>("416", "Range Not Satisfiable"),
            new KeyValuePair<string, string>("417", "Expectation Failed"),
            new KeyValuePair<string, string>("421", "Misdirected Request"),
            new KeyValuePair<string, string>("422", "Unprocessable Content"),
            new KeyValuePair<string, string>("423", "Locked"),
            new KeyValuePair<string, string>("424", "Failed Dependency"),
            new KeyValuePair<string, string>("426", "Upgrade Required"),
            new KeyValuePair<string, string>("428", "Precondition Required"),
            new KeyValuePair<string, string>("429", "Too Many Requests"),
            new KeyValuePair<string, string>("431", "Request Header Fields Too Large"),
            new KeyValuePair<string, string>("451", "Unavailable For Legal Reasons"),
            new KeyValuePair<string, string>("4\\d{2}", "Client Error"),

            new KeyValuePair<string, string>("500", "Internal Server Error"),
            new KeyValuePair<string, string>("501", "Not Implemented"),
            new KeyValuePair<string, string>("502", "Bad Gateway"),
            new KeyValuePair<string, string>("503", "Service Unavailable"),
            new KeyValuePair<string, string>("504", "Gateway Timeout"),
            new KeyValuePair<string, string>("505", "HTTP Version Not Supported"),
            new KeyValuePair<string, string>("5\\d{2}", "Server Error"),

            new KeyValuePair<string, string>("default", "Error")
        ];

        private OpenApiSchema GetSchemaForFormParameter(ApiParameterDescription apiParameterDescription, OpenApiSchema generatedSchema, OpenApiMediaType mediaType, bool modifySchema, out bool isParameter)
        {
            isParameter = false;
            if (apiParameterDescription.IsFromForm() && (generatedSchema.Reference is null
                || (apiParameterDescription.Type is not null && (Nullable.GetUnderlyingType(apiParameterDescription.Type) ?? apiParameterDescription.Type).IsEnum)))
            {
                isParameter = true;
                if (modifySchema)
                {
                    var name = _options.DescribeAllParametersInCamelCase
                    ? apiParameterDescription.Name.ToCamelCase()
                    : apiParameterDescription.Name;
                    mediaType.Encoding.Add(apiParameterDescription.Name, new OpenApiEncoding { Style = ParameterStyle.Form });
                    return new OpenApiSchema()
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>()
                        {
                            [name] = generatedSchema
                        },
                        Required = apiParameterDescription.IsRequiredParameter() ? new SortedSet<string>() { name } : null
                    };
                }
            }
            return generatedSchema;
        }

#if NET7_0_OR_GREATER
        private static string GenerateSummary(ApiDescription apiDescription) =>
            apiDescription.ActionDescriptor?.EndpointMetadata
                ?.OfType<IEndpointSummaryMetadata>()
                .Select(s => s.Summary)
                .LastOrDefault();

        private static string GenerateDescription(ApiDescription apiDescription) =>
            apiDescription.ActionDescriptor?.EndpointMetadata
                ?.OfType<IEndpointDescriptionMetadata>()
                .Select(s => s.Description)
                .LastOrDefault();
#endif
    }
}
