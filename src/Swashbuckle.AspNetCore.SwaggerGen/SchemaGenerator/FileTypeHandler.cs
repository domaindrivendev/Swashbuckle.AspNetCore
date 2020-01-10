using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class FileTypeHandler : SchemaGeneratorHandler
    {
        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (type.IsAssignableToOneOf(typeof(IFormFile), typeof(FileResult)))
            {
                shouldBeReferenced = false;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateSchema(Type type, SchemaRepository schemaRepository)
        {
            return new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };
        }
    }
}