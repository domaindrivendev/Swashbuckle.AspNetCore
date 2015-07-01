using Swashbuckle.Swagger.XmlComments;
using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerOptions
    {
        public SwaggerGeneratorOptions SwaggerGeneratorOptions { get; private set; }

        public SchemaGeneratorOptions SchemaGeneratorOptions { get; private set; }

        public SwaggerOptions()
        {
            SwaggerGeneratorOptions = new SwaggerGeneratorOptions();
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerOperationAttributes());
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerResponseAttributes());

            SchemaGeneratorOptions = new SchemaGeneratorOptions();
        }

        public void IncludeXmlComments(string filePath)
        {
            SchemaGeneratorOptions.ModelFilters.Add(new ApplyXmlTypeComments(filePath));
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplyXmlActionComments(filePath));
        }
    }
}