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

        private static IEnumerable<Type> AnnotationsSubTypeResolver(Type type)
        {
            var subTypes = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault()?.SubTypes ?? Enumerable.Empty<Type>();

            var subTypesFromObsoleteAttribute = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypeAttribute>()
                .Select(attr => attr.SubType);

            return subTypes.Union(subTypesFromObsoleteAttribute);
        }

        private static string AnnotationsDiscriminatorSelector(Type type)
        {
            return type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault()?.Discriminator;
        }
    }
}
