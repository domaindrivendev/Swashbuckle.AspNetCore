using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public SchemaFilterContext(
            ModelMetadata modelMetadata,
            SchemaRepository schemaRepository,
            ISchemaGenerator schemaGenerator,
            JsonContract jsonContract)
        {
            ModelMetadata = modelMetadata;
            SchemaRepository = schemaRepository;
            SchemaGenerator = schemaGenerator;
            JsonContract = jsonContract;
        }

        public SchemaRepository SchemaRepository { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public ModelMetadata ModelMetadata { get; }

        public JsonContract JsonContract { get; }
    }
}