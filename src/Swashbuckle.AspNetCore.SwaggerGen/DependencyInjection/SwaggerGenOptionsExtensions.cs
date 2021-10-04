using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenOptionsExtensions
    {
        /// <summary>
        /// Define one or more documents to be created by the Swagger generator
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="name">A URI-friendly name that uniquely identifies the document</param>
        /// <param name="info">Global metadata to be included in the Swagger output</param>
        public static void SwaggerDoc(
            this SwaggerGenOptions swaggerGenOptions,
            string name,
            OpenApiInfo info)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.Add(name, info);
        }

        /// <summary>
        /// Provide a custom strategy for selecting actions.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="predicate">
        /// A lambda that returns true/false based on document name and ApiDescription
        /// </param>
        public static void DocInclusionPredicate(
            this SwaggerGenOptions swaggerGenOptions,
            Func<string, ApiDescription, bool> predicate)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.DocInclusionPredicate = predicate;
        }

        /// <summary>
        /// Ignore any actions that are decorated with the ObsoleteAttribute
        /// </summary>
        public static void IgnoreObsoleteActions(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.IgnoreObsoleteActions = true;
        }

        /// <summary>
        /// Merge actions that have conflicting HTTP methods and paths (must be unique for Swagger 2.0)
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="resolver"></param>
        public static void ResolveConflictingActions(
            this SwaggerGenOptions swaggerGenOptions,
            Func<IEnumerable<ApiDescription>, ApiDescription> resolver)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.ConflictingActionsResolver = resolver;
        }

        /// <summary>
        /// Provide a custom strategy for assigning "operationId" to operations
        /// </summary>
        public static void CustomOperationIds(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, string> operationIdSelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.OperationIdSelector = operationIdSelector;
        }


        /// <summary>
        /// Provide a custom strategy for assigning a default "tag" to operations
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="tagSelector"></param>
        [Obsolete("Deprecated: Use the overload that accepts a Func that returns a list of tags")]
        public static void TagActionsBy(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, string> tagSelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.TagsSelector = (apiDesc) => new[] { tagSelector(apiDesc) };
        }

        /// <summary>
        /// Provide a custom strategy for assigning "tags" to actions
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="tagsSelector"></param>
        public static void TagActionsBy(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, IList<string>> tagsSelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.TagsSelector = tagsSelector;
        }

        /// <summary>
        /// Provide a custom strategy for sorting actions before they're transformed into the Swagger format
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="sortKeySelector"></param>
        public static void OrderActionsBy(
            this SwaggerGenOptions swaggerGenOptions,
            Func<ApiDescription, string> sortKeySelector)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SortKeySelector = sortKeySelector;
        }

        /// <summary>
        /// Describe all parameters, regardless of how they appear in code, in camelCase
        /// </summary>
        public static void DescribeAllParametersInCamelCase(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
        }


        /// <summary>
        /// Provide specific server information to include in the generated Swagger document
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="server">A description of the server</param>
        public static void AddServer(this SwaggerGenOptions swaggerGenOptions, OpenApiServer server)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.Servers.Add(server);
        }

        /// <summary>
        /// Add one or more "securityDefinitions", describing how your API is protected, to the generated Swagger
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="name">A unique name for the scheme, as per the Swagger spec.</param>
        /// <param name="securityScheme">
        /// A description of the scheme - can be an instance of BasicAuthScheme, ApiKeyScheme or OAuth2Scheme
        /// </param>
        public static void AddSecurityDefinition(
            this SwaggerGenOptions swaggerGenOptions,
            string name,
            OpenApiSecurityScheme securityScheme)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SecuritySchemes.Add(name, securityScheme);
        }

        /// <summary>
        /// Adds a global security requirement
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="securityRequirement">
        /// A dictionary of required schemes (logical AND). Keys must correspond to schemes defined through AddSecurityDefinition
        /// If the scheme is of type "oauth2", then the value is a list of scopes, otherwise it MUST be an empty array
        /// </param>
        public static void AddSecurityRequirement(
            this SwaggerGenOptions swaggerGenOptions,
            OpenApiSecurityRequirement securityRequirement)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.SecurityRequirements.Add(securityRequirement);
        }

        /// <summary>
        /// Provide a custom mapping, for a given type, to the Swagger-flavored JSONSchema
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="type">System type</param>
        /// <param name="schemaFactory">A factory method that generates Schema's for the provided type</param>
        public static void MapType(
            this SwaggerGenOptions swaggerGenOptions,
            Type type,
            Func<OpenApiSchema> schemaFactory)
        {
            swaggerGenOptions.SchemaGeneratorOptions.CustomTypeMappings.Add(type, schemaFactory);
        }

        /// <summary>
        /// Provide a custom mapping, for a given type, to the Swagger-flavored JSONSchema
        /// </summary>
        /// <typeparam name="T">System type</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="schemaFactory">A factory method that generates Schema's for the provided type</param>
        public static void MapType<T>(
            this SwaggerGenOptions swaggerGenOptions,
            Func<OpenApiSchema> schemaFactory)
        {
            swaggerGenOptions.MapType(typeof(T), schemaFactory);
        }

        /// <summary>
        /// Generate inline schema definitions (as opposed to referencing a shared definition) for enum parameters and properties
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        public static void UseInlineDefinitionsForEnums(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaGeneratorOptions.UseInlineDefinitionsForEnums = true;
        }

        /// <summary>
        /// Provide a custom strategy for generating the unique Id's that are used to reference object Schema's
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="schemaIdSelector">
        /// A lambda that returns a unique identifier for the provided system type
        /// </param>
        public static void CustomSchemaIds(
            this SwaggerGenOptions swaggerGenOptions,
            Func<Type, string> schemaIdSelector)
        {
            swaggerGenOptions.SchemaGeneratorOptions.SchemaIdSelector = schemaIdSelector;
        }

        /// <summary>
        /// Ignore any properties that are decorated with the ObsoleteAttribute
        /// </summary>
        public static void IgnoreObsoleteProperties(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaGeneratorOptions.IgnoreObsoleteProperties = true;
        }

        /// <summary>
        /// Enables composite schema generation. If enabled, subtype schemas will contain the allOf construct to
        /// incorporate properties from the base class instead of defining those properties inline.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        public static void UseAllOfForInheritance(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaGeneratorOptions.UseAllOfForInheritance = true;
        }

        /// <summary>
        /// Enables polymorphic schema generation. If enabled, request and response schemas will contain the oneOf
        /// construct to describe sub types as a set of alternative schemas.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        public static void UseOneOfForPolymorphism(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaGeneratorOptions.UseOneOfForPolymorphism = true;
        }

        /// <summary>
        /// To support polymorphism and inheritance behavior, Swashbuckle needs to detect the "known" subtypes for a given base type.
        /// That is, the subtypes exposed by your API. By default, this will be any subtypes in the same assembly as the base type.
        /// To override this, you can provide a custom selector function. This setting is only applicable when used in conjunction with
        /// UseOneOfForPolymorphism and/or UseAllOfForInheritance.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="customSelector"></param>
        public static void SelectSubTypesUsing(
            this SwaggerGenOptions swaggerGenOptions,
            Func<Type, IEnumerable<Type>> customSelector)
        {
            swaggerGenOptions.SchemaGeneratorOptions.SubTypesSelector = customSelector;
        }

        /// <summary>
        /// If the configured serializer supports polymorphic serialization/deserialization by emitting/accepting a special "discriminator" property,
        /// and UseOneOfForPolymorphism is enabled, then Swashbuckle will include a description for that property based on the serializer's behavior.
        /// However, if you've customized your serializer to support polymorphism, you can provide a custom strategy to tell Swashbuckle which property,
        /// for a given type, will be used as a discriminator. This setting is only applicable when used in conjunction with UseOneOfForPolymorphism.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="customSelector"></param>
        public static void SelectDiscriminatorNameUsing(
            this SwaggerGenOptions swaggerGenOptions,
            Func<Type, string> customSelector)
        {
            swaggerGenOptions.SchemaGeneratorOptions.DiscriminatorNameSelector = customSelector;
        }

        /// <summary>
        /// If the configured serializer supports polymorphic serialization/deserialization by emitting/accepting a special "discriminator" property,
        /// and UseOneOfForPolymorphism is enabled, then Swashbuckle will include a mapping of possible discriminator values to schema definitions.
        /// However, if you've customized your serializer to support polymorphism, you can provide a custom mapping strategy to tell Swashbuckle what
        /// the discriminator value should be for a given sub type. This setting is only applicable when used in conjunction with UseOneOfForPolymorphism.
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="customSelector"></param>
        public static void SelectDiscriminatorValueUsing(
            this SwaggerGenOptions swaggerGenOptions,
            Func<Type, string> customSelector)
        {
            swaggerGenOptions.SchemaGeneratorOptions.DiscriminatorValueSelector = customSelector;
        }

        /// <summary>
        /// Extend reference schemas (using the allOf construct) so that contextual metadata can be applied to all parameter and property schemas
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        public static void UseAllOfToExtendReferenceSchemas(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaGeneratorOptions.UseAllOfToExtendReferenceSchemas = true;
        }

        /// <summary>
        /// Enable detection of non nullable reference types to set Nullable flag accordingly on schema properties
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        public static void SupportNonNullableReferenceTypes(this SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SchemaGeneratorOptions.SupportNonNullableReferenceTypes = true;
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify Schemas after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from ISchemaFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void SchemaFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : ISchemaFilter
        {
            swaggerGenOptions.SchemaFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify Parameters after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IParameterFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void ParameterFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : IParameterFilter
        {
            swaggerGenOptions.ParameterFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify RequestBodys after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IRequestBodyFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void RequestBodyFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : IRequestBodyFilter
        {
            swaggerGenOptions.RequestBodyFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify Operations after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IOperationFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void OperationFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : IOperationFilter
        {
            swaggerGenOptions.OperationFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Extend the Swagger Generator with "filters" that can modify SwaggerDocuments after they're initially generated
        /// </summary>
        /// <typeparam name="TFilter">A type that derives from IDocumentFilter</typeparam>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="arguments">Optionally inject parameters through filter constructors</param>
        public static void DocumentFilter<TFilter>(
            this SwaggerGenOptions swaggerGenOptions,
            params object[] arguments)
            where TFilter : IDocumentFilter
        {
            swaggerGenOptions.DocumentFilterDescriptors.Add(new FilterDescriptor
            {
                Type = typeof(TFilter),
                Arguments = arguments
            });
        }

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="xmlDocFactory">A factory method that returns XML Comments as an XPathDocument</param>
        /// <param name="includeControllerXmlComments">
        /// Flag to indicate if controller XML comments (i.e. summary) should be used to assign Tag descriptions.
        /// Don't set this flag if you're customizing the default tag for operations via TagActionsBy.
        /// </param>
        public static void IncludeXmlComments(
            this SwaggerGenOptions swaggerGenOptions,
            Func<XPathDocument> xmlDocFactory,
            bool includeControllerXmlComments = false)
        {
            var xmlDoc = xmlDocFactory();
            swaggerGenOptions.ParameterFilter<XmlCommentsParameterFilter>(xmlDoc);
            swaggerGenOptions.RequestBodyFilter<XmlCommentsRequestBodyFilter>(xmlDoc);
            swaggerGenOptions.OperationFilter<XmlCommentsOperationFilter>(xmlDoc);
            swaggerGenOptions.SchemaFilter<XmlCommentsSchemaFilter>(xmlDoc);

            if (includeControllerXmlComments)
                swaggerGenOptions.DocumentFilter<XmlCommentsDocumentFilter>(xmlDoc);
        }

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="filePath">An absolute path to the file that contains XML Comments</param>
        /// <param name="includeControllerXmlComments">
        /// Flag to indicate if controller XML comments (i.e. summary) should be used to assign Tag descriptions.
        /// Don't set this flag if you're customizing the default tag for operations via TagActionsBy.
        /// </param>
        public static void IncludeXmlComments(
            this SwaggerGenOptions swaggerGenOptions,
            string filePath,
            bool includeControllerXmlComments = false)
        {
            swaggerGenOptions.IncludeXmlComments(() => new XPathDocument(filePath), includeControllerXmlComments);
        }

        /// <summary>
        /// Generate polymorphic schemas (i.e. "oneOf") based on discovered subtypes.
        /// Deprecated: Use the \"UseOneOfForPolymorphism\" and \"UseAllOfForInheritance\" settings instead
        /// </summary>
        /// <param name="swaggerGenOptions"></param>
        /// <param name="subTypesResolver"></param>
        /// <param name="discriminatorSelector"></param>
        [Obsolete("You can use \"UseOneOfForPolymorphism\", \"UseAllOfForInheritance\" and \"SelectSubTypesUsing\" to configure equivalent behavior")]
        public static void GeneratePolymorphicSchemas(
            this SwaggerGenOptions swaggerGenOptions,
            Func<Type, IEnumerable<Type>> subTypesResolver = null,
            Func<Type, string> discriminatorSelector = null)
        {
            swaggerGenOptions.UseOneOfForPolymorphism();
            swaggerGenOptions.UseAllOfForInheritance();

            if (subTypesResolver != null)
            {
                swaggerGenOptions.SelectSubTypesUsing(subTypesResolver);
            }

            if (discriminatorSelector != null)
            {
                swaggerGenOptions.SelectDiscriminatorNameUsing(discriminatorSelector);
            }
        }
    }
}
