using System;
using System.Linq;
using System.Text;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger
{
    public class EnumSchemaFilter : ISchemaFilter
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
                OpenApiArray names = [];
                foreach (var enumValue in Enum.GetValues(enumType))
                {
                    if (enumValue is Enum value)
                    {
                        names.Add(new OpenApiString(value.ToString()));
                        stringBuilder.Append($"<li>{value} - {value:d}</li>");
                    }
                }
                schema.Extensions.Add("x-enum-varnames", names);
                schema.Description = stringBuilder.Append("</ul>").ToString();
            }
        }
    }
}
