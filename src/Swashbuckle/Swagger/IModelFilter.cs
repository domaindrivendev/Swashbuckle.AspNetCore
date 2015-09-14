using System;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
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
            ISchemaProvider schemaProvider)
        {
            SystemType = systemType;
            JsonObjectContract = jsonObjectContract;
            SchemaProvider = schemaProvider;
        }

        public Type SystemType { get; private set; }

        public JsonObjectContract JsonObjectContract { get; private set; }

        public ISchemaProvider SchemaProvider { get; private set; }
    }
}