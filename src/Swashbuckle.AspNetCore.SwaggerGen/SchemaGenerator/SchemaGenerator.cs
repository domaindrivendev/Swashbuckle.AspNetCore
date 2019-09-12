using System;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaGenerator : ISchemaGenerator
    {
        private readonly IApiModelResolver _apiModelResolver;
        private readonly SchemaGeneratorOptions _options;
        private readonly ApiModelHandler _chainOfHandlers;

        public SchemaGenerator(IApiModelResolver apiModelResolver, IOptions<SchemaGeneratorOptions> optionsAccessor)
            : this(
                apiModelResolver,
                optionsAccessor.Value ?? new SchemaGeneratorOptions())
        { }

        public SchemaGenerator(IApiModelResolver apiModelResolver, SchemaGeneratorOptions options)
        {
            _apiModelResolver = apiModelResolver;
            _options = options;

            _chainOfHandlers = new FileTypeHandler(options, this)
                .Add(new PolymorphicTypeHandler(options, this))
                .Add(new ApiPrimitiveHandler(options, this))
                .Add(new ApiDictionaryHandler(options, this))
                .Add(new ApiArrayHandler(options, this))
                .Add(new ApiObjectHandler(options, this))
                .Add(new FallbackHandler(options, this));
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository)
        {
            if (_options.CustomTypeMappings.ContainsKey(type))
            {
                return _options.CustomTypeMappings[type]();
            }

            var apiModel = _apiModelResolver.ResolveApiModelFor(type);

            return _chainOfHandlers.GenerateSchema(apiModel, schemaRepository);
        }
    }

    public abstract class ApiModelHandler
    {
        protected ApiModelHandler(SchemaGeneratorOptions options, ISchemaGenerator schemaGenerator)
        {
            Options = options;
            Generator = schemaGenerator;
        }

        protected SchemaGeneratorOptions Options { get; }
        protected ISchemaGenerator Generator { get; }
        protected ApiModelHandler Next { get; set; }

        public OpenApiSchema GenerateSchema(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            if (!CanGenerateSchema(apiModel, out bool shouldBeReferenced))
                return Next.GenerateSchema(apiModel, schemaRepository);

            if (shouldBeReferenced)
            {
                var schemaId = Options.SchemaIdSelector(apiModel.Type);

                return schemaRepository.GetOrAdd(apiModel.Type,
                    schemaId,
                    () => GenerateDefinitionSchemaAndApplyFilters(apiModel, schemaRepository));
            }

            return GenerateDefinitionSchemaAndApplyFilters(apiModel, schemaRepository);
        }

        public ApiModelHandler Add(ApiModelHandler handler)
        {
            var tail = this;
            while (tail.Next != null) tail = tail.Next;
            tail.Next = handler;

            return this;
        }

        protected abstract bool CanGenerateSchema(ApiModel apiModel, out bool shouldBeReferenced);

        protected abstract OpenApiSchema GenerateDefinitionSchema(ApiModel apiModel, SchemaRepository schemaRepository);

        private OpenApiSchema GenerateDefinitionSchemaAndApplyFilters(ApiModel apiModel, SchemaRepository schemaRepository)
        {
            var schema = GenerateDefinitionSchema(apiModel, schemaRepository);

            var filterContext = new SchemaFilterContext(apiModel, schemaRepository, Generator);
            foreach (var filter in Options.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }
    }
}
