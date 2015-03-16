using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public interface IDocumentFilter
    {
        void Apply(SwaggerDocument swaggerDoc, SchemaGenerator schemaRegistry, ApiDescriptionGroupCollection apiDescriptionGroups);
    }
}
