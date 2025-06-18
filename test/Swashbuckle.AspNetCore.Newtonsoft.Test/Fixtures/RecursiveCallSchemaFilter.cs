using Microsoft.OpenApi.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class RecursiveCallSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema model, SchemaFilterContext context)
    {
        if (model.Type == JsonSchemaTypes.Object)
        {
            model.Properties.Add("Self", context.SchemaGenerator.GenerateSchema(context.Type, context.SchemaRepository));
        }
    }
}
