using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaFilter
    {
        void Apply(Schema model, SchemaFilterContext context);
    }

    public class SchemaFilterContext
    {
        public SchemaFilterContext(
            Type systemType,
            ModelMetadata modelMetadata,
            JsonContract jsonContract,
            ISchemaRegistry schemaRegistry)
        {
            SystemType = systemType;
            ModelMetadata = modelMetadata;
            JsonContract = jsonContract;
            SchemaRegistry = schemaRegistry;
        }

        [Obsolete("Use ModelMetadata instead")]
        public Type SystemType { get; private set; }

        public ModelMetadata ModelMetadata { get; private set; }

        public JsonContract JsonContract { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}