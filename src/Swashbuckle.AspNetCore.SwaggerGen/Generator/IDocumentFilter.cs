using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IDocumentFilter
    {
        void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context);
    }

    public class DocumentFilterContext
    {
        public DocumentFilterContext(
            ApiDescriptionGroupCollection apiDescriptionsGroups,
            IEnumerable<ApiDescription> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ApiDescriptionsGroups = apiDescriptionsGroups;
#pragma warning restore CS0618 // Type or member is obsolete
            ApiDescriptions = apiDescriptions;
            SchemaRegistry = schemaRegistry;
        }

        [Obsolete("Deprecated: Use ApiDescriptions")]
        public ApiDescriptionGroupCollection ApiDescriptionsGroups { get; private set; }

        public IEnumerable<ApiDescription> ApiDescriptions { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}
