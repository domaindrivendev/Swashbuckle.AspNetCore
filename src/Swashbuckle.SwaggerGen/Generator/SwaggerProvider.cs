using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.SwaggerGen.Generator
{
    public class SwaggerProvider : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaRegistryFactory _schemaRegistryFactory;
        private readonly SwaggerProviderOptions _options;

        public SwaggerProvider(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaRegistryFactory schemaRegistryFactory,
            SwaggerProviderOptions options = null)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaRegistryFactory = schemaRegistryFactory;
            _options = options ?? new SwaggerProviderOptions();
        }

        public SwaggerDocument GetSwagger(
            string apiVersion,
            string host = null,
            string basePath = null,
            string[] schemes = null)
        {
            var schemaRegistry = _schemaRegistryFactory.Create();

            var info = _options.ApiVersions.FirstOrDefault(v => v.Version == apiVersion);
            if (info == null)
                throw new UnknownApiVersion(apiVersion);

            var paths = GetApiDescriptionsFor(apiVersion)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.IsObsolete()))
                .OrderBy(_options.GroupNameSelector, _options.GroupNameComparer)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));

            var swaggerDoc = new SwaggerDocument
            {
                Info = info,
                Host = host,
                BasePath = basePath,
                Schemes = schemes,
                Paths = paths,
                Definitions = schemaRegistry.Definitions,
                SecurityDefinitions = _options.SecurityDefinitions
            };

            var filterContext = new DocumentFilterContext(
                _apiDescriptionsProvider.ApiDescriptionGroups,
                null);

            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFor(string apiVersion)
        {
            var allDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items);

            return (_options.VersionSupportResolver == null)
                ? allDescriptions
                : allDescriptions.Where(apiDesc => _options.VersionSupportResolver(apiDesc, apiVersion));
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
            var groupName = _options.GroupNameSelector(apiDescription);

            var parameters = apiDescription.ParameterDescriptions
                .Where(paramDesc => paramDesc.Source.IsFromRequest)
                .Select(paramDesc => CreateParameter(paramDesc, schemaRegistry))
                .ToList();

            var responses = new Dictionary<string, Response>();
            if (!apiDescription.SupportedResponseTypes.Any())
                responses.Add("204", new Response { Description = "No Content" });
            else
            {
                var description = apiDescription.SupportedResponseTypes.OrderBy(responseType => responseType.StatusCode).FirstOrDefault();
                responses.Add("200", CreateSuccessResponse(description.Type, schemaRegistry));
            }

            var operation = new Operation
            {
                Tags = (groupName != null) ? new[] { groupName } : null,
                OperationId = apiDescription.FriendlyId(),
                Produces = apiDescription.Produces().ToList(),
                //consumes = apiDescription.Consumes().ToList(),
                Parameters = parameters.Any() ? parameters : null, // parameters can be null but not empty
                Responses = responses,
                Deprecated = apiDescription.IsObsolete()
            };

            var filterContext = new OperationFilterContext(apiDescription, schemaRegistry);
            foreach (var filter in _options.OperationFilters)
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

        private Response CreateSuccessResponse(Type responseType, ISchemaRegistry schemaRegistry)
        {
            return new Response
            {
                Description = "OK",
                Schema = (responseType != null)
                    ? schemaRegistry.GetOrRegister(responseType)
                    : null
            };
        }
    }
}