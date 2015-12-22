using System;
using System.Collections.Generic;

namespace Swashbuckle.SwaggerGen.Generator
{
    public interface ISchemaRegistry
    {
        Schema GetOrRegister(Type type);

        IDictionary<string, Schema> Definitions { get; }
    }
}
