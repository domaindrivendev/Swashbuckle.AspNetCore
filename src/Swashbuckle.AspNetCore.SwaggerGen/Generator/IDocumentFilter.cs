using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Used to implement a logic of Swagger document modification.
    /// </summary>
    public interface IDocumentFilter
    {
        /// <summary>
        /// Applies Swagger document modification logic to each document.
        /// </summary>
        /// <param name="swaggerDoc">Swagger document.</param>
        /// <param name="context">Swagger document context.</param>
        void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context);
    }

    public class DocumentFilterContext
    {
        public DocumentFilterContext(
            ApiDescriptionGroupCollection apiDescriptionsGroups,
            ISchemaRegistry schemaRegistry)
        {
            ApiDescriptionsGroups = apiDescriptionsGroups;
            SchemaRegistry = schemaRegistry;
        }

        public ApiDescriptionGroupCollection ApiDescriptionsGroups { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}
