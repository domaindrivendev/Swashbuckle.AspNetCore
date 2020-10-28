using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings
{
    public interface ISchemaMappingContext
    {
        /// <summary>
        /// The type to be represented by the resulting schema.
        /// </summary>
        Type Type { get; }

        SchemaRepository SchemaRepository { get; }
    }


    public class SchemaMappingContext : ISchemaMappingContext
    {
        public Type Type { get; }
        public SchemaRepository SchemaRepository { get; }

        public SchemaMappingContext(Type type, SchemaRepository schemaRepository) {
            Type = type;
            SchemaRepository = schemaRepository;
        }
    }
}