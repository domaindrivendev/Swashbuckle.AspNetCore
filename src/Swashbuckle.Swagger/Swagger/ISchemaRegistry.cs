using System;
using System.Collections.Generic;

namespace Swashbuckle.Swagger
{
    public interface ISchemaRegistry
    {
        Schema GetOrRegister(Type type);

        IDictionary<string, Schema> Definitions { get; }
    }
}