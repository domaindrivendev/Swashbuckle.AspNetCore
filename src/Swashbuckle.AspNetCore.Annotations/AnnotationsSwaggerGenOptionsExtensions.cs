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
        /// <param name="enableAnnotationsForInheritance">Enables SwaggerSubType attribute for inheritance</param>
        /// <param name="enableAnnotationsForPolymorphism">Enables SwaggerSubType and SwaggerDiscriminator attributes for polymorphism</param>
        public static void EnableAnnotations(
            this SwaggerGenOptions options,
            bool enableAnnotationsForInheritance,
            bool enableAnnotationsForPolymorphism)
        {
            options.SchemaFilter<AnnotationsSchemaFilter>();
            options.ParameterFilter<AnnotationsParameterFilter>();
            options.RequestBodyFilter<AnnotationsRequestBodyFilter>();
            options.OperationFilter<AnnotationsOperationFilter>();
            options.DocumentFilter<AnnotationsDocumentFilter>();

            if (enableAnnotationsForInheritance || enableAnnotationsForPolymorphism)
            {
                options.SelectSubTypesUsing(AnnotationsSubTypesSelector);
                options.SelectDiscriminatorNameUsing(AnnotationsDiscriminatorNameSelector);
                options.SelectDiscriminatorValueUsing(AnnotationsDiscriminatorValueSelector);

                if (enableAnnotationsForInheritance)
                {
                    options.UseAllOfForInheritance();
                }

                if (enableAnnotationsForPolymorphism)
                {
                    options.UseOneOfForPolymorphism();
                }
            }
        }

        /// <summary>
        /// Enables Swagger annotations (SwaggerOperationAttribute, SwaggerParameterAttribute etc.)
        /// </summary>
        /// <param name="options"></param>
        public static void EnableAnnotations(this SwaggerGenOptions options)
        {
            options.EnableAnnotations(
                enableAnnotationsForPolymorphism: false,
                enableAnnotationsForInheritance: false);
        }

        private static IEnumerable<Type> AnnotationsSubTypesSelector(Type type)
        {
            var subTypeAttributes = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypeAttribute>();

            if (subTypeAttributes.Any())
            {
                return subTypeAttributes.Select(attr => attr.SubType);
            }

            var obsoleteAttribute = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault();

            if (obsoleteAttribute != null)
            {
                return obsoleteAttribute.SubTypes;
            }

            return Enumerable.Empty<Type>();
        }

        private static string AnnotationsDiscriminatorNameSelector(Type baseType)
        {
            var discriminatorAttribute = baseType.GetCustomAttributes(false)
                .OfType<SwaggerDiscriminatorAttribute>()
                .FirstOrDefault();

            if (discriminatorAttribute != null)
            {
                return discriminatorAttribute.PropertyName;
            }

            var obsoleteAttribute = baseType.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault();

            if (obsoleteAttribute != null)
            {
                return obsoleteAttribute.Discriminator;
            }

            return null;
        }

        private static string AnnotationsDiscriminatorValueSelector(Type subType)
        {
            if (subType.BaseType == null)
                return null;

            var subTypeAttribute = subType.BaseType.GetCustomAttributes(false)
                .OfType<SwaggerSubTypeAttribute>()
                .FirstOrDefault(attr => attr.SubType == subType);

            if (subTypeAttribute != null)
            {
                return subTypeAttribute.DiscriminatorValue;
            }

            return null;
        }
    }
}
