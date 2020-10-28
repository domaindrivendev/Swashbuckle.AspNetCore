using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaRepository
    {
        private readonly ISchemaMappingProvider[] _schemaMappingProviders;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly Dictionary<Type, SchemaDisposition> _dispositionCache = new Dictionary<Type, SchemaDisposition>();
        private readonly Dictionary<Type, string> _registeredSchemaIds = new Dictionary<Type, string>();
        private readonly SortedDictionary<string, OpenApiSchema> _registeredSchemas = new SortedDictionary<string, OpenApiSchema>();
        private readonly Dictionary<Type, SchemaMapping> _schemaMappingCache = new Dictionary<Type, SchemaMapping>();

        public IDictionary<string, OpenApiSchema> Schemas => _registeredSchemas;

        // TODO: Split mappings & filters from SchemaGeneratorOptions
        public SchemaRepository(IEnumerable<ISchemaMappingProvider> schemaMappingProviders, ISchemaGenerator schemaGenerator, SchemaGeneratorOptions generatorOptions) {
            _schemaMappingProviders = generatorOptions.SchemaMappingProviders.Concat(schemaMappingProviders).ToArray();
            _schemaGenerator = schemaGenerator;
            _generatorOptions = generatorOptions;
        }

        /// <summary>
        /// Generates a schema id for the provided type.
        /// </summary>
        public string GetSchemaId<T>() =>
            GetSchemaId(typeof(T));

        /// <summary>
        /// Generates a schema id for the provided type.
        /// </summary>
        public string GetSchemaId(Type type) =>
            _generatorOptions.SchemaIdSelector(type);

        /// <summary>
        /// Retrieves a schema for the specified parameter combining the schema for the parameter type with
        /// contextual restrictions and information.
        /// </summary>
        public OpenApiSchema GetParameterSchema(ParameterInfo parameterInfo) {
            var schema = _schemaGenerator.GenerateParameterSchema(parameterInfo, this);

            if(schema.Reference == null)
                ApplyFilters(schema, parameterInfo.ParameterType, this, null, parameterInfo);

            return schema;
        }

        /// <summary>
        /// Retrieves a schema for the specified member combining the schema for the parameter type with
        /// contextual restrictions and information.
        /// </summary>
        public OpenApiSchema GetMemberSchema(MemberInfo memberInfo) {
            var schema = _schemaGenerator.GenerateMemberSchema(memberInfo, this);

            if(schema.Reference == null)
                ApplyFilters(schema, SchemaGenerator.GetMemberType(memberInfo), this, memberInfo, null);

            return schema;
        }

        /// <summary>
        /// Retrieves a schema for the specified member combining the schema for the parameter type with
        /// contextual restrictions and information.
        /// </summary>
        public OpenApiSchema GetMemberSchema<T>(Expression<Func<T, object>> memberExpression) {
            return GetMemberSchema(Resolve(memberExpression.Body));

            MemberInfo Resolve(Expression expr) {
                switch(expr) {
                    case MemberExpression member: return member.Member;
                    case UnaryExpression unary when unary.NodeType == ExpressionType.Convert: return Resolve(unary.Operand);
                    default:
                        throw new Exception($"Unable to resolve a member from the provided expression: {memberExpression}");
                }
            }
        }

        /// <summary>
        /// Retrieves a schema for the specified type.
        /// For a schema incorporating contextual information such as nullability, see
        /// <seealso cref="GetMemberSchema"/> and <seealso cref="GetParameterSchema"/>.
        /// </summary>
        public OpenApiSchema GetTypeSchema<T>() =>
            GetTypeSchema(typeof(T));

        /// <summary>
        /// Retrieves a schema for the specified type.
        /// For a schema incorporating contextual information such as nullability, see
        /// <seealso cref="GetMemberSchema"/> and <seealso cref="GetParameterSchema"/>.
        /// </summary>
        public OpenApiSchema GetTypeSchema(Type type) {
            // The disposition cache allows us to do stupid things like having a type with both an inline and
            // reference schema (see SchemaGenerator.GeneratePolymorphicSchema).
            if(_dispositionCache.TryGetValue(type, out var cachedDisposition) && SchemaDisposition.Reference == cachedDisposition) {
                if(TryGetReference(type, out var referenceSchema))
                    return referenceSchema;

                throw new Exception($"No registered schema is available for type {type} with cached disposition of {cachedDisposition}?");
            }

            if(TryGetSchemaMapping(type, out var mapping))
                return MappedSchema(type, mapping);

            return MappedSchema(type, _schemaGenerator.GenerateTypeSchema(type));
        }

        private bool TryGetSchemaMapping(Type type, out SchemaMapping mapping) {
            if(_schemaMappingCache.TryGetValue(type, out mapping) && mapping != null)
                return true;

            foreach(var schemaMappingProvider in _schemaMappingProviders) {
                if(schemaMappingProvider.TryGetMapping(type, out mapping)) {
                    // We only need to cache mappings for inline schemas, as the resulting reference schemas
                    // are cached to begin with.
                    if(mapping.Disposition == SchemaDisposition.Inline) {
                        _schemaMappingCache[type] = mapping;
                    }

                    return true;
                }
            }

            mapping = _schemaMappingCache[type] = null;
            return false;
        }

        /// <summary>
        /// Bypasses the type mapping process to register a reference schema for the specified type.
        /// Use at your own risk.
        /// </summary>
        public OpenApiSchema RegisterSupplementarySchema(Type type, Func<ISchemaMappingContext, OpenApiSchema> schemaFactory) {
            // This skips the disposition check seen in GetTypeSchema because we should always return
            // the reference schema (as that is the one we are attempting to register)
            if(TryGetReference(type, out var referenceSchema))
                return referenceSchema;

            return MappedSchema(type, SchemaMapping.Reference(schemaFactory));
        }

        /// <summary>
        /// Bypasses the type mapping process to register a reference schema which does not correspond to a type.
        /// </summary>
        public OpenApiSchema RegisterSupplementarySchema(string schemaId, Func<OpenApiSchema> schemaFactory) {
            if(_registeredSchemas.ContainsKey(schemaId))
                return CreateReference(schemaId);

            _registeredSchemas.Add(schemaId, null);

            try {
                _registeredSchemas[schemaId] = schemaFactory();
                return CreateReference(schemaId);
            } catch (Exception) {
                _registeredSchemas.Remove(schemaId);
                throw;
            }
        }

        private OpenApiSchema MappedSchema(Type type, SchemaMapping mapping) {
            if(!_dispositionCache.ContainsKey(type))
                _dispositionCache.Add(type, mapping.Disposition);

            return SchemaDisposition.Reference == mapping.Disposition
            ? MappedReferenceSchema(type, mapping)
            : MappedInlineSchema(type, mapping);
        }

        private OpenApiSchema MappedInlineSchema(Type type, SchemaMapping mapping) {
            var schema = mapping.GenerateSchema(new SchemaMappingContext(type, this));

            ApplyFilters(schema, type, this, null, null);

            return schema;
        }

        private OpenApiSchema MappedReferenceSchema(Type type, SchemaMapping mapping) {
            var schemaId = GetSchemaId(type);
            RegisterSchemaId(type, schemaId);

            var schema = mapping.GenerateSchema(new SchemaMappingContext(type, this));

            ApplyFilters(schema, type, this, null, null);

            _registeredSchemas.Add(schemaId, schema);
            return CreateReference(schemaId);
        }


        private void RegisterSchemaId(Type type, string schemaId)
        {
            foreach(var conflictingEntry in _registeredSchemaIds.Where(e => e.Value == schemaId))
                throw new InvalidOperationException($"SchemaId \"{schemaId}\" for type \"{type}\" is already used for type \"{conflictingEntry.Key}\"");

            _registeredSchemaIds.Add(type, schemaId);
        }

        private bool TryGetReference(Type type, out OpenApiSchema referenceSchema)
        {
            if (_registeredSchemaIds.TryGetValue(type, out string schemaId)) {
                referenceSchema = CreateReference(schemaId);
                return true;
            }

            referenceSchema = null;
            return false;
        }

        private OpenApiSchema CreateReference(string schemaId) =>
            new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = schemaId } };

        private void ApplyFilters(OpenApiSchema schema, Type type, SchemaRepository schemaRepository, MemberInfo memberInfo, ParameterInfo parameterInfo)
        {
            var filterContext = new SchemaFilterContext(
                type: type,
                schemaRepository: schemaRepository,
                memberInfo: memberInfo,
                parameterInfo: parameterInfo);

            foreach (var filter in _generatorOptions.SchemaFilters)
            {
                filter.Apply(schema, filterContext);
            }
        }
    }
}
