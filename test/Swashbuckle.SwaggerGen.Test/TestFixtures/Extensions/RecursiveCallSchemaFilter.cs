using System.Collections.Generic;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.TestFixtures
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
