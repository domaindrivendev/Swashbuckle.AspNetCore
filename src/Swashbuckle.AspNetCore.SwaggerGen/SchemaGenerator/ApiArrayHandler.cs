using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class ApiArrayHandler : ApiModelHandler
    {
        public ApiArrayHandler(SchemaGeneratorOptions options, ISchemaGenerator generator)
            : base(options, generator)
        { }

        protected override bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced)
        {
            if (apiModel is ApiArray apiArray)
            {
                shouldBeReferenced = (apiArray.Type == apiArray.ArrayItemType);
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            var arrayType = (ApiArray)apiModel;

            return new OpenApiSchema
            {
                Type = "array",
                Items = Generator.GenerateSchema(arrayType.ArrayItemType, schemaRepository),
                UniqueItems = arrayType.Type.IsSet() ? (bool?)true : null
            };
        }
    }
}