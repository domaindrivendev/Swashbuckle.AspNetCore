using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Annotations;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AnnotationsSwaggerGenOptionsExtensions
    {
        /// <summary>
        /// Enables Swagger annotations (SwaggerOperationAttribute, SwaggerParameterAttribute etc.)
        /// </summary>
        /// <param name="options"></param>
        /// <param name="enableSubTypeAnnotations">Enables polymorphic schemas based on the presence of SwaggerSubTypeAttribute on base types</param>
        public static void EnableAnnotations(this SwaggerGenOptions options, bool enableSubTypeAnnotations = false)
        {
            options.SchemaFilter<AnnotationsSchemaFilter>();
            options.ParameterFilter<AnnotationsParameterFilter>();
            options.RequestBodyFilter<AnnotationsRequestBodyFilter>();
            options.OperationFilter<AnnotationsOperationFilter>();
            options.DocumentFilter<AnnotationsDocumentFilter>();

            if (enableSubTypeAnnotations)
            {
                options.UseOneOfForPolymorphism(AnnotationsDiscriminatorSelector);
                options.DetectSubTypesUsing(AnnotationsSubTypeResolver);
                options.UseAllOfForInheritance();
            }
        }

        private static IDictionary<Type, string> AnnotationsSubTypeResolver(Type type)
        {
            var knownSubTypes = type.GetCustomAttributes(false)
                .OfType<SwaggerKnownSubTypeAttribute>()
                .ToDictionary(st => st.SubType, st => st.DiscriminatorValue);

            var subTypes = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault()?.SubTypes
                ?.ToDictionary(st => st, st => null as string)
                ?? new Dictionary<Type, string>();

            var subTypesFromObsoleteAttribute = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypeAttribute>()
                .ToDictionary(st => st.SubType, st => null as string);

            return knownSubTypes
                .Union(subTypes)
                .Union(subTypesFromObsoleteAttribute)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private static string AnnotationsDiscriminatorSelector(Type type)
        {
            return type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault()?.Discriminator;
        }
    }
}
