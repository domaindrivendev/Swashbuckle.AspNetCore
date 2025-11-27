# Migrating to Swashbuckle.AspNetCore v10

> [!IMPORTANT]  
> This document describes the breaking changes introduced in Swashbuckle.AspNetCore v10, and how to migrate from v9.x to v10+.

## Why breaking changes?

While the [OpenAPI 3.1 specification][openapi-specification] is a minor release compared to OpenAPI 3.0, the OpenAPI specification does
not use Semantic Versioning (SemVer). The changes introduced between the two versions are quite breaking in a practical sense, so major
changes were required to be made to [Microsoft.OpenApi][microsoft-openapi-package], the package which Swashbuckle.AspNetCore builds upon,
in order to allow applications to produce OpenAPI 3.1 documents.

These changes were introduced in [Microsoft.OpenApi v2.0.0][microsoft-openapi-v2-migration-guide], which _does_ follow SemVer. As a result,
Swashbuckle.AspNetCore v10+ now depends on Microsoft.OpenApi v2+ to allow users to produce OpenAPI 3.1 documents, fulfilling a
long-standing feature request: [Plans on official support for OpenApi 3.1.0 #2349][feature-request].

These changes are unfortunately required, even if you still wish to target Swagger 2.0 or OpenAPI 3.0 documents, as the same library is used
to produce all three document format versions.

For the same breaking changes, ASP.NET Core v10+ also depends on Microsoft.OpenApi v2+, so these changes were also required to allow applications
using ASP.NET Core 10 to use Swashbuckle.AspNetCore effectively with minimal friction. This also helps support users who may wish to migrate an
application from Swashbuckle.AspNetCore to Microsoft.AspNetCore.OpenApi (for example if they need native AoT support). More information about the
breaking changes in ASP.NET Core 10 can be found in this document: _[What's new in ASP.NET Core in .NET 10][breaking-changes-aspnetcore]_.

The refactoring required to support OpenAPI 3.1 in Swashbuckle.AspNetCore was significant. If you're interested in what exactly what was changed,
you can check out the PR to implement it that was worked on over the course of .NET 10's development (it's quite large): _[Support .NET 10 #3283][swashbuckle-aspnetcore-10]_.

## How do I enable OpenAPI 3.1 support?

By default, to minimise breaking _behavioural_ changes, Swashbuckle.AspNetCore v10+ will continue to produce OpenAPI 3.0 documents by default.

To upgrade your OpenAPI documents to output using version 3.1 of the OpenAPI specification, you can override the version as shown in the code snippet below.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-OpenAPI3.1 -->
<a id='snippet-Swagger-OpenAPI3.1'></a>
```cs
app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L218-L223' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-OpenAPI3.1' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## How do I migrate to Swashbuckle.AspNetCore v10+?

> [!TIP]
> It is strongly recommended that you upgrade to [Swashbuckle.AspNetCore v9.0.6][swashbuckle-aspnetcore-906] **before** upgrading to v10.

The majority of the breaking changes you may encounter when migrating to Swashbuckle.AspNetCore v10+ are due to the underlying
changes in Microsoft.OpenApi v2.0.0+. The [v2 migration guide for Microsoft.OpenApi][microsoft-openapi-v2-migration-guide] provides a
detailed overview of the breaking changes and how to migrate your code from Microsoft.OpenApi v1.x to v2.x. The Microsoft.OpenApi
breaking changes document should be your primary reference when migrating to Swashbuckle.AspNetCore v10+.

The chances of encountering breaking changes when migrating to Swashbuckle.AspNetCore v10+ depend on how extensively you use the
Swashbuckle.AspNetCore extensibility points, such as custom filters, to generate the OpenAPI document. If you do not use any custom logic
and do not depend on any other third-party libraries that depend on Microsoft.OpenApi v1.x, you may not encounter any breaking changes at all.

In cases where Swashbuckle.AspNetCore has a breaking change, this is because the types from the underlying Microsoft.OpenApi library
are exposed in the public API of Swashbuckle.AspNetCore, so the breaking change bubbles up through the public API surface to cause compilation
issues in your code. For example the signature for `IRequestBodyAsyncFilter.ApplyAsync()` has changed as shown below:

```diff
- Swashbuckle.AspNetCore.SwaggerGen.IRequestBodyAsyncFilter.ApplyAsync(Microsoft.OpenApi.Models.OpenApiRequestBody requestBody, Swashbuckle.AspNetCore.SwaggerGen.RequestBodyFilterContext context, System.Threading.CancellationToken cancellationToken) -> System.Threading.Tasks.Task
+ Swashbuckle.AspNetCore.SwaggerGen.IRequestBodyAsyncFilter.ApplyAsync(Microsoft.OpenApi.IOpenApiRequestBody requestBody, Swashbuckle.AspNetCore.SwaggerGen.RequestBodyFilterContext context, System.Threading.CancellationToken cancellationToken) -> System.Threading.Tasks.Task
```

## Migration Overview

Migrating to Swashbuckle.AspNetCore v10+ will likely involve changes in the following areas:

- Update any NuGet package references for Swashbuckle.AspNetCore and Microsoft.OpenApi to v10.0.0+ and v2.3.0+ respectively.
- Update any `using` directives that reference types from the `Microsoft.OpenApi.Models` namespace to use the new namespace `Microsoft.OpenApi`.
- Update model references (e.g. `OpenApiSchema`) to use the new interfaces (e.g. `IOpenApiSchema`) and use the relevant concrete types to mutate them (e.g. `OpenApiSchema`). An example of this is shown below for an `ISchemaFilter` implementation:

   ```csharp
  public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
  {
       if (schema is OpenApiSchema openApiSchema)
       {
            // The properties are only mutable on the concrete type
            openApiSchema.Type = JsonSchemaType.String;
       }
   }     
   ```

- Update any use of `.Reference` properties (e.g. `OpenApiSchema.ReferenceV3`) to use the new `*Reference` class instead (e.g. `OpenApiSchemaReference`).
- Replace usage of the `OpenApiSchema.Type` property using a string (e.g. `"string"` or `"boolean"`) with the `JsonSchemaType` flags enumeration.
- Replace usage of the `OpenApiSchema.Nullable` property by OR-ing the `JsonSchemaType.Null` value to `OpenApiSchema.Type` (e.g. `schema.Type |= JsonSchemaType.Null;`).
- Remove any use of the [now-deprecated `WithOpenApi()` extension method][withopenapi-deprecation] in [Microsoft.AspNetCore.OpenApi][microsoft-aspnetcore-openapi-package].
- Updating any use of `AddSecurityRequirement()` to use a `Func<OpenApiDocument, OpenApiSecurityRequirement>` ([documentation][security]).
- Updating some collections to use specific concrete types instead of interfaces if collection expressions were used, for example using `HashSet<OpenApiTag>` with `OpenApiDocument.Tags` which is now an `ISet<OpenApiTag>`.

[breaking-changes-aspnetcore]: https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0?#openapi-31-breaking-changes "OpenAPI 3.1 breaking changes"
[feature-request]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2349 "Plans on official support for OpenApi 3.1.0"
[microsoft-aspnetcore-openapi-package]: https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/ "Microsoft.AspNetCore.OpenApi NuGet package"
[microsoft-openapi-package]: https://www.nuget.org/packages/Microsoft.OpenApi/ "Microsoft.OpenApi NuGet package"
[microsoft-openapi-v2-migration-guide]: https://github.com/microsoft/OpenAPI.NET/blob/main/docs/upgrade-guide-2.md "Microsoft OpenAPI.NET v2 migration guide"
[openapi-specification]: https://swagger.io/specification/ "OpenAPI Specification"
[security]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/v10.0.0/docs/configure-and-customize-swaggergen.md#add-security-definitions-and-requirements "Add Security Definitions and Requirements"
[swashbuckle-aspnetcore-906]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v9.0.6 "Swashbuckle.AspNetCore v9.0.6"
[swashbuckle-aspnetcore-10]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/3283 "Support .NET 10"
[withopenapi-deprecation]: https://github.com/aspnet/Announcements/issues/519 "[Breaking change]: Deprecation of WithOpenApi extension method"
