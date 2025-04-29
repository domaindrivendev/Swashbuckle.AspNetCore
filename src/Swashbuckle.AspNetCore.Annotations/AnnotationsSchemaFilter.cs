using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
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
            if (nullable)
            {
                concrete.Type |= JsonSchemaType.Null;
            }
            else
            {
                concrete.Type &= ~JsonSchemaType.Null;
            }
        }

        if (schemaAttribute.Required is { } required)
        {
            if (required.Length < 2)
            {
                concrete.Required = [.. required];
            }
            else
            {
                concrete.Required = [.. new SortedSet<string>(required)];
            }
        }

        if (schemaAttribute.Title is { } title)
        {
            concrete.Title = title;
        }
    }
}
