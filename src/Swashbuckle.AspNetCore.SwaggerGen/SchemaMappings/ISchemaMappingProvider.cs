using System;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings
{
    /// <summary>
    /// Interface providing mappings from types to <see cref="SchemaMapping"/> instances.
    /// Exists primarily to permit the establishment of the correct <see cref="SchemaDisposition"/>
    /// for mapped types prior to schema generation.
    /// </summary>
    public interface ISchemaMappingProvider
    {
        /// <summary>
        /// Attempts to resolve the provided type to an <see cref="SchemaMapping"/>.
        /// </summary>
        bool TryGetMapping(Type type, out SchemaMapping schemaMapping);
    }
}