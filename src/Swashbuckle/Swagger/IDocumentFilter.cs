using Microsoft.AspNet.Mvc.ApiExplorer;

namespace Swashbuckle.Swagger
{
    public interface IDocumentFilter
    {
        void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context);
    }

    public class DocumentFilterContext
    {
        public DocumentFilterContext(
            ApiDescriptionGroupCollection apiDescriptionsGroups,
            ISchemaProvider schemaProvider)
        {
            ApiDescriptionsGroups = apiDescriptionsGroups;
            SchemaProvider = schemaProvider;
        }

        public ApiDescriptionGroupCollection ApiDescriptionsGroups { get; private set; }

        public ISchemaProvider SchemaProvider { get; private set; }
    }
}
