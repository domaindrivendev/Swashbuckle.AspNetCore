using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaFilter
    {
        void Apply(OpenApiSchema schema, SchemaFilterContext context);
    }

    public class SchemaFilterContext
    {
        public SchemaFilterContext(
            ApiModel apiModel,
            SchemaRepository schemaRepository,
            ISchemaGenerator schemaGenerator)
        {
            ApiModel = apiModel;
            SchemaRepository = schemaRepository;
            SchemaGenerator = schemaGenerator;
        }

        public ApiModel ApiModel { get; }

        public SchemaRepository SchemaRepository { get; }

        public ISchemaGenerator SchemaGenerator { get; }
    }
}