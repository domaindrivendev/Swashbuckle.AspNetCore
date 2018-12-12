using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class RecursiveCallSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            model.Properties = new Dictionary<string, OpenApiSchema>();
            model.Properties.Add("ExtraProperty", context.SchemaGenerator.GenerateSchemaFor(typeof(ComplexType), context.SchemaRepository));
        }
    }
}
