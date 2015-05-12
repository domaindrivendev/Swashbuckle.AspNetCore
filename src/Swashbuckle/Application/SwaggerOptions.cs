using System;
using Swashbuckle.Swagger.XmlComments;
using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerOptions
    {
        public IRootUrlResolver RootUrlResolver { get; private set; }

        internal SchemaGeneratorOptions SchemaGeneratorOptions { get; private set; }

        internal SwaggerGeneratorOptions SwaggerGeneratorOptions { get; private set; }

        public SwaggerOptions()
        {
            RootUrlResolver = new DefaultRootUrlResolver();

            SchemaGeneratorOptions = new SchemaGeneratorOptions();

            SwaggerGeneratorOptions = new SwaggerGeneratorOptions();
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerOperationAttributes());
            SwaggerGeneratorOptions.OperationFilters.Add(new ApplySwaggerResponseAttributes());
        }

        public void SetRootUrlResolver(IRootUrlResolver rootUrlResolver)
        {
            RootUrlResolver = rootUrlResolver;
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