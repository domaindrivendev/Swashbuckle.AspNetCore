using System;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.Generator
{
    public interface IModelFilter
    {
        void Apply(Schema model, ModelFilterContext context);
    }

    public class ModelFilterContext
    {
        public ModelFilterContext(
            Type systemType,
            JsonObjectContract jsonObjectContract,
            ISchemaRegistry schemaRegistry)
        {
            SystemType = systemType;
            JsonObjectContract = jsonObjectContract;
            SchemaRegistry = schemaRegistry;
        }

        public Type SystemType { get; private set; }

        public JsonObjectContract JsonObjectContract { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}