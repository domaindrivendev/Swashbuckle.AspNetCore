using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ReferenceableTypeHandler : SchemaGeneratorHandler
    {
        public ReferenceableTypeHandler(SchemaGeneratorOptions schemaGeneratorOptions, ISchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            // Enum
            if (modelMetadata.IsEnum)
                return true;

            // self-referencing array
            if (jsonContract is JsonArrayContract jsonArrayContract && jsonArrayContract.CollectionItemType == jsonContract.UnderlyingType)
                return true;

            // self-referencing dictionary
            if (jsonContract is JsonDictionaryContract jsonDictionaryContract && jsonDictionaryContract.DictionaryValueType == jsonContract.UnderlyingType)
                return true;

            // "Object"
            if (jsonContract is JsonObjectContract)
                return true;

            return false;
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            if (!schemaRepository.TryGetIdFor(modelMetadata.ModelType, out string schemaId))
            {
                schemaId = SchemaGeneratorOptions.SchemaIdSelector(modelMetadata.ModelType);
                schemaRepository.ReserveIdFor(modelMetadata.ModelType, schemaId);

                schemaRepository.AddSchemaFor(modelMetadata.ModelType, Next.GenerateSchema(modelMetadata, schemaRepository, jsonContract));
            }

            return new OpenApiSchema
            {
                Reference = new OpenApiReference { Id = schemaId, Type = ReferenceType.Schema }
            };
        }
    }
}