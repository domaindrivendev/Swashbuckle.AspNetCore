# Migrating to Swashbuckle.AspNetCore v11

> [!IMPORTANT]  
> This document describes the breaking changes introduced in Swashbuckle.AspNetCore v11, and how to migrate from v10.x to v11+.

## Why breaking changes?

TODO Explain the rationale for breaking changes in v11 due to Microsoft.OpenApi v3.x.

TODO Investigate whether things work OK with ASP.NET Core 8/9/10.

## How do I enable OpenAPI 3.2 support?

By default, to minimise breaking _behavioural_ changes, Swashbuckle.AspNetCore v11+ will continue to produce OpenAPI 3.0 documents by default.

To upgrade your OpenAPI documents to output using version 3.2 of the OpenAPI specification, you can override the version as shown in the code snippet below.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-OpenAPI3.2 -->
<a id='snippet-Swagger-OpenAPI3.2'></a>
```cs
app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_2;
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L225-L230' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-OpenAPI3.2' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## How do I migrate to Swashbuckle.AspNetCore v11+?

> [!TIP]
> It is strongly recommended that you upgrade to [Swashbuckle.AspNetCore v10.0.0][swashbuckle-aspnetcore-1000] **before** upgrading to v11.

The majority of the breaking changes you may encounter when migrating to Swashbuckle.AspNetCore v11+ are due to the underlying
changes in Microsoft.OpenApi v3.0.0+. The [v3 migration guide for Microsoft.OpenApi][microsoft-openapi-v3-migration-guide] provides a
detailed overview of the breaking changes and how to migrate your code from Microsoft.OpenApi v2.x to v3.x. The Microsoft.OpenApi
breaking changes document should be your primary reference when migrating to Swashbuckle.AspNetCore v11+.

The chances of encountering breaking changes when migrating to Swashbuckle.AspNetCore v11+ depend on how extensively you use the
Swashbuckle.AspNetCore extensibility points, such as custom filters, to generate the OpenAPI document. If you do not use any custom logic
and do not depend on any other third-party libraries that depend on Microsoft.OpenApi v1.x or v2.x, you may not encounter any breaking changes at all.

## Migration Overview

Migrating to Swashbuckle.AspNetCore v11+ will likely involve changes in the following areas:

- Update any NuGet package references for Swashbuckle.AspNetCore and Microsoft.OpenApi to v11.0.0+ and v3.0.0+ respectively.
- Update model references (e.g. `OpenApiMediaType`) to use the new interfaces (e.g. `IOpenApiMediaType`) and the relevant concrete types to mutate them (e.g. `OpenApiMediaType`).

[microsoft-openapi-v3-migration-guide]: https://github.com/microsoft/OpenAPI.NET/blob/main/docs/upgrade-guide-3.md "Microsoft OpenAPI.NET v3 migration guide"
[swashbuckle-aspnetcore-1000]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v10.0.0 "Swashbuckle.AspNetCore v10.0.0"
