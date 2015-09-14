using System;
using System.Collections.Generic;

namespace Swashbuckle.Swagger
{
    public interface ISchemaProvider
    {
        Schema GetSchema(Type type, IDictionary<string, Schema> definitions);
    }
}
