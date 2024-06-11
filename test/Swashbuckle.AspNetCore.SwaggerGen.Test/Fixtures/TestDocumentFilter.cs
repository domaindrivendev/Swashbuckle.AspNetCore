﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TestDocumentFilter : IDocumentFilter, IDocumentAsyncFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions.Add("X-foo", new OpenApiString("bar"));
            swaggerDoc.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
            context.SchemaGenerator.GenerateSchema(typeof(ComplexType), context.SchemaRepository);
        }

        public Task ApplyAsync(OpenApiDocument swaggerDoc, DocumentFilterContext context, CancellationToken cancellationToken)
        {
            Apply(swaggerDoc, context);
            return Task.CompletedTask;
        }
    }
}
