using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class RecursiveCallSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            model.Properties = new Dictionary<string, Schema>();
            model.Properties.Add("ExtraProperty", context.SchemaRegistry.GetOrRegister(typeof(ComplexType)));
        }
    }
}
