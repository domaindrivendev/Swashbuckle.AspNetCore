using System;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Used to implement a logic of Swagger-flavored JSONSchema modification.
    /// </summary>
    public interface ISchemaFilter
    {
        /// <summary>
        /// Applies modification logic to each Swagger schema item.
        /// </summary>
        /// <param name="model">Swagger schema item.</param>
        /// <param name="context">Swagger schema item context.</param>
        void Apply(Schema model, SchemaFilterContext context);
    }

    public class SchemaFilterContext
    {
        public SchemaFilterContext(
            Type systemType,
            JsonContract jsonContract,
            ISchemaRegistry schemaRegistry)
        {
            SystemType = systemType;
            JsonContract = jsonContract;
            SchemaRegistry = schemaRegistry;
        }

        public Type SystemType { get; private set; }

        public JsonContract JsonContract { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}