using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using Xunit;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class AnnotationsSchemaFilterTests
    {
        private readonly IApiModelResolver _apiModelResolver;

        public AnnotationsSchemaFilterTests()
        {
            _apiModelResolver = new NewtonsoftApiModelResolver(new JsonSerializerSettings(), new SchemaGeneratorOptions());
        }

        [Theory]
        [InlineData(typeof(SwaggerAnnotatedClass))]
        [InlineData(typeof(SwaggerAnnotatedStruct))]
        public void Apply_DelegatesToSpecifiedFilter_IfTypeDecoratedWithFilterAttribute(Type type)
        {
            var schema = new OpenApiSchema();
            var context = FilterContextFor(type);

            Subject().Apply(schema, context);

            Assert.NotEmpty(schema.Extensions);
        }

        [Theory]
        [InlineData(typeof(SwaggerPropertyAnnotedClass))]
        public void Apply_SwaggerPropertyAttibute(Type typeToGenerate)
        {
            var context = FilterContextFor(typeToGenerate);
            var schema = context.SchemaGenerator.GenerateSchema(typeToGenerate, context.SchemaRepository);
            var subject = Subject();

            subject.Apply(schema, context);

            if (!schema.Properties.Any() &&
                schema.Reference != null &&
                context.SchemaRepository.Schemas.TryGetValue(schema.Reference.Id ,out OpenApiSchema propertiesSchema) &&
                propertiesSchema.Properties.Any())
            {
                foreach (var item in propertiesSchema.Properties.SelectMany(p => p.Value.Properties))
                {
                    subject.Apply(item.Value, context);
                }
            }

            var schemaRepository = context.SchemaRepository.Schemas;

            Assert.True(schemaRepository.ContainsKey("SerializedPropertyAnnotedClass"));
            Assert.False(schemaRepository.ContainsKey("DeclaredPropertyAnnotedClass"));
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            return new SchemaFilterContext(
                _apiModelResolver.ResolveApiModelFor(type),
                schemaRepository: new SchemaRepository(),
                schemaGenerator: GetSchemaGenerator()
            );
        }

        private AnnotationsSchemaFilter Subject()
        {
            return new AnnotationsSchemaFilter(null);
        }

        private ISchemaGenerator GetSchemaGenerator(
            Action<SchemaGeneratorOptions> configureOptions = null,
            Action<JsonSerializerSettings> configureSerializer = null)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            configureSerializer?.Invoke(jsonSerializerSettings);

            var schemaGeneratorOptions = new SchemaGeneratorOptions();
            configureOptions?.Invoke(schemaGeneratorOptions);

            return new SchemaGenerator(
                new NewtonsoftApiModelResolver(jsonSerializerSettings, schemaGeneratorOptions),
                schemaGeneratorOptions);
        }
    }
}