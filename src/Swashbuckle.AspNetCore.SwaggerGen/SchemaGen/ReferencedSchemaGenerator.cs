using System;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class ReferencedSchemaGenerator : ChainableSchemaGenerator
    {
        public ReferencedSchemaGenerator(
            IContractResolver contractResolver,
            ISchemaGenerator rootGenerator,
            SchemaGeneratorOptions options)
            : base(contractResolver, rootGenerator, options)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            var jsonContract = ContractResolver.ResolveContract(type);

            return (type.IsEnum && Options.UseReferencedDefinitionsForEnums) // enum
                || (jsonContract is JsonObjectContract) // regular object
                || (jsonContract is JsonArrayContract && ((JsonArrayContract)jsonContract).CollectionItemType == jsonContract.UnderlyingType) // self-referencing array
                || (jsonContract is JsonDictionaryContract && ((JsonDictionaryContract)jsonContract).DictionaryValueType == jsonContract.UnderlyingType); // self-referencing dictionary
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            if (!schemaRepository.TryGetIdFor(type, out string schemaId))
            {
                schemaId = Options.SchemaIdSelector(type);
                schemaRepository.ReserveIdFor(type, schemaId);

                schemaRepository.AddSchemaFor(type, Next.GenerateSchema(type, schemaRepository));
            }

            return new OpenApiSchema
            {
                Reference = new OpenApiReference { Id = schemaId, Type = ReferenceType.Schema }
            };
        }
    }
}