using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ApiExplorer;

namespace Swashbuckle.Swagger
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaProvider _schemaProvider;
        private readonly SwaggerGeneratorOptions _options;

        public SwaggerGenerator(
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            ISchemaProvider schemaProvider,
            SwaggerGeneratorOptions options = null)
        {
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaProvider = schemaProvider;
            _options = options ?? new SwaggerGeneratorOptions();
        }

        public SwaggerDocument GetSwagger(
            string apiVersion,
            string host = null,
            string basePath = null,
            string[] schemes = null)
        {
            var schemaDefinitions = new Dictionary<string, Schema>();

            var info = _options.ApiVersions.FirstOrDefault(v => v.Version == apiVersion);
            if (info == null)
                throw new UnknownApiVersion(apiVersion);

            var paths = GetApiDescriptionsFor(apiVersion)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.IsObsolete()))
                .OrderBy(_options.GroupNameSelector, _options.GroupNameComparer)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaDefinitions));

            var swaggerDoc = new SwaggerDocument
            {
                Info = info,
                Host = host,
                BasePath = basePath,
                Schemes = schemes,
                Paths = paths,
                Definitions = schemaDefinitions,
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

        private PathItem CreatePathItem(
            IEnumerable<ApiDescription> apiDescriptions,
            IDictionary<string, Schema> schemaDefinitions)
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
                        pathItem.Get = CreateOperation(apiDescription, schemaDefinitions);
                        break;
                    case "put":
                        pathItem.Put = CreateOperation(apiDescription, schemaDefinitions);
                        break;
                    case "post":
                        pathItem.Post = CreateOperation(apiDescription, schemaDefinitions);
                        break;
                    case "delete":
                        pathItem.Delete = CreateOperation(apiDescription, schemaDefinitions);
                        break;
                    case "options":
                        pathItem.Options = CreateOperation(apiDescription, schemaDefinitions);
                        break;
                    case "head":
                        pathItem.Head = CreateOperation(apiDescription, schemaDefinitions);
                        break;
                    case "patch":
                        pathItem.Patch = CreateOperation(apiDescription, schemaDefinitions);
                        break;
                }
            }

            return pathItem;
        }

        private Operation CreateOperation(
            ApiDescription apiDescription,
            IDictionary<string, Schema> schemaDefinitions)
        {
            var groupName = _options.GroupNameSelector(apiDescription);

            var parameters = apiDescription.ParameterDescriptions
                .Where(paramDesc => paramDesc.Source.IsFromRequest)
                .Select(paramDesc => CreateParameter(paramDesc, schemaDefinitions))
                .ToList();

            var responses = new Dictionary<string, Response>();
            if (apiDescription.ResponseType == typeof(void))
                responses.Add("204", new Response { Description = "No Content" });
            else
                responses.Add("200", CreateSuccessResponse(apiDescription.ResponseType, schemaDefinitions));

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

            var filterContext = new OperationFilterContext(apiDescription, schemaDefinitions, _schemaProvider);
            foreach (var filter in _options.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }

            return operation;
        }

        private IParameter CreateParameter(
            ApiParameterDescription paramDesc,
            IDictionary<string, Schema> schemaDefinitions)
        {
            var source = paramDesc.Source.Id.ToLower();
            var schema = (paramDesc.Type != null)
                ? _schemaProvider.GetSchema(paramDesc.Type, schemaDefinitions)
                : null;

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

                if (schema != null) nonBodyParam.PopulateFrom(schema);

                if (nonBodyParam.Type == "array")
                    nonBodyParam.CollectionFormat = "multi";

                return nonBodyParam;
            }
        }

        private Response CreateSuccessResponse(
            Type responseType,
            IDictionary<string, Schema> schemaDefinitions)
        {
            return new Response
            {
                Description = "OK",
                Schema = (responseType != null)
                    ? _schemaProvider.GetSchema(responseType, schemaDefinitions)
                    : null
            };
        }
    }
}