using System;
using System.Collections.Generic;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.Generator
{
    public interface ISchemaRegistry
    {
        Schema GetOrRegister(Type type);

        IDictionary<string, Schema> Definitions { get; }
    }
}
