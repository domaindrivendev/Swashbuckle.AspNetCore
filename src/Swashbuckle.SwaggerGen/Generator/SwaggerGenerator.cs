using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.Generator
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
            var schemaRegistry = _schemaRegistryFactory.Create();

            SwaggerDocumentDescriptor documentDescriptor;
            if (!_settings.SwaggerDocs.TryGetValue(documentName, out documentDescriptor))
                throw new UnknownSwaggerDocument(documentName);

            var apiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc => documentDescriptor.IncludeActionPredicate(apiDesc))
                .Where(apiDesc => !(_settings.IgnoreObsoleteActions && apiDesc.IsObsolete()))
                .OrderBy(_settings.GroupNameSelector, _settings.GroupNameComparer);

            var paths = apiDescriptions
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));

            var swaggerDoc = new SwaggerDocument
            {
                Info = documentDescriptor.Info,
                Host = host,
                BasePath = basePath,
                Schemes = schemes,
                Paths = paths,
                Definitions = schemaRegistry.Definitions,
                SecurityDefinitions = _settings.SecurityDefinitions
            };

            var filterContext = new DocumentFilterContext(
                _apiDescriptionsProvider.ApiDescriptionGroups,
                schemaRegistry);

            foreach (var filter in _settings.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private PathItem CreatePathItem(IEnumerable<ApiDescription> apiDescriptions, ISchemaRegistry schemaRegistry)
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
                        "Unbounded HTTP verbs for path '{0}'. Are you missing an HttpMethodAttribute?",
                        group.First().RelativePathSansQueryString()));

                if (group.Count() > 1)
                    throw new NotSupportedException(string.Format(
                        "Multiple operations with path '{0}' and method '{1}'. Are you overloading action methods?",
                        group.First().RelativePathSansQueryString(), httpMethod));

                var apiDescription = group.Single();

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

        private Operation CreateOperation(ApiDescription apiDescription, ISchemaRegistry schemaRegistry)
        {
            var groupName = _settings.GroupNameSelector(apiDescription);

            var parameters = apiDescription.ParameterDescriptions
                .Where(paramDesc => paramDesc.Source.IsFromRequest)
                .Select(paramDesc => CreateParameter(paramDesc, schemaRegistry))
                .ToList();

            var responses = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 200 })
                .ToDictionary(
                    apiResponseType => apiResponseType.StatusCode.ToString(),
                    apiResponseType => CreateResponse(apiResponseType, schemaRegistry)
                 );

            var operation = new Operation
            {
                Tags = (groupName != null) ? new[] { groupName } : null,
                OperationId = apiDescription.FriendlyId(),
                Consumes = apiDescription.SupportedRequestMediaTypes().ToList(),
                Produces = apiDescription.SupportedResponseMediaTypes().ToList(),
                Parameters = parameters.Any() ? parameters : null, // parameters can be null but not empty
                Responses = responses,
                Deprecated = apiDescription.IsObsolete()
            };

            var filterContext = new OperationFilterContext(apiDescription, schemaRegistry);
            foreach (var filter in _settings.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }

            return operation;
        }

        private IParameter CreateParameter(ApiParameterDescription paramDesc, ISchemaRegistry schemaRegistry)
        {
            var source = paramDesc.Source.Id.ToLower();
            var schema = (paramDesc.Type == null) ? null : schemaRegistry.GetOrRegister(paramDesc.Type);

            if (source == "body")
            {
                return new BodyParameter
                {
                    Name = paramDesc.Name,
                    In = source,
                    Schema = schema
                };
            }
            else
            {
                var nonBodyParam = new NonBodyParameter
                {
                    Name = paramDesc.Name,
                    In = source,
                    Required = (source == "path")
                };

                if (schema == null)
                    nonBodyParam.Type = "string";
                else
                    nonBodyParam.PopulateFrom(schema);

                if (nonBodyParam.Type == "array")
                    nonBodyParam.CollectionFormat = "multi";

                return nonBodyParam;
            }
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
