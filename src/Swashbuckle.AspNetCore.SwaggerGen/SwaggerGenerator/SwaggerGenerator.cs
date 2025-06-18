using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models.References;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerGenerator(
    SwaggerGeneratorOptions options,
    IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
    ISchemaGenerator schemaGenerator) : ISwaggerProvider, IAsyncSwaggerProvider, ISwaggerDocumentMetadataProvider
{
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider = apiDescriptionsProvider;
    private readonly ISchemaGenerator _schemaGenerator = schemaGenerator;
    private readonly SwaggerGeneratorOptions _options = options ?? new();
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

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
        var (filterContext, document) = GetSwaggerDocumentWithoutPaths(documentName, host, basePath);

        document.Paths = await GeneratePathsAsync(document, filterContext.ApiDescriptions, filterContext.SchemaRepository);

        // See https://github.com/microsoft/OpenAPI.NET/issues/2300#issuecomment-2775307399
        foreach (var scheme in await GetSecuritySchemesAsync())
        {
            document.AddComponent(scheme.Key, scheme.Value);
        }

        if (_options.SecurityRequirements is { Count: > 0 } requirements)
        {
            foreach (var requirement in requirements)
            {
                document.Security ??= [];
                document.Security.Add(requirement(document));
            }
        }

        foreach (var filter in _options.DocumentAsyncFilters)
        {
            await filter.ApplyAsync(document, filterContext, CancellationToken.None);
        }

        foreach (var filter in _options.DocumentFilters)
        {
            filter.Apply(document, filterContext);
        }

        SortDocument(document);

        return document;
    }

    public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
    {
        try
        {
            var (filterContext, document) = GetSwaggerDocumentWithoutPaths(documentName, host, basePath);

            document.Paths = GeneratePaths(document, filterContext.ApiDescriptions, filterContext.SchemaRepository);

            // See https://github.com/microsoft/OpenAPI.NET/issues/2300#issuecomment-2775307399
            foreach (var scheme in GetSecuritySchemesAsync().Result)
            {
                document.AddComponent(scheme.Key, scheme.Value);
            }

            if (_options.SecurityRequirements is { Count: > 0 } requirements)
            {
                foreach (var requirement in requirements)
                {
                    document.Security.Add(requirement(document));
                }
            }

            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(document, filterContext);
            }

            SortDocument(document);

            return document;
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

    public IList<string> GetDocumentNames() => [.. _options.SwaggerDocs.Keys];

    private void SortDocument(OpenApiDocument document)
    {
        if (document.Components?.Schemas?.Count > 1)
        {
            document.Components.Schemas =
                new SortedDictionary<string, IOpenApiSchema>(document.Components.Schemas, _options.SchemaComparer)
                .ToDictionary((k) => k.Key, (v) => v.Value);
        }

        foreach (var schema in document.Components.Schemas.Values)
        {
            SortSchema(schema);
        }

        static void SortSchema(IOpenApiSchema schema)
        {
            if (schema is OpenApiSchema concrete)
            {
                if (concrete.Required is { Count: > 1 } required)
                {
                    concrete.Required = [.. new SortedSet<string>(required)];
                }

                if (concrete.AllOf is { Count: > 0 } allOf)
                {
                    foreach (var child in allOf)
                    {
                        SortSchema(child);
                    }
                }

                if (concrete.AnyOf is { Count: > 0 } anyOf)
                {
                    foreach (var child in anyOf)
                    {
                        SortSchema(child);
                    }
                }

                if (concrete.OneOf is { Count: > 0 } oneOf)
                {
                    foreach (var child in oneOf)
                    {
                        SortSchema(child);
                    }
                }
            }
        }
    }

    private (DocumentFilterContext, OpenApiDocument) GetSwaggerDocumentWithoutPaths(string documentName, string host = null, string basePath = null)
    {
        if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
        {
            throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select((p) => p.Key));
        }

        var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
            .SelectMany((p) => p.Items)
            .Where((p) =>
            {
                var attributes = p.CustomAttributes().ToList();
                return !(_options.IgnoreObsoleteActions && attributes.OfType<ObsoleteAttribute>().Any()) &&
                       !attributes.OfType<SwaggerIgnoreAttribute>().Any() &&
                       _options.DocInclusionPredicate(documentName, p);
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
        };

        return (new DocumentFilterContext(applicableApiDescriptions, _schemaGenerator, schemaRepository), swaggerDoc);
    }

    private async Task<IDictionary<string, IOpenApiSecurityScheme>> GetSecuritySchemesAsync()
    {
        if (!_options.InferSecuritySchemes)
        {
            return new Dictionary<string, IOpenApiSecurityScheme>(_options.SecuritySchemes);
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
            .Where((scheme) => scheme.Name == "Bearer")
            .ToDictionary(
                (scheme) => scheme.Name,
                (scheme) => new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                } as IOpenApiSecurityScheme);
    }

    private List<OpenApiServer> GenerateServers(string host, string basePath)
    {
        if (_options.Servers.Count > 0)
        {
            return [.. _options.Servers];
        }

        return host == null && basePath == null
            ? []
            : [new() { Url = $"{host}{basePath}" }];
    }

    private async Task<OpenApiPaths> GeneratePathsAsync(
        OpenApiDocument document,
        IEnumerable<ApiDescription> apiDescriptions,
        SchemaRepository schemaRepository,
        Func<OpenApiDocument, IGrouping<string, ApiDescription>, SchemaRepository, Task<Dictionary<HttpMethod, OpenApiOperation>>> operationsGenerator)
    {
        var apiDescriptionsByPath = apiDescriptions
            .OrderBy(_options.SortKeySelector)
            .GroupBy(_options.PathGroupSelector);

        var paths = new OpenApiPaths();
        foreach (var group in apiDescriptionsByPath)
        {
            paths.Add(
                $"/{group.Key}",
                new OpenApiPathItem
                {
                    Operations = await operationsGenerator(document, group, schemaRepository)
                });
        }

        return paths;
    }

    private OpenApiPaths GeneratePaths(
        OpenApiDocument document,
        IEnumerable<ApiDescription> apiDescriptions,
        SchemaRepository schemaRepository)
    {
        return GeneratePathsAsync(
            document,
            apiDescriptions,
            schemaRepository,
            (document, group, schemaRepository) => Task.FromResult(GenerateOperations(document, group, schemaRepository))).Result;
    }

    private async Task<OpenApiPaths> GeneratePathsAsync(
        OpenApiDocument document,
        IEnumerable<ApiDescription> apiDescriptions,
        SchemaRepository schemaRepository)
    {
        return await GeneratePathsAsync(
            document,
            apiDescriptions,
            schemaRepository,
            GenerateOperationsAsync);
    }

    private IEnumerable<(HttpMethod, ApiDescription)> GetOperationsGroupedByMethod(
        IEnumerable<ApiDescription> apiDescriptions)
    {
        return apiDescriptions
            .OrderBy(_options.SortKeySelector)
            .GroupBy((p) => p.HttpMethod)
            .Select(PrepareGenerateOperation);
    }

    private Dictionary<HttpMethod, OpenApiOperation> GenerateOperations(
        OpenApiDocument document,
        IEnumerable<ApiDescription> apiDescriptions,
        SchemaRepository schemaRepository)
    {
        var apiDescriptionsByMethod = GetOperationsGroupedByMethod(apiDescriptions);
        var operations = new Dictionary<HttpMethod, OpenApiOperation>();

        foreach ((var operationType, var description) in apiDescriptionsByMethod)
        {
            operations.Add(operationType, GenerateOperation(document, description, schemaRepository));
        }

        return operations;
    }

    private async Task<Dictionary<HttpMethod, OpenApiOperation>> GenerateOperationsAsync(
        OpenApiDocument document,
        IEnumerable<ApiDescription> apiDescriptions,
        SchemaRepository schemaRepository)
    {
        var apiDescriptionsByMethod = GetOperationsGroupedByMethod(apiDescriptions);
        var operations = new Dictionary<HttpMethod, OpenApiOperation>();

        foreach ((var operationType, var description) in apiDescriptionsByMethod)
        {
            operations.Add(operationType, await GenerateOperationAsync(document, description, schemaRepository));
        }

        return operations;
    }

    private (HttpMethod OperationType, ApiDescription ApiDescription) PrepareGenerateOperation(IGrouping<string, ApiDescription> group)
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
                "Actions require a unique method/path combination for Swagger/OpenAPI 2.0 and 3.0. Use ConflictingActionsResolver as a workaround or provide your own implementation of PathGroupSelector.",
                httpMethod,
                group.First().RelativePath,
                string.Join(", ", group.Select((p) => p.ActionDescriptor.DisplayName))));
        }

        var apiDescription =
            count > 1 ?
            _options.ConflictingActionsResolver(group) :
            group.Single();

        if (!OperationTypeMap.TryGetValue(httpMethod, out var operationType))
        {
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2600 and
            // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2740.
            throw new SwaggerGeneratorException($"The \"{httpMethod}\" HTTP method is not supported.");
        }

        return (operationType, apiDescription);
    }

    private async Task<OpenApiOperation> GenerateOperationAsync(
        OpenApiDocument document,
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        Func<ApiDescription, SchemaRepository, OpenApiDocument, Task<List<IOpenApiParameter>>> parametersGenerator,
        Func<ApiDescription, SchemaRepository, OpenApiDocument, Task<IOpenApiRequestBody>> bodyGenerator,
        Func<OpenApiOperation, OperationFilterContext, Task> applyFilters)
    {
        var operation = await GenerateOpenApiOperationFromMetadataAsync(apiDescription, schemaRepository, document);

        try
        {
            operation ??= new OpenApiOperation
            {
                Tags = GenerateOperationTags(document, apiDescription),
                OperationId = _options.OperationIdSelector(apiDescription),
                Parameters = await parametersGenerator(apiDescription, schemaRepository, document),
                RequestBody = await bodyGenerator(apiDescription, schemaRepository, document),
                Responses = GenerateResponses(apiDescription, schemaRepository),
                Deprecated = apiDescription.CustomAttributes().OfType<ObsoleteAttribute>().Any(),
                Summary = GenerateSummary(apiDescription),
                Description = GenerateDescription(apiDescription),
            };

            apiDescription.TryGetMethodInfo(out MethodInfo methodInfo);
            var filterContext = new OperationFilterContext(apiDescription, _schemaGenerator, schemaRepository, document, methodInfo);

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

    private OpenApiOperation GenerateOperation(OpenApiDocument document, ApiDescription apiDescription, SchemaRepository schemaRepository)
    {
        return GenerateOperationAsync(
            document,
            apiDescription,
            schemaRepository,
            (description, repository, document) => Task.FromResult(GenerateParameters(description, repository, document)),
            (description, repository, document) => Task.FromResult(GenerateRequestBody(description, repository, document)),
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
        OpenApiDocument document,
        ApiDescription apiDescription,
        SchemaRepository schemaRepository)
    {
        return await GenerateOperationAsync(
            document,
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

    private async Task<OpenApiOperation> GenerateOpenApiOperationFromMetadataAsync(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        var metadata = apiDescription.ActionDescriptor?.EndpointMetadata;
        var operation = metadata?.OfType<OpenApiOperation>().SingleOrDefault();

        if (operation is null)
        {
            return null;
        }

        // Schemas will be generated via Swashbuckle by default.
        foreach (var parameter in operation.Parameters ?? [])
        {
            var apiParameter = apiDescription.ParameterDescriptions.SingleOrDefault((p) => p.Name == parameter.Name && !p.IsFromBody() && !p.IsFromForm() && !p.IsIllegalHeaderParameter());
            if (apiParameter is not null)
            {
                var (parameterAndContext, filterContext) = GenerateParameterAndContext(apiParameter, schemaRepository, document);

                if (parameter is OpenApiParameter concrete)
                {
                    concrete.Name = parameterAndContext.Name;
                    concrete.Schema = parameterAndContext.Schema;
                }

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

        var requestContentTypes = operation.RequestBody?.Content?.Keys;
        if (requestContentTypes is not null)
        {
            foreach (var contentType in requestContentTypes)
            {
                var contentTypeValue = operation.RequestBody.Content[contentType];
                var fromFormParameters = apiDescription.ParameterDescriptions.Where((p) => p.IsFromForm()).ToList();
                ApiParameterDescription bodyParameterDescription = null;
                if (fromFormParameters.Count > 0)
                {
                    var generatedContentTypeValue = GenerateRequestBodyFromFormParameters(
                        apiDescription,
                        schemaRepository,
                        fromFormParameters,
                        [contentType]).Content[contentType];

                    contentTypeValue.Schema = generatedContentTypeValue.Schema;
                    contentTypeValue.Encoding = generatedContentTypeValue.Encoding;
                }
                else
                {
                    bodyParameterDescription = apiDescription.ParameterDescriptions.SingleOrDefault((p) => p.IsFromBody());
                    if (bodyParameterDescription is not null)
                    {
                        contentTypeValue.Schema = GenerateSchema(
                            bodyParameterDescription.ModelMetadata.ModelType,
                            schemaRepository,
                            bodyParameterDescription.PropertyInfo(),
                            bodyParameterDescription.ParameterInfo());
                    }
                }

                if (fromFormParameters.Count > 0 || bodyParameterDescription is not null)
                {
                    var filterContext = new RequestBodyFilterContext(
                        bodyParameterDescription: bodyParameterDescription,
                        formParameterDescriptions: bodyParameterDescription is null ? fromFormParameters : null,
                        schemaGenerator: _schemaGenerator,
                        schemaRepository: schemaRepository,
                        document);

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

        if (operation.Responses is { Count: > 0 } responses)
        {
            foreach (var kvp in responses)
            {
                var response = kvp.Value;
                var responseModel = apiDescription.SupportedResponseTypes.SingleOrDefault((p) => p.StatusCode.ToString() == kvp.Key);
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
        }

        return operation;
    }

    private HashSet<OpenApiTagReference> GenerateOperationTags(OpenApiDocument document, ApiDescription apiDescription)
    {
        // The tags must be present at the document level for the tag references
        // to be serialized correctly at the operation level, so we need to add
        // them to the document before adding the references to the operation.
        // See https://github.com/microsoft/OpenAPI.NET/issues/2319.
        string[] names = [.. _options.TagsSelector(apiDescription)];

        if (names.Length > 0)
        {
            document.Tags ??= [];
            foreach (var name in names)
            {
                document.Tags.Add(new OpenApiTag { Name = name });
            }
        }

        return [.. names.Select((name) => new OpenApiTagReference(name, document))];
    }

    private static async Task<List<IOpenApiParameter>> GenerateParametersAsync(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        OpenApiDocument document,
        Func<ApiParameterDescription, SchemaRepository, OpenApiDocument, Task<OpenApiParameter>> parameterGenerator)
    {
        if (apiDescription.ParameterDescriptions.Any(IsFromFormAttributeUsedWithIFormFile))
        {
            throw new SwaggerGeneratorException(string.Format(
                   "Error reading parameter(s) for action {0} as [FromForm] attribute used with IFormFile. " +
                   "Please refer to https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/master/docs/configure-and-customize-swaggergen.md#handle-forms-and-file-uploads for more information",
                   apiDescription.ActionDescriptor.DisplayName));
        }

        var applicableApiParameters = apiDescription.ParameterDescriptions
            .Where(apiParam =>
            {
                return !apiParam.IsFromBody() && !apiParam.IsFromForm()
                    && !apiParam.CustomAttributes().OfType<BindNeverAttribute>().Any()
                    && !apiParam.CustomAttributes().OfType<SwaggerIgnoreAttribute>().Any()
                    && (apiParam.ModelMetadata == null || apiParam.ModelMetadata.IsBindingAllowed)
                    && !apiParam.IsIllegalHeaderParameter();
            });

        var parameters = new List<IOpenApiParameter>();

        foreach (var parameter in applicableApiParameters)
        {
            parameters.Add(await parameterGenerator(parameter, schemaRepository, document));
        }

        return parameters;
    }

    private List<IOpenApiParameter> GenerateParameters(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        return GenerateParametersAsync(
            apiDescription,
            schemaRepository,
            document,
            (parameter, schemaRepository, document) => Task.FromResult(GenerateParameter(parameter, schemaRepository, document))).Result;
    }

    private async Task<List<IOpenApiParameter>> GenerateParametersAsync(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        return await GenerateParametersAsync(
            apiDescription,
            schemaRepository,
            document,
            GenerateParameterAsync);
    }

    private OpenApiParameter GenerateParameterWithoutFilter(
        ApiParameterDescription apiParameter,
        SchemaRepository schemaRepository)
    {
        var name = _options.DescribeAllParametersInCamelCase
            ? apiParameter.Name.ToCamelCase()
            : apiParameter.Name;

        var location =
            apiParameter.Source != null &&
            ParameterLocationMap.TryGetValue(apiParameter.Source, out var value)
            ? value
            : ParameterLocation.Query;

        var isRequired = apiParameter.IsRequiredParameter();

        var type = apiParameter.ModelMetadata?.ModelType;

        if (type is not null &&
            type == typeof(string) &&
            apiParameter.Type is not null &&
            (Nullable.GetUnderlyingType(apiParameter.Type) ?? apiParameter.Type).IsEnum)
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
            : new OpenApiSchema { Type = JsonSchemaTypes.String };

        var description = schema.Description;
        if (string.IsNullOrEmpty(description) &&
            schema is OpenApiSchemaReference reference &&
            !string.IsNullOrEmpty(reference.Reference.Id) &&
            schemaRepository.Schemas.TryGetValue(reference.Reference.Id, out var openApiSchema))
        {
            description = openApiSchema.Description;
        }

        return new OpenApiParameter
        {
            Name = name,
            In = location,
            Required = isRequired,
            Schema = schema,
            Description = description,
            Style = GetParameterStyle(type, apiParameter.Source)
        };
    }

    private static ParameterStyle? GetParameterStyle(Type type, BindingSource source)
    {
        return
            source == BindingSource.Query &&
            type?.IsGenericType == true &&
            typeof(IEnumerable<KeyValuePair<string, string>>).IsAssignableFrom(type)
            ? ParameterStyle.DeepObject
            : null;
    }

    private (OpenApiParameter, ParameterFilterContext) GenerateParameterAndContext(
        ApiParameterDescription apiParameter,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        var parameter = GenerateParameterWithoutFilter(apiParameter, schemaRepository);

        var context = new ParameterFilterContext(
            apiParameter,
            _schemaGenerator,
            schemaRepository,
            document,
            apiParameter.PropertyInfo(),
            apiParameter.ParameterInfo());

        return (parameter, context);
    }

    private OpenApiParameter GenerateParameter(
        ApiParameterDescription apiParameter,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        var (parameter, filterContext) = GenerateParameterAndContext(apiParameter, schemaRepository, document);

        foreach (var filter in _options.ParameterFilters)
        {
            filter.Apply(parameter, filterContext);
        }

        return parameter;
    }

    private async Task<OpenApiParameter> GenerateParameterAsync(
        ApiParameterDescription apiParameter,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        var (parameter, filterContext) = GenerateParameterAndContext(apiParameter, schemaRepository, document);

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

    private IOpenApiSchema GenerateSchema(
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

    private (IOpenApiRequestBody RequestBody, RequestBodyFilterContext FilterContext) GenerateRequestBodyAndFilterContext(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        OpenApiRequestBody requestBody = null;
        RequestBodyFilterContext filterContext = null;

        var bodyParameter = apiDescription.ParameterDescriptions
            .FirstOrDefault((p) => p.IsFromBody());

        var formParameters = apiDescription.ParameterDescriptions
            .Where((p) => p.IsFromForm())
            .ToList();

        if (bodyParameter != null)
        {
            requestBody = GenerateRequestBodyFromBodyParameter(apiDescription, schemaRepository, bodyParameter);

            filterContext = new RequestBodyFilterContext(
                bodyParameterDescription: bodyParameter,
                formParameterDescriptions: null,
                schemaGenerator: _schemaGenerator,
                schemaRepository: schemaRepository,
                document);
        }
        else if (formParameters.Count > 0)
        {
            requestBody = GenerateRequestBodyFromFormParameters(apiDescription, schemaRepository, formParameters, null);

            filterContext = new RequestBodyFilterContext(
                bodyParameterDescription: null,
                formParameterDescriptions: formParameters,
                schemaGenerator: _schemaGenerator,
                schemaRepository: schemaRepository,
                document);
        }

        return (requestBody, filterContext);
    }

    private IOpenApiRequestBody GenerateRequestBody(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        var (requestBody, filterContext) = GenerateRequestBodyAndFilterContext(apiDescription, schemaRepository, document);

        if (requestBody != null)
        {
            foreach (var filter in _options.RequestBodyFilters)
            {
                filter.Apply(requestBody, filterContext);
            }
        }

        return requestBody;
    }

    private async Task<IOpenApiRequestBody> GenerateRequestBodyAsync(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        OpenApiDocument document)
    {
        var (requestBody, filterContext) = GenerateRequestBodyAndFilterContext(apiDescription, schemaRepository, document);

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
            Required = isRequired,
            Content = contentTypes.ToDictionary(
                (contentType) => contentType,
                (contentType) => new OpenApiMediaType
                {
                    Schema = schema
                }),
        };
    }

    private static IEnumerable<string> InferRequestContentTypes(ApiDescription apiDescription)
    {
        // If there's content types explicitly specified via ConsumesAttribute, use them
        var explicitContentTypes = apiDescription
            .CustomAttributes()
            .OfType<ConsumesAttribute>()
            .SelectMany((p) => p.ContentTypes)
            .Distinct();

        if (explicitContentTypes.Any())
        {
            return explicitContentTypes;
        }

        // If there's content types surfaced by ApiExplorer, use them
        return apiDescription.SupportedRequestFormats
            .Select((format) => format.MediaType)
            .Where((p) => p != null)
            .Distinct();
    }

    private OpenApiRequestBody GenerateRequestBodyFromFormParameters(
        ApiDescription apiDescription,
        SchemaRepository schemaRepository,
        IEnumerable<ApiParameterDescription> formParameters,
        IEnumerable<string> contentTypes)
    {
        if (contentTypes is null)
        {
            contentTypes = InferRequestContentTypes(apiDescription);
            contentTypes = contentTypes.Any() ? contentTypes : ["multipart/form-data"];
        }

        var schema = GenerateSchemaFromFormParameters(formParameters, schemaRepository);

        var totalProperties = schema.AllOf
            ?.FirstOrDefault((p) => p.Properties?.Count > 0)
            ?.Properties ?? schema.Properties;

        return new OpenApiRequestBody
        {
            Content = contentTypes.ToDictionary(
                (contentType) => contentType,
                (contentType) => new OpenApiMediaType
                {
                    Schema = schema,
                    Encoding = totalProperties?.ToDictionary(
                        (entry) => entry.Key,
                        (entry) => new OpenApiEncoding { Style = ParameterStyle.Form }
                    ) ?? []
                })
        };
    }

    private IOpenApiSchema GenerateSchemaFromFormParameters(
        IEnumerable<ApiParameterDescription> formParameters,
        SchemaRepository schemaRepository)
    {
        var properties = new Dictionary<string, IOpenApiSchema>();
        var requiredPropertyNames = new List<string>();
        var ownSchemas = new List<IOpenApiSchema>();

        foreach (var formParameter in formParameters)
        {
            var propertyInfo = formParameter.PropertyInfo();
            if (!propertyInfo?.HasAttribute<SwaggerIgnoreAttribute>() ?? true)
            {
                var schema =
                    formParameter.ModelMetadata != null
                    ? GenerateSchema(
                        formParameter.ModelMetadata.ModelType,
                        schemaRepository,
                        propertyInfo,
                        formParameter.ParameterInfo())
                    : new OpenApiSchema { Type = JsonSchemaTypes.String };

                if (schema is not OpenApiSchemaReference ||
                    (formParameter.ModelMetadata?.ModelType is not null && (Nullable.GetUnderlyingType(formParameter.ModelMetadata.ModelType) ?? formParameter.ModelMetadata.ModelType).IsEnum))
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
            bool isAllOf =
                ownSchemas.Count > 1 ||
                (ownSchemas.Count > 0 && properties.Count > 0);

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

        static OpenApiSchema GenerateSchemaForProperties(Dictionary<string, IOpenApiSchema> properties, List<string> requiredPropertyNames) =>
             new()
             {
                 Type = JsonSchemaTypes.Object,
                 Properties = properties,
                 Required = [.. new SortedSet<string>(requiredPropertyNames)],
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
        string description = null;

#if NET10_0_OR_GREATER
        description = apiResponseType.Description;
#endif

        if (string.IsNullOrEmpty(description))
        {
            description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(statusCode, entry.Key))
                .Value;
        }

        var responseContentTypes = InferResponseContentTypes(apiDescription, apiResponseType);

        return new OpenApiResponse
        {
            Description = description,
            Content = responseContentTypes.ToDictionary(
                (contentType) => contentType,
                (contentType) => CreateResponseMediaType(apiResponseType.ModelMetadata?.ModelType ?? apiResponseType.Type, schemaRepository)
            )
        };
    }

    private static IEnumerable<string> InferResponseContentTypes(ApiDescription apiDescription, ApiResponseType apiResponseType)
    {
        // If there's no associated model type, return an empty list (i.e. no content)
        if (apiResponseType.ModelMetadata == null &&
            (apiResponseType.Type == null || apiResponseType.Type == typeof(void)))
        {
            return [];
        }

        // If there's content types explicitly specified via ProducesAttribute, use them
        var explicitContentTypes = apiDescription.CustomAttributes().OfType<ProducesAttribute>()
            .SelectMany((p) => p.ContentTypes)
            .Distinct();

        if (explicitContentTypes.Any())
        {
            return explicitContentTypes;
        }

        // If there's content types surfaced by ApiExplorer, use them
        return [.. apiResponseType.ApiResponseFormats
            .Select((responseFormat) => responseFormat.MediaType)
            .Distinct()];
    }

    private OpenApiMediaType CreateResponseMediaType(Type modelType, SchemaRepository schemaRepository)
    {
        return new OpenApiMediaType
        {
            Schema = GenerateSchema(modelType, schemaRepository)
        };
    }

    private static bool IsFromFormAttributeUsedWithIFormFile(ApiParameterDescription apiParameter)
    {
        var parameterInfo = apiParameter.ParameterInfo();
        var fromFormAttribute = parameterInfo?.GetCustomAttribute<FromFormAttribute>();

        return fromFormAttribute != null && parameterInfo?.ParameterType == typeof(IFormFile);
    }

    private static readonly Dictionary<string, HttpMethod> OperationTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GET"] = HttpMethod.Get,
        ["PUT"] = HttpMethod.Put,
        ["POST"] = HttpMethod.Post,
        ["DELETE"] = HttpMethod.Delete,
        ["OPTIONS"] = HttpMethod.Options,
        ["HEAD"] = HttpMethod.Head,
        ["PATCH"] = HttpMethod.Patch,
        ["TRACE"] = HttpMethod.Trace,
    };

    private static readonly Dictionary<BindingSource, ParameterLocation> ParameterLocationMap = new()
    {
        [BindingSource.Query] = ParameterLocation.Query,
        [BindingSource.Header] = ParameterLocation.Header,
        [BindingSource.Path] = ParameterLocation.Path,
    };

    private static readonly IReadOnlyCollection<KeyValuePair<string, string>> ResponseDescriptionMap =
    [
        // Informational responses
        new("100", "Continue"),
        new("101", "Switching Protocols"),
        new("102", "Processing"),
        new("103", "Early Hints"),
        new("1\\d{2}", "Information"),

        // Successful responses
        new("200", "OK"),
        new("201", "Created"),
        new("202", "Accepted"),
        new("203", "Non-Authoritative Information"),
        new("204", "No Content"),
        new("205", "Reset Content"),
        new("206", "Partial Content"),
        new("207", "Multi-Status"),
        new("208", "Already Reported"),
        new("226", "IM Used"),
        new("2\\d{2}", "Success"),

        // Redirection messages
        new("300", "Multiple Choices"),
        new("301", "Moved Permanently"),
        new("302", "Found"),
        new("303", "See Other"),
        new("304", "Not Modified"),
        new("305", "Use Proxy"),
        new("307", "Temporary Redirect"),
        new("308", "Permanent Redirect"),
        new("3\\d{2}", "Redirect"),

        // Client error responses
        new("400", "Bad Request"),
        new("401", "Unauthorized"),
        new("402", "Payment Required"),
        new("403", "Forbidden"),
        new("404", "Not Found"),
        new("405", "Method Not Allowed"),
        new("406", "Not Acceptable"),
        new("407", "Proxy Authentication Required"),
        new("408", "Request Timeout"),
        new("409", "Conflict"),
        new("410", "Gone"),
        new("411", "Length Required"),
        new("412", "Precondition Failed"),
        new("413", "Content Too Large"),
        new("414", "URI Too Long"),
        new("415", "Unsupported Media Type"),
        new("416", "Range Not Satisfiable"),
        new("417", "Expectation Failed"),
        new("418", "I'm a teapot"),
        new("421", "Misdirected Request"),
        new("422", "Unprocessable Content"),
        new("423", "Locked"),
        new("424", "Failed Dependency"),
        new("425", "Too Early"),
        new("426", "Upgrade Required"),
        new("428", "Precondition Required"),
        new("429", "Too Many Requests"),
        new("431", "Request Header Fields Too Large"),
        new("451", "Unavailable For Legal Reasons"),
        new("4\\d{2}", "Client Error"),

        // Server error responses
        new("500", "Internal Server Error"),
        new("501", "Not Implemented"),
        new("502", "Bad Gateway"),
        new("503", "Service Unavailable"),
        new("504", "Gateway Timeout"),
        new("505", "HTTP Version Not Supported"),
        new("506", "Variant Also Negotiates"),
        new("507", "Insufficient Storage"),
        new("508", "Loop Detected"),
        new("510", "Not Extended"),
        new("511", "Network Authentication Required"),
        new("5\\d{2}", "Server Error"),

        new("default", "Error")
    ];

    private static string GenerateSummary(ApiDescription apiDescription) =>
        apiDescription.ActionDescriptor?.EndpointMetadata
            ?.OfType<IEndpointSummaryMetadata>()
            .Select((p) => p.Summary)
            .LastOrDefault();

    private static string GenerateDescription(ApiDescription apiDescription) =>
        apiDescription.ActionDescriptor?.EndpointMetadata
            ?.OfType<IEndpointDescriptionMetadata>()
            .Select((p) => p.Description)
            .LastOrDefault();
}
