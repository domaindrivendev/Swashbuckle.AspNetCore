using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ApiExplorer;

namespace Swashbuckle.Swagger
{
    public interface IOperationFilter
    {
        void Apply(Operation operation, OperationFilterContext context);
    }

    public class OperationFilterContext
    {
        public OperationFilterContext(
            ApiDescription apiDescription,
            IDictionary<string, Schema> schemaDefinitions,
            ISchemaProvider schemaProvider)
        {
            ApiDescription = apiDescription;
            SchemaDefinitions = schemaDefinitions;
            SchemaProvider = schemaProvider;
        }

        public ApiDescription ApiDescription { get; private set; }

        public IDictionary<string, Schema> SchemaDefinitions { get; private set; }

        public ISchemaProvider SchemaProvider { get; private set; }
    }
}
