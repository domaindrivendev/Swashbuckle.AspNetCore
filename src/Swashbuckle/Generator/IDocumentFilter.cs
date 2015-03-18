using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger.Generator
{
    public interface IDocumentFilter
    {
        void Apply(SwaggerDocument swaggerDoc, SchemaGenerator schemaRegistry, ApiDescriptionGroupCollection apiDescriptionGroups);
    }
}
