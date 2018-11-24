using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IDocumentFilter
    {
        void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context);
    }

    public class DocumentFilterContext
    {
        public DocumentFilterContext(
            IEnumerable<ApiDescription> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {
            ApiDescriptions = apiDescriptions;
            SchemaRegistry = schemaRegistry;
        }

        public IEnumerable<ApiDescription> ApiDescriptions { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}
