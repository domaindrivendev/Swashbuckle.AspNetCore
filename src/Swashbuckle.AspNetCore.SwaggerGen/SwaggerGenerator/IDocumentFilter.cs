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
            SchemaRepository schemaRepository)
        {
            ApiDescriptions = apiDescriptions;
            SchemaRepository = schemaRepository;
        }

        public IEnumerable<ApiDescription> ApiDescriptions { get; }

        public SchemaRepository SchemaRepository { get; }
    }
}
