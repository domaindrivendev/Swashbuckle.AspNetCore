using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
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
#if NET7_0_OR_GREATER
            var jsonDerivedTypeAttributes = type.GetCustomAttributes(false)
                .OfType<JsonDerivedTypeAttribute>()
                .ToArray();

            if (jsonDerivedTypeAttributes.Any())
            {
                return jsonDerivedTypeAttributes.Select(attr => attr.DerivedType);
            }
#endif

#pragma warning disable CS0618 // Type or member is obsolete
            var subTypeAttributes = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypeAttribute>()
                .ToArray();
#pragma warning restore CS0618 // Type or member is obsolete

            if (subTypeAttributes.Any())
            {
                return subTypeAttributes.Select(attr => attr.SubType);
            }

#pragma warning disable CS0618 // Type or member is obsolete
            var obsoleteAttribute = type.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault();
#pragma warning restore CS0618 // Type or member is obsolete

            if (obsoleteAttribute != null)
            {
                return obsoleteAttribute.SubTypes;
            }

            return Enumerable.Empty<Type>();
        }

        private static string AnnotationsDiscriminatorNameSelector(Type baseType)
        {
#if NET7_0_OR_GREATER
            var jsonPolymorphicAttribute = baseType.GetCustomAttributes(false)
                .OfType<JsonPolymorphicAttribute>()
                .FirstOrDefault();

            if (jsonPolymorphicAttribute is not null)
            {
                return jsonPolymorphicAttribute.TypeDiscriminatorPropertyName;
            }
#endif

#pragma warning disable CS0618 // Type or member is obsolete
            var discriminatorAttribute = baseType.GetCustomAttributes(false)
                .OfType<SwaggerDiscriminatorAttribute>()
                .FirstOrDefault();
#pragma warning restore CS0618 // Type or member is obsolete

            if (discriminatorAttribute != null)
            {
                return discriminatorAttribute.PropertyName;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            var obsoleteAttribute = baseType.GetCustomAttributes(false)
                .OfType<SwaggerSubTypesAttribute>()
                .FirstOrDefault();
#pragma warning restore CS0618 // Type or member is obsolete

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

#if NET7_0_OR_GREATER
            var jsonDerivedTypeAttribute = subType.BaseType.GetCustomAttributes(false)
                .OfType<JsonDerivedTypeAttribute>()
                .FirstOrDefault(attr => attr.DerivedType == subType);

            if (jsonDerivedTypeAttribute is not null)
            {
                return jsonDerivedTypeAttribute.TypeDiscriminator?.ToString();
            }
#endif

#pragma warning disable CS0618 // Type or member is obsolete
            var subTypeAttribute = subType.BaseType.GetCustomAttributes(false)
                .OfType<SwaggerSubTypeAttribute>()
                .FirstOrDefault(attr => attr.SubType == subType);
#pragma warning restore CS0618 // Type or member is obsolete

            if (subTypeAttribute != null)
            {
                return subTypeAttribute.DiscriminatorValue;
            }

            return null;
        }
    }
}
