using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class SwaggerOptions
    {
        public SwaggerOptions()
        {
            PreSerializeFilters = new List<Action<SwaggerDocument, HttpRequest>>();
        }

        public string RouteTemplate { get; set; } = "swagger/{documentName}/swagger.json";

        public List<Action<SwaggerDocument, HttpRequest>> PreSerializeFilters { get; private set; }
    }
}
