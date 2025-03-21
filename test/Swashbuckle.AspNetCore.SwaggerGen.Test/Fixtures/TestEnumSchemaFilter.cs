using System.Text;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

internal class TestEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        bool isEnumArgument = (context.Type?.GenericTypeArguments?.Length ?? 0) == 1 && context.Type.GenericTypeArguments.All(b => b.IsEnum);
        var isEnumArray = context.Type.IsArray && context.Type.GetElementType().IsEnum;
        if (context.Type.IsEnum || isEnumArgument || isEnumArray)
        {
            var enumType = (context.Type.IsEnum, isEnumArgument, isEnumArray) switch
            {
                (true, _, _) => context.Type,
                (_, true, _) => context.Type.GenericTypeArguments.First(),
                _ => context.Type.GetElementType()
            };
            StringBuilder stringBuilder = new("<p>Members:</p><ul>");
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                if (enumValue is Enum value)
                {
                    stringBuilder.Append($"<li>{value} - {value:d}</li>");
                }
            }
            schema.Description = stringBuilder.Append("</ul>").ToString();
        }
    }
}
