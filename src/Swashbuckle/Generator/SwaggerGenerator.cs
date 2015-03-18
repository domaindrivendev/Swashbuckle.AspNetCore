using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Description;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Schema;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Swashbuckle.Swagger
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SwaggerGeneratorOptions _options;

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            IContractResolver jsonContractResolver,
            SwaggerGeneratorOptions options = null)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _jsonContractResolver = jsonContractResolver;
            _options = options ?? new SwaggerGeneratorOptions();
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var schemaRegistry = new SchemaGenerator(_jsonContractResolver);
            //_options.CustomSchemaMappings,
            //_options.SchemaFilters,
            //_options.IgnoreObsoleteProperties,
            //_options.UseFullTypeNameInSchemaIds,
            //_options.DescribeAllEnumsAsStrings);

            Info info;
            _options.ApiVersions.TryGetValue(apiVersion, out info);
            if (info == null)
                throw new UnknownApiVersion(apiVersion);

            var paths = GetApiDescriptionsFor(apiVersion)
                .OrderBy(_options.GroupNameSelector, _options.GroupNameComparer)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));
                //.Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.IsObsolete()))

            var rootUri = new Uri(rootUrl);

            var swaggerDoc = new SwaggerDocument
            {
                info = info,
                host = rootUri.Host + ":" + rootUri.Port,
                basePath = (rootUri.AbsolutePath != "/") ? rootUri.AbsolutePath : null,
                schemes = (_options.Schemes != null) ? _options.Schemes.ToList() : new[] { rootUri.Scheme }.ToList(),
                paths = paths,
                definitions = schemaRegistry.Definitions,
                securityDefinitions = _options.SecurityDefinitions
            };

            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, schemaRegistry, _apiDescriptionsProvider.ApiDescriptionGroups);
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

        private PathItem CreatePathItem(IEnumerable<ApiDescription> apiDescriptions, SchemaGenerator schemaRegistry)
        {
            var pathItem = new PathItem();

            // Group further by http method
            var perMethodGrouping = apiDescriptions
                .GroupBy(apiDesc => apiDesc.HttpMethod.ToLower());

            foreach (var group in perMethodGrouping)
            {
                var httpMethod = group.Key;

                var apiDescription = (group.Count() == 1)
                    ? group.First()
                    : _options.ConflictingActionsResolver(group);

                switch (httpMethod)
                {
                    case "get":
                        pathItem.get = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "put":
                        pathItem.put = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "post":
                        pathItem.post = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "delete":
                        pathItem.delete = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "options":
                        pathItem.options = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "head":
                        pathItem.head = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "patch":
                        pathItem.patch = CreateOperation(apiDescription, schemaRegistry);
                        break;
                }
            }

            return pathItem;
        }

        private Operation CreateOperation(ApiDescription apiDescription, SchemaGenerator schemaRegistry)
        {
            var groupName = _options.GroupNameSelector(apiDescription);

            var parameters = apiDescription.ParameterDescriptions
                .Select(paramDesc => CreateParameter(paramDesc, schemaRegistry))
                .ToList();

            var responses = new Dictionary<string, Response>();
            var responseType = apiDescription.ResponseType;
            if (responseType == typeof(void) || responseType == null)
                responses.Add("204", new Response { description = "No Content" });
            else
                responses.Add("200", new Response { description = "OK", schema = schemaRegistry.GetOrRegister(responseType) });

            var operation = new Operation
            { 
                tags = (groupName != null) ? new[] { groupName } : null,
                operationId = apiDescription.ActionDescriptor.DisplayName,
                produces = apiDescription.Produces().ToList(),
                //consumes = apiDescription.Consumes().ToList(),
                parameters = parameters.Any() ? parameters : null, // parameters can be null but not empty
                responses = responses,
                //deprecated = apiDescription.IsObsolete()
            };

            //foreach (var filter in _options.OperationFilters)
            //{
            //    filter.Apply(operation, schemaRegistry, apiDescription);
            //}

            return operation;
        }

        private Parameter CreateParameter(ApiParameterDescription paramDesc, SchemaGenerator schemaRegistry)
        {
            var parameter = new Parameter
            {
                name = paramDesc.Name,
                @in = paramDesc.Source.DisplayName.ToLowerInvariant(),
                required = paramDesc.IsRequired()
            };

            if (paramDesc.Type == null)
            {
                parameter.type = "string";
                return parameter; 
            }

            var schema = schemaRegistry.GetOrRegister(paramDesc.Type);
            if (parameter.@in == "body")
                parameter.schema = schema;
            else
                parameter.PopulateFrom(schema);

            return parameter;
        }
    }
}