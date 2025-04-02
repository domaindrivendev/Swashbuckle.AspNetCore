using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations;

public class AnnotationsSchemaFilter(IServiceProvider serviceProvider) : ISchemaFilter
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
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

    private void ApplyTypeAnnotations(OpenApiSchema schema, SchemaFilterContext context)
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

    private static void ApplyParamAnnotations(OpenApiSchema schema, ParameterInfo parameterInfo)
    {
        var schemaAttribute = parameterInfo.GetCustomAttributes<SwaggerSchemaAttribute>()
            .FirstOrDefault();

        if (schemaAttribute != null)
        {
            ApplySchemaAttribute(schema, schemaAttribute);
        }
    }

    private static void ApplyMemberAnnotations(OpenApiSchema schema, MemberInfo memberInfo)
    {
        var schemaAttribute = memberInfo.GetCustomAttributes<SwaggerSchemaAttribute>()
            .FirstOrDefault();

        if (schemaAttribute != null)
        {
            ApplySchemaAttribute(schema, schemaAttribute);
        }
    }

    private static void ApplySchemaAttribute(OpenApiSchema schema, SwaggerSchemaAttribute schemaAttribute)
    {
        if (schemaAttribute.Description is { } description)
        {
            schema.Description = description;
        }

        if (schemaAttribute.Format is { } format)
        {
            schema.Format = format;
        }

        if (schemaAttribute.ReadOnlyFlag is { } readOnly)
        {
            schema.ReadOnly = readOnly;
        }

        if (schemaAttribute.WriteOnlyFlag is { } writeOnly)
        {
            schema.WriteOnly = writeOnly;
        }

        if (schemaAttribute.NullableFlag is { } nullable)
        {
            schema.Nullable = nullable;
        }

        if (schemaAttribute.Required is { } required)
        {
            schema.Required = new SortedSet<string>(required);
        }

        if (schemaAttribute.Title is { } title)
        {
            schema.Title = title;
        }
    }
}
