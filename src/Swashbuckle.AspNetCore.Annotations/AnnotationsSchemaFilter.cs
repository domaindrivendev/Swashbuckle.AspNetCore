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
            if (nullable)
            {
                if (concrete.AllOf is { Count: > 0 })
                {
                    // Do not restructure the composition here: wrapping the allOf in an anyOf
                    // prevents client generators from resolving the referenced schema when the
                    // document is serialized as OpenAPI 3.0, where the null member is dropped.
                    concrete.Type ??= JsonSchemaType.Null;
                    concrete.Type |= JsonSchemaType.Null;
                }
                else if (concrete.AnyOf is { Count: > 0 } anyOf)
                {
                    AddNullSchema(anyOf);
                }
                else if (concrete.OneOf is { Count: > 0 } oneOf)
                {
                    AddNullSchema(oneOf);
                }
                else if (concrete.Type.HasValue)
                {
                    // A schema without a "type" already validates every JSON type, including null,
                    // so the null flag is only added when a concrete type is present.
                    concrete.Type |= JsonSchemaType.Null;
                }
            }
            else if (concrete.Type.HasValue)
            {
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

    private static void AddNullSchema(IList<IOpenApiSchema> schemas)
    {
        // Only add a null member if the composition does not already allow null,
        // otherwise a schema that is nullable through multiple mechanisms (e.g. a
        // nullable reference type also annotated with [SwaggerSchema(Nullable = true)])
        // would end up with the null type listed more than once.
        if (!schemas.Any(static s => s.Type is { } type && type.HasFlag(JsonSchemaType.Null)))
        {
            schemas.Add(new OpenApiSchema { Type = JsonSchemaType.Null });
        }
    }
}
