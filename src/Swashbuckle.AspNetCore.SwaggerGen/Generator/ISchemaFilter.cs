using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaFilter
    {
        void Apply(OpenApiSchema schema, SchemaFilterContext context);
    }

    public class SchemaFilterContext
    {
        public SchemaFilterContext(JsonContract jsonContract, SchemaRepository schemaRepository, ISchemaGenerator schemaGenerator)
        {
            JsonContract = jsonContract;
            SchemaRepository = schemaRepository;
            SchemaGenerator = schemaGenerator;
        }

        public JsonContract JsonContract { get; }

        public SchemaRepository SchemaRepository { get; }

        public ISchemaGenerator SchemaGenerator { get; }
    }
}