using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class OpenApiAnyHelper
    {
        public static IOpenApiAny ConvertToOpenApiType(Type memberType, OpenApiSchema schema, string stringValue)
        {
            object typedValue;

            try
            {
                typedValue = TypeDescriptor.GetConverter(memberType).ConvertFrom(
                    context: null,
                    culture: CultureInfo.InvariantCulture,
                    stringValue);
            }
            catch (Exception)
            {
                return null;
            }

            return OpenApiAnyFactory.CreateFor(schema, typedValue);
        }
    }
}