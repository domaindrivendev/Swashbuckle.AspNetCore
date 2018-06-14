using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaRegistryFactory _schemaRegistryFactory;
        private readonly SwaggerGeneratorSettings _settings;

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaRegistryFactory schemaRegistryFactory,
            SwaggerGeneratorSettings settings = null)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaRegistryFactory = schemaRegistryFactory;
            _settings = settings ?? new SwaggerGeneratorSettings();
        }

        public SwaggerDocument GetSwagger(
            string documentName,
            string host = null,
            string basePath = null,
            string[] schemes = null)
        {
            if (!_settings.SwaggerDocs.TryGetValue(documentName, out Info info))
                throw new UnknownSwaggerDocument(documentName);

            var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc => _settings.DocInclusionPredicate(documentName, apiDesc))
                .Where(apiDesc => !_settings.IgnoreObsoleteActions || !apiDesc.IsObsolete());

            var schemaRegistry = _schemaRegistryFactory.Create();

            var swaggerDoc = new SwaggerDocument
            {
                Info = info,
                Host = host,
                BasePath = basePath,
                Schemes = schemes,
                Paths = CreatePathItems(applicableApiDescriptions, schemaRegistry),
                Definitions = schemaRegistry.Definitions,
                SecurityDefinitions = _settings.SecurityDefinitions.Any() ? _settings.SecurityDefinitions : null,
                Security = _settings.SecurityRequirements.Any() ? _settings.SecurityRequirements : null
            };

            var filterContext = new DocumentFilterContext(
                _apiDescriptionsProvider.ApiDescriptionGroups,
                applicableApiDescriptions,
                schemaRegistry);

            foreach (var filter in _settings.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private Dictionary<string, PathItem> CreatePathItems(
            IEnumerable<ApiDescription> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {
            return apiDescriptions
                .OrderBy(_settings.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));
        }

        private PathItem CreatePathItem(
            IEnumerable<ApiDescription> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {
            var pathItem = new PathItem();

            // Group further by http method
            var perMethodGrouping = apiDescriptions
                .GroupBy(apiDesc => apiDesc.HttpMethod);

            foreach (var group in perMethodGrouping)
            {
                var httpMethod = group.Key;

                if (httpMethod == null)
                    throw new NotSupportedException(string.Format(
                        "Ambiguous HTTP method for action - {0}. " +
                        "Actions require an explicit HttpMethod binding for Swagger 2.0",
                        group.First().ActionDescriptor.DisplayName));

                if (group.Count() > 1 && _settings.ConflictingActionsResolver == null)
                    throw new NotSupportedException(string.Format(
                        "HTTP method \"{0}\" & path \"{1}\" overloaded by actions - {2}. " +
                        "Actions require unique method/path combination for Swagger 2.0. Use ConflictingActionsResolver as a workaround",
                        httpMethod,
                        group.First().RelativePathSansQueryString(),
                        string.Join(",", group.Select(apiDesc => apiDesc.ActionDescriptor.DisplayName))));

                var apiDescription = (group.Count() > 1) ? _settings.ConflictingActionsResolver(group) : group.Single();

                switch (httpMethod)
                {
                    case "GET":
                        pathItem.Get = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "PUT":
                        pathItem.Put = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "POST":
                        pathItem.Post = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "DELETE":
                        pathItem.Delete = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "OPTIONS":
                        pathItem.Options = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "HEAD":
                        pathItem.Head = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "PATCH":
                        pathItem.Patch = CreateOperation(apiDescription, schemaRegistry);
                        break;
                }
            }

            return pathItem;
        }

        private Operation CreateOperation(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry)
        {
            // Try to retrieve additional metadata that's not provided by ApiExplorer
            MethodInfo methodInfo;

            var customAttributes = Enumerable.Empty<object>();
            if (apiDescription.TryGetMethodInfo(out methodInfo))
            {
                customAttributes = methodInfo.GetCustomAttributes(true)
                    .Union(methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true));
            }

            var isDeprecated = customAttributes.Any(attr => attr.GetType() == typeof(ObsoleteAttribute));

            var operation = new Operation
            {
                Tags = new[] { _settings.TagSelector(apiDescription) },
                OperationId = apiDescription.FriendlyId(),
                Consumes = apiDescription.SupportedRequestMediaTypes().ToList(),
                Produces = apiDescription.SupportedResponseMediaTypes().ToList(),
                Parameters = CreateParameters(apiDescription, schemaRegistry),
                Responses = CreateResponses(apiDescription, schemaRegistry),
                Deprecated = isDeprecated ? true : (bool?)null
            };

            var filterContext = new OperationFilterContext(
                apiDescription,
                schemaRegistry,
                methodInfo);

            foreach (var filter in _settings.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }

            return operation;
        }

        private IList<IParameter> CreateParameters(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry)
        {
            var applicableParamDescriptions = apiDescription.ParameterDescriptions
                .Where(paramDesc =>
                {
                    return paramDesc.Source.IsFromRequest
                        && (paramDesc.ModelMetadata == null || paramDesc.ModelMetadata.IsBindingAllowed)
                        && !paramDesc.IsPartOfCancellationToken();
                });

            return applicableParamDescriptions
                .Select(paramDesc => CreateParameter(apiDescription, paramDesc, schemaRegistry))
                .ToList();
        }

        private IParameter CreateParameter(
            ApiDescription apiDescription,
            ApiParameterDescription apiParameterDescription,
            ISchemaRegistry schemaRegistry)
        {
            var name = _settings.DescribeAllParametersInCamelCase
                ? apiParameterDescription.Name.ToCamelCase()
                : apiParameterDescription.Name;

            var location = ParameterLocationMap.ContainsKey(apiParameterDescription.Source)
                ? ParameterLocationMap[apiParameterDescription.Source]
                : "query";

            var schema = (apiParameterDescription.Type != null)
                ? schemaRegistry.GetOrRegister(apiParameterDescription.Type)
                : null;

            // Try to retrieve additional metadata that's not provided by ApiExplorer
            ParameterInfo parameterInfo = null;
            PropertyInfo propertyInfo = null;

            var customAttributes = Enumerable.Empty<object>();
            if (apiParameterDescription.TryGetParameterInfo(apiDescription, out parameterInfo))
                customAttributes = parameterInfo.GetCustomAttributes(true);
            else if (apiParameterDescription.TryGetPropertyInfo(out propertyInfo))
                customAttributes = propertyInfo.GetCustomAttributes(true);

            var isRequired = customAttributes.Any(attr =>
                new[] { typeof(RequiredAttribute), typeof(BindRequiredAttribute) }.Contains(attr.GetType()));

            var parameter = (location == "body")
                ? new BodyParameter { Name = name, Schema = schema, Required = isRequired }
                : CreateNonBodyParameter(name, location, schema, isRequired, schemaRegistry);

            var filterContext = new ParameterFilterContext(
                apiParameterDescription,
                schemaRegistry,
                parameterInfo,
                propertyInfo);

            foreach (var filter in _settings.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private IParameter CreateNonBodyParameter(
            string name,
            string location,
            Schema schema,
            bool isRequired,
            ISchemaRegistry schemaRegistry)
        {
            var nonBodyParam = new NonBodyParameter
            {
                Name = name,
                In = location,
                Required = (location == "path") ? true : isRequired
            };

            if (schema == null)
            {
                nonBodyParam.Type = "string";
            }
            else
            {
                // In some cases (e.g. enum types), the schemaRegistry may return a reference instead of a
                // full schema. Retrieve the full schema before populating the parameter description
                var fullSchema = (schema.Ref != null)
                    ? schemaRegistry.Definitions[schema.Ref.Replace("#/definitions/", string.Empty)]
                    : schema;

                nonBodyParam.PopulateFrom(schema);
            }

            if (nonBodyParam.Type == "array")
                nonBodyParam.CollectionFormat = "multi";

            return nonBodyParam;
        }

        private IDictionary<string, Response> CreateResponses(
            ApiDescription apiDescription,
            ISchemaRegistry schemaRegistry)
        {
            var supportedApiResponseTypes = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 200 });

            return supportedApiResponseTypes
                .ToDictionary(
                    apiResponseType => apiResponseType.StatusCode.ToString(),
                    apiResponseType => CreateResponse(apiResponseType, schemaRegistry));
        }

        private Response CreateResponse(ApiResponseType apiResponseType, ISchemaRegistry schemaRegistry)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(apiResponseType.StatusCode.ToString(), entry.Key))
                .Value;

            return new Response
            {
                Description = description,
                Schema = (apiResponseType.Type != null && apiResponseType.Type != typeof(void))
                    ? schemaRegistry.GetOrRegister(apiResponseType.Type)
                    : null
            };
        }

        private static Dictionary<BindingSource, string> ParameterLocationMap = new Dictionary<BindingSource, string>
        {
            { BindingSource.Form, "formData" },
            { BindingSource.Body, "body" },
            { BindingSource.Header, "header" },
            { BindingSource.Path, "path" },
            { BindingSource.Query, "query" }
        };

        private static readonly Dictionary<string, string> ResponseDescriptionMap = new Dictionary<string, string>
        {
            { "1\\d{2}", "Information" },
            { "2\\d{2}", "Success" },
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