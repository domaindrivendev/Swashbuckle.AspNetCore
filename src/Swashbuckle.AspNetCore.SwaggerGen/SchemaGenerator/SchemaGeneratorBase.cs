using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public abstract class SchemaGeneratorBase : ISchemaGenerator
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly List<SchemaGeneratorHandler> _handlers;

        protected SchemaGeneratorBase(SchemaGeneratorOptions generatorOptions)
        {
            _generatorOptions = generatorOptions;
            _handlers = new List<SchemaGeneratorHandler>();
        }

        protected void AddHandler(SchemaGeneratorHandler handler)
        {
            _handlers.Add(handler);
        }

        public OpenApiSchema GenerateSchema(Type type, SchemaRepository schemaRepository)
        {
            if (_generatorOptions.CustomTypeMappings.ContainsKey(type))
            {
                return _generatorOptions.CustomTypeMappings[type]();
            }

            if (type.IsNullable(out Type innerType))
                return GenerateSchema(innerType, schemaRepository);

            foreach (var handler in _handlers)
            {
                if (!handler.CanCreateSchemaFor(type, out bool shouldBeReferenced)) continue;

                return shouldBeReferenced
                    ? CreateReferenceSchema(type, schemaRepository, () => ApplyFiltersTo(handler.CreateSchema(type, schemaRepository), type, schemaRepository))
                    : ApplyFiltersTo(handler.CreateSchema(type, schemaRepository), type, schemaRepository);
            }

            throw new NotSupportedException($"Cannot generate schema for type {type}");
        }

        private OpenApiSchema CreateReferenceSchema(Type type, SchemaRepository schemaRepository, Func<OpenApiSchema> factoryMethod)
        {
            return schemaRepository.GetOrAdd(
                type: type,
                schemaId: _generatorOptions.SchemaIdSelector(type),
                factoryMethod: factoryMethod);
        }

        private OpenApiSchema ApplyFiltersTo(OpenApiSchema schema, Type type, SchemaRepository schemaRepository)
        {
            var filterContext = new SchemaFilterContext(type, schemaRepository, this);
            foreach (var filter in _generatorOptions.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }

            return schema;
        }
    }

    public abstract class SchemaGeneratorHandler
    {
        public abstract bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced);

        public abstract OpenApiSchema CreateSchema(Type type, SchemaRepository schemaRepository);
    }
}
