using System;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaGenerator
    {
        bool CanGenerateSchemaFor(Type type);

        OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository);
    }
}
