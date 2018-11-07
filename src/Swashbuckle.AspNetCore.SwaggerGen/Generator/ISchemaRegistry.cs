using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaRegistry
    {
        OpenApiSchema GetOrRegister(Type type);

        IDictionary<string, OpenApiSchema> Schemas { get; }
    }
}
