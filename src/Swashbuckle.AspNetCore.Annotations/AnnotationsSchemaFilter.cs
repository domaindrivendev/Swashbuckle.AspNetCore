using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations;

public class AnnotationsSchemaFilter(IServiceProvider serviceProvider) : ISchemaFilter
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ApplyTypeAnnotations(schema, context);

        // NOTE: It's possible for both MemberInfo and ParameterInfo to have non-null values - i.e. when the schema is for a property
        // within a class that is bound to a parameter. In this case, the MemberInfo should take precedence.

        if (context.MemberInfo != null)
        {
            ApplyMemberAnnotations(schema, context.MemberInfo);
        }
        else if (context.ParameterInfo != null)
        {
            ApplyParamAnnotations(schema, context.ParameterInfo);
        }
    }

    private void ApplyTypeAnnotations(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var schemaAttribute = context.Type.GetCustomAttributes<SwaggerSchemaAttribute>()
            .FirstOrDefault();

        if (schemaAttribute != null)
        {
            ApplySchemaAttribute(schema, schemaAttribute);
        }

        var schemaFilterAttribute = context.Type.GetCustomAttributes<SwaggerSchemaFilterAttribute>()
            .FirstOrDefault();

        if (schemaFilterAttribute != null)
        {
            var filter = (ISchemaFilter)ActivatorUtilities.CreateInstance(
                _serviceProvider,
                schemaFilterAttribute.Type,
                schemaFilterAttribute.Arguments);

            filter.Apply(schema, context);
        }
    }

    private static void ApplyParamAnnotations(IOpenApiSchema schema, ParameterInfo parameterInfo)
    {
        var schemaAttribute = parameterInfo.GetCustomAttributes<SwaggerSchemaAttribute>()
            .FirstOrDefault();

        if (schemaAttribute != null)
        {
            ApplySchemaAttribute(schema, schemaAttribute);
        }
    }

    private static void ApplyMemberAnnotations(IOpenApiSchema schema, MemberInfo memberInfo)
    {
        var schemaAttribute = memberInfo.GetCustomAttributes<SwaggerSchemaAttribute>()
            .FirstOrDefault();

        if (schemaAttribute != null)
        {
            ApplySchemaAttribute(schema, schemaAttribute);
        }
    }

    private static void ApplySchemaAttribute(IOpenApiSchema schema, SwaggerSchemaAttribute schemaAttribute)
    {
        if (schemaAttribute.Description is { } description)
        {
            schema.Description = description;
        }

        if (schema is not OpenApiSchema concrete)
        {
            return;
        }

        if (schemaAttribute.Format is { } format)
        {
            concrete.Format = format;
        }

        if (schemaAttribute.ReadOnlyFlag is { } readOnly)
        {
            concrete.ReadOnly = readOnly;
        }

        if (schemaAttribute.WriteOnlyFlag is { } writeOnly)
        {
            concrete.WriteOnly = writeOnly;
        }

        if (schemaAttribute.NullableFlag is { } nullable)
        {
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3387
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3936
            //
            // Where nullability is recorded depends on the shape of the schema, and each branch
            // below has a mirror image in the non-nullable case:
            //   * allOf: on "type", as adding a null member breaks the OpenAPI 3.0 output
            //   * anyOf/oneOf: as an extra "null" member, as the composition constrains the value
            //   * otherwise: on "type", but only when a concrete type is already present
            if (nullable)
            {
                if (concrete.AllOf is { Count: > 0 })
                {
                    // Do not restructure the composition here: wrapping the allOf in an anyOf
                    // prevents client generators from resolving the referenced schema when the
                    // document is serialized as OpenAPI 3.0, where the null member is dropped.
                    // This makes the OpenAPI 3.1 schema contradictory (the value must be null
                    // and satisfy the allOf), which is accepted so that the 3.0 output stays
                    // idiomatic and usable by client generators.
                    concrete.Type ??= JsonSchemaType.Null;
                    concrete.Type |= JsonSchemaType.Null;
                }
                else if (concrete.AnyOf is { Count: > 0 } anyOf)
                {
                    TryAddNullSchema(anyOf);
                }
                else if (concrete.OneOf is { Count: > 0 } oneOf)
                {
                    TryAddNullSchema(oneOf);
                }
                else if (concrete.Type.HasValue)
                {
                    // A schema without a "type" already validates every JSON type, including null,
                    // so the null flag is only added when a concrete type is present. That reasoning
                    // does not extend to the compositions above: their members restrict which types
                    // are valid, so an absent "type" does not admit null and it must be added.
                    concrete.Type |= JsonSchemaType.Null;
                }
            }
            else if (concrete.AnyOf is { Count: > 0 } anyOf)
            {
                TryRemoveNullSchema(anyOf);
            }
            else if (concrete.OneOf is { Count: > 0 } oneOf)
            {
                TryRemoveNullSchema(oneOf);
            }
            else if (concrete.Type.HasValue)
            {
                // An allOf composition falls through to here, as "type" is where the
                // nullable case above records its nullability.
                concrete.Type &= ~JsonSchemaType.Null;
            }
        }

        if (schemaAttribute.Required is { } required)
        {
            concrete.Required = new SortedSet<string>(required);
        }

        if (schemaAttribute.Title is { } title)
        {
            concrete.Title = title;
        }
    }

    private static void TryAddNullSchema(IList<IOpenApiSchema> schemas)
    {
        if (!schemas.Any(static s => s.Type is { } type && type.HasFlag(JsonSchemaType.Null)))
        {
            schemas.Add(new OpenApiSchema { Type = JsonSchemaType.Null });
        }
    }

    private static void TryRemoveNullSchema(IList<IOpenApiSchema> schemas)
    {
        for (var i = schemas.Count - 1; i >= 0; i--)
        {
            if (schemas[i] is not OpenApiSchema { Type: { } type } member ||
                !type.HasFlag(JsonSchemaType.Null))
            {
                continue;
            }

            if (type == JsonSchemaType.Null)
            {
                // A lone null member is left in place, as an empty anyOf/oneOf is not valid.
                if (schemas.Count > 1)
                {
                    schemas.RemoveAt(i);
                }
            }
            else
            {
                // The member permits other types too, so only the null flag is cleared.
                member.Type = type & ~JsonSchemaType.Null;
            }
        }
    }
}
