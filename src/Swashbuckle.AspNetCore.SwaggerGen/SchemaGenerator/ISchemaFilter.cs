using System;
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
            Type type,
            SchemaRepository schemaRepository,
            ISchemaGenerator schemaGenerator)
        {
            Type = type;
            SchemaRepository = schemaRepository;
            SchemaGenerator = schemaGenerator;
        }

        public Type Type { get; }

        public SchemaRepository SchemaRepository { get; }

        public ISchemaGenerator SchemaGenerator { get; }
    }
}