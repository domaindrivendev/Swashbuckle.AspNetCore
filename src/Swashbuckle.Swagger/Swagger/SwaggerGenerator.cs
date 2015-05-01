using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly Func<ISchemaRegistry> _schemaRegistryFactory;
        private readonly SwaggerGeneratorOptions _options;

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            Func<ISchemaRegistry> schemaRegistryFactory,
            SwaggerGeneratorOptions options = null)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaRegistryFactory = schemaRegistryFactory;
            _options = options ?? new SwaggerGeneratorOptions();
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var schemaRegistry = _schemaRegistryFactory();

            var info = _options.ApiVersions.FirstOrDefault(v => v.Version == apiVersion);
            if (info == null)
                throw new UnknownApiVersion(apiVersion);

            var paths = GetApiDescriptionsFor(apiVersion)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.IsObsolete()))
                .OrderBy(_options.GroupNameSelector, _options.GroupNameComparer)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));

            var rootUri = new Uri(rootUrl);

            var swaggerDoc = new SwaggerDocument
            {
                Info = info,
                Host = rootUri.Host + ":" + rootUri.Port,
                BasePath = (rootUri.AbsolutePath != "/") ? rootUri.AbsolutePath : null,
                Schemes = (_options.Schemes != null) ? _options.Schemes.ToList() : new[] { rootUri.Scheme }.ToList(),
                Paths = paths,
                Definitions = schemaRegistry.Definitions,
                SecurityDefinitions = _options.SecurityDefinitions
            };

            var filterContext = new DocumentFilterContext(_apiDescriptionsProvider.ApiDescriptionGroups, schemaRegistry);
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
                .GroupBy(apiDesc => apiDesc.HttpMethod.ToLower());

            foreach (var group in perMethodGrouping)
            {
                var httpMethod = group.Key;

                if (group.Count() > 1) throw new NotSupportedException(string.Format(
                    "Not supported by Swagger 2.0: Multiple operations with path '{0}' and method '{1}'.",
                    group.First().RelativePathSansQueryString(), httpMethod));

                var apiDescription = group.Single();

                switch (httpMethod)
                {
                    case "get":
                        pathItem.Get = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "put":
                        pathItem.Put = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "post":
                        pathItem.Post = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "delete":
                        pathItem.Delete = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "options":
                        pathItem.Options = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "head":
                        pathItem.Head = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "patch":
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
            if (apiDescription.ResponseType == typeof(void))
                responses.Add("204", new Response { Description = "No Content" });
            else
                responses.Add("200", CreateSuccessResponse(apiDescription.ResponseType, schemaRegistry));

            var operation = new Operation
            { 
                Tags = (groupName != null) ? new[] { groupName } : null,
                OperationId = apiDescription.ActionDescriptor.DisplayName,
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
            var schema = (paramDesc.Type != null) ? schemaRegistry.GetOrRegister(paramDesc.Type) : null;

            if (source == "body")
            {
                return new BodyParameter
                {
                    Name = paramDesc.Name,
                    In = source,
                    Required = paramDesc.IsRequired(),
                    Schema = schema
                };
            }
            else
            {
                var nonBodyParam = new NonBodyParameter
                {
                    Name = paramDesc.Name,
                    In = source,
                    Required = paramDesc.IsRequired(),
                };
                if (schema != null) nonBodyParam.PopulateFrom(schema);
                return nonBodyParam;
            }
        }

        private Response CreateSuccessResponse(Type responseType, ISchemaRegistry schemaRegistry)
        {
            return new Response
            {
                Description = "OK",
                Schema = (responseType != null) ? schemaRegistry.GetOrRegister(responseType) : null
            };
        }
    }
}