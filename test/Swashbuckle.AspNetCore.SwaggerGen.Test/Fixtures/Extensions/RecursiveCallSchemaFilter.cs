using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class RecursiveCallSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            var complexTypeMetadata = context.ModelMetadata.GetMetadataForType(typeof(ComplexType));

            model.Properties = new Dictionary<string, OpenApiSchema>();
            model.Properties.Add("ExtraProperty", context.SchemaGenerator.GenerateSchema(complexTypeMetadata, context.SchemaRepository));
        }
    }
}
