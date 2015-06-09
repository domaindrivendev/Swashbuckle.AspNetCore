using System;
using Swashbuckle.Swagger.XmlComments;
using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerOptions
    {
        internal SchemaGeneratorOptions SchemaGeneratorOptions { get; private set; }

        internal SwaggerGeneratorOptions SwaggerGeneratorOptions { get; private set; }

        public SwaggerOptions()
        {
            SchemaGeneratorOptions = new SchemaGeneratorOptions();

            SwaggerGeneratorOptions = new SwaggerGeneratorOptions();
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerOperationAttributes());
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerResponseAttributes());
        }

        public void SwaggerGenerator(Action<SwaggerGeneratorOptions> configure)
        {
            configure(SwaggerGeneratorOptions);
        }

        public void SchemaGenerator(Action<SchemaGeneratorOptions> configure)
        {
            configure(SchemaGeneratorOptions);
        }

        public void IncludeXmlComments(string filePath)
        {
            SchemaGeneratorOptions.ModelFilters.Add(new ApplyXmlTypeComments(filePath));
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplyXmlActionComments(filePath));
        }
    }
}