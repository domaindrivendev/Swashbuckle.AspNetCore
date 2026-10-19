# Migrating to Swashbuckle.AspNetCore v11

> [!IMPORTANT]  
> This document describes the breaking changes introduced in Swashbuckle.AspNetCore v11, and how to migrate from v10.x to v11+.

## Why breaking changes?

Swashbuckle.AspNetCore v11 adds support for [ASP.NET Core 11][aspnetcore-11] and for producing documents that use
[version 3.2 of the OpenAPI specification][openapi-specification]. As with OpenAPI 3.1 before it, OpenAPI 3.2 is a minor release
of the specification that nevertheless required a new major version of [Microsoft.OpenApi][microsoft-openapi-package], the library
that Swashbuckle.AspNetCore builds upon: [Microsoft.OpenApi v3][microsoft-openapi-v3-migration-guide]. ASP.NET Core 11 also depends on
Microsoft.OpenApi v3, so Swashbuckle.AspNetCore follows it to avoid version conflicts in applications that use both.

At the same time, v11 drops support for .NET 8 and .NET 9, and upgrades the embedded [Redoc][redoc] UI to [Redoc 3][redoc-v3], which
itself contains breaking changes.

## Supported frameworks

Swashbuckle.AspNetCore v11 targets .NET 10 and .NET 11. Applications that target .NET 8 or .NET 9 should continue to use
[Swashbuckle.AspNetCore v10.x][swashbuckle-aspnetcore-10], which remains the latest version that supports those frameworks.

| Target framework | Microsoft.OpenApi | OpenAPI versions supported |
|:-----------------|:------------------|:---------------------------|
| `net10.0`        | 2.x               | 2.0, 3.0, 3.1              |
| `net11.0`        | 3.x               | 2.0, 3.0, 3.1, 3.2         |

> [!NOTE]
> Swashbuckle.AspNetCore v11 depends on different major versions of Microsoft.OpenApi depending on the target framework of your
> application, as shown in the table above. As a result, some features are only available when targeting .NET 11 or later.

## How do I migrate to Swashbuckle.AspNetCore v11+?

> [!TIP]
> It is strongly recommended that you upgrade to the latest [Swashbuckle.AspNetCore v10.x][swashbuckle-aspnetcore-10] release
> **before** upgrading to v11, so that the changes described in _[Migrating to Swashbuckle.AspNetCore v10][v10-migration]_ are dealt with first.

### Applications targeting .NET 10

If your application targets .NET 10, Swashbuckle.AspNetCore v11 continues to use Microsoft.OpenApi v2.x, so no changes related to
Microsoft.OpenApi should be required. The only breaking changes you are likely to encounter are those described in the
[Redoc 3](#redoc-3) section below if you use the `Swashbuckle.AspNetCore.ReDoc` package.

### Applications targeting .NET 11

If your application targets .NET 11, Swashbuckle.AspNetCore v11 uses Microsoft.OpenApi v3.x. As with the v10 upgrade, the breaking
changes you may encounter depend on how extensively you use the Swashbuckle.AspNetCore extensibility points, such as custom filters,
because the types from Microsoft.OpenApi are exposed in the public API of Swashbuckle.AspNetCore. The
[v3 upgrade guide for Microsoft.OpenApi][microsoft-openapi-v3-migration-guide] should be your primary reference when migrating.

The changes in Microsoft.OpenApi v3 are much smaller in scope than those in v2. The most likely ones to affect Swashbuckle.AspNetCore
users are:

- `OpenApiMediaType` is replaced by the `IOpenApiMediaType` interface in collection properties, for example `OpenApiResponse.Content`
  and `OpenApiRequestBody.Content` are now `IDictionary<string, IOpenApiMediaType>`. Use `OpenApiMediaType` as the concrete type to
  create or mutate media types, in the same way as `IOpenApiSchema` and `OpenApiSchema`.
- The `Attribute` and `Wrapped` properties of `OpenApiXml` are replaced by the `NodeType` property.
- Visitor methods (`OpenApiVisitorBase`) that previously accepted `OpenApiMediaType` now accept `IOpenApiMediaType`.

For example, an `IContentValidator` implementation for the `Swashbuckle.AspNetCore.ApiTesting` package has a different signature depending
on the target framework:

```diff
- void Validate(Microsoft.OpenApi.OpenApiMediaType mediaTypeSpec, Microsoft.OpenApi.OpenApiDocument openApiDocument, System.Net.Http.HttpContent content)
+ void Validate(Microsoft.OpenApi.IOpenApiMediaType mediaTypeSpec, Microsoft.OpenApi.OpenApiDocument openApiDocument, System.Net.Http.HttpContent content)
```

If your application multi-targets .NET 10 and .NET 11, such code may need to use `#if NET11_0_OR_GREATER` to compile for both.

## How do I enable OpenAPI 3.2 support?

By default, to minimise breaking _behavioural_ changes, Swashbuckle.AspNetCore v11+ continues to produce OpenAPI 3.0 documents by default.

When targeting .NET 11 or later, you can opt into version 3.2 of the OpenAPI specification as shown in the code snippet below.
The `Swashbuckle.AspNetCore.Cli` tool also accepts `--openapiversion 3.2` when run with the .NET 11 SDK.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-OpenAPI3.2 -->
<a id='snippet-Swagger-OpenAPI3.2'></a>
```cs
app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_2;
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L234-L239' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-OpenAPI3.2' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

OpenAPI 3.2 introduces the HTTP `QUERY` method as a first-class operation, and allows any other HTTP method to be described using
the new `additionalOperations` keyword. When targeting .NET 11 or later, actions that use the `QUERY` method or any other non-standard
HTTP method (for example with `[AcceptVerbs("QUERY")]` or `[AcceptVerbs("PURGE")]`) are included in the generated documents instead of
causing a `SwaggerGeneratorException` to be thrown. `QUERY` is emitted as a `query` operation and other methods are emitted using
`additionalOperations` in OpenAPI 3.2 documents. In documents using earlier versions of the specification, all such operations are
emitted using the `x-oai-additionalOperations` extension. When targeting .NET 10, such actions still cause a `SwaggerGeneratorException`
to be thrown, as in v10.

> [!NOTE]
> Not all tooling supports OpenAPI 3.2 yet. For example, at the time of writing, [Kiota][kiota-openapi-32] cannot generate clients
> from OpenAPI 3.2 documents that contain `QUERY` operations. Check that the tools consuming your documents support OpenAPI 3.2
> before changing the version.

### C# union types

When targeting .NET 11 or later, Swashbuckle.AspNetCore supports the [C# `union` types][csharp-unions] introduced with .NET 11.
System.Text.Json serializes a union value as whichever of its case types it holds, so the schema generator mirrors this by generating
an `anyOf` schema that references the schema for each case type of the union, matching the behaviour of
[Microsoft.AspNetCore.OpenApi][microsoft-aspnetcore-openapi-package]. No configuration is required, and union types are not supported
when targeting .NET 10. See _[C# unions schema support #4103][swashbuckle-aspnetcore-unions]_ for more details.

### Discriminator default mapping

OpenAPI 3.2 adds a `defaultMapping` field to discriminators. When targeting .NET 11 or later, it can be populated using the new
`SelectDiscriminatorDefaultMappingUsing()` option, or with the `Swashbuckle.AspNetCore.Annotations` package using the new
`DefaultMapping` property of `[SwaggerDiscriminator]`, or `[JsonPolymorphic]` with a fallback `UnknownDerivedTypeHandling` on a
concrete base type. See _[Discriminator Default Mapping][discriminator-default-mapping]_ for more details.

### Swashbuckle.AspNetCore.ApiTesting

`ApiTestRunnerOptions` has a new `OpenApiVersion` property that controls the version of the OpenAPI specification used when
`GenerateOpenApiFiles` is enabled. The default is `OpenApiSpecVersion.OpenApi3_0`, which matches the behaviour of previous versions.

## Redoc 3

The `Swashbuckle.AspNetCore.ReDoc` package now embeds [Redoc 3][redoc-v3], which adds support for OpenAPI 3.2 but contains
[a number of breaking changes][redoc-migration] of its own. The ones that affect Swashbuckle.AspNetCore users are described below.

### Custom `index.html` pages

Redoc 3 is distributed as an ES module and no longer provides the global `Redoc` object. The default `index.html` page now loads the
`index.js` initializer with `<script type="module">`, and `index.js` imports `init` from `redoc.standalone.js`.

If you provide your own `index.html` page using the `IndexStream` option, you must update it to match. A page that loads
`redoc.standalone.js` with a classic `<script>` element and calls `Redoc.init(...)` will render a blank page with Redoc 3.

```diff
- <script src="redoc.standalone.js"></script>
- <script src="index.js"></script>
+ <script type="module" src="index.js"></script>
```

You can use the [default `index.html` page](../src/Swashbuckle.AspNetCore.ReDoc/index.html) as a reference.

### Removed options

Redoc 3 removed a number of configuration options, so the corresponding members of `ConfigObject` and the extension methods for
`ReDocOptions` have been removed from Swashbuckle.AspNetCore:

- `DisableSearch` (search is always available in Redoc 3)
- `ExpandResponses`
- `HideHostname`
- `NativeScrollbars`
- `NoAutoAuth`
- `PathInMiddlePanel`
- `SortPropsAlphabetically`

Any values for these options bound from configuration (for example from `appsettings.json`) are now ignored.

### Renamed options

The following options have been renamed by Redoc 3. The names of the `ConfigObject` properties are unchanged, but they are now
serialized using the new names. If you set the old names using `ConfigObject.AdditionalItems`, you should update them:

| `ConfigObject` property | Redoc 2 option       | Redoc 3 option           |
|:------------------------|:---------------------|:-------------------------|
| `HideDownloadButton`    | `hideDownloadButton` | `hideDownloadButtons`    |
| `RequiredPropsFirst`    | `requiredPropsFirst` | `sortRequiredPropsFirst` |
| `UntrustedSpec`         | `untrustedSpec`      | `sanitize`               |

### Theming

The `theme` option, which could previously be set using `ConfigObject.AdditionalItems`, has been removed by Redoc 3. The theme is now
customized using CSS variables, which can be injected into the page using the `HeadContent` property or the `InjectStylesheet()`
extension method. See _[Inject Custom CSS][redoc-custom-css]_ for an example.

### Telemetry

Redoc 3 [collects anonymous usage telemetry by default][redoc-telemetry]. To turn it off, set the `disableTelemetry` option:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-AdditionalOptions -->
<a id='snippet-Redoc-AdditionalOptions'></a>
```cs
app.UseReDoc(options =>
{
    options.ConfigObject.AdditionalItems = new Dictionary<string, object>
    {
        ["layout"] = "stacked",
        ["disableTelemetry"] = true,
    };
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L176-L185' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-AdditionalOptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

### Deep links

Redoc 3 uses a new format for deep links to operations. Links in the Redoc 2 format are redirected when the page loads, except for
links to schemas and to individual security schemes, which have no equivalent in Redoc 3.

## Migration Overview

Migrating to Swashbuckle.AspNetCore v11+ will likely involve changes in the following areas:

- If your application targets .NET 8 or .NET 9, either upgrade it to .NET 10 or later, or stay on Swashbuckle.AspNetCore v10.x.
- Update any NuGet package references for Swashbuckle.AspNetCore to v11.0.0+.
- If your application targets .NET 11, update any explicit NuGet package references for Microsoft.OpenApi to v3.x and follow the
  [v3 upgrade guide for Microsoft.OpenApi][microsoft-openapi-v3-migration-guide], in particular replacing `OpenApiMediaType` with
  `IOpenApiMediaType` in custom filters and `IContentValidator` implementations.
- If you use `Swashbuckle.AspNetCore.ReDoc`:
  - Update any custom `index.html` page to load `index.js` as an ES module.
  - Remove any use of the removed `ConfigObject` properties and `ReDocOptions` extension methods listed above.
  - Update any use of the renamed Redoc options in `ConfigObject.AdditionalItems`.
  - Replace any use of the `theme` option with CSS variables.
  - Decide whether to disable Redoc's telemetry.
- Optionally, opt into OpenAPI 3.2 when targeting .NET 11 or later.

<!-- TODO Update this link to the "What's new in ASP.NET Core 11.0" release notes once published, before merging #3786 -->
[aspnetcore-11]: https://learn.microsoft.com/aspnet/core/ "ASP.NET Core documentation"
[csharp-unions]: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union "C# union types"
[discriminator-default-mapping]: configure-and-customize-swaggergen.md#discriminator-default-mapping "Discriminator Default Mapping"
[kiota-openapi-32]: https://github.com/microsoft/kiota-dotnet/pull/805 "Kiota support for OpenAPI 3.2"
[microsoft-aspnetcore-openapi-package]: https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi/ "Microsoft.AspNetCore.OpenApi NuGet package"
[microsoft-openapi-package]: https://www.nuget.org/packages/Microsoft.OpenApi/ "Microsoft.OpenApi NuGet package"
[microsoft-openapi-v3-migration-guide]: https://github.com/microsoft/OpenAPI.NET/blob/main/docs/upgrade-guide-3.md "Microsoft OpenAPI.NET v3 upgrade guide"
[openapi-specification]: https://spec.openapis.org/oas/v3.2.0.html "OpenAPI Specification v3.2.0"
[redoc]: https://github.com/Redocly/redoc "The Redoc project in GitHub"
[redoc-custom-css]: configure-and-customize-redoc.md#inject-custom-css "Inject Custom CSS"
[redoc-migration]: https://redocly.com/docs/redoc/config-migration "Migration from Redoc CE 2.x to 3.x"
[redoc-telemetry]: https://redocly.com/docs/redoc/v3.x/telemetry "Redoc telemetry"
[redoc-v3]: https://github.com/Redocly/redoc/releases/tag/redoc%403.0.0 "Redoc 3.0.0 release notes"
<!-- TODO Update this link to the latest v10.x release before merging #3786 -->
[swashbuckle-aspnetcore-10]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v10.3.1 "Swashbuckle.AspNetCore v10.3.1"
[swashbuckle-aspnetcore-unions]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/4103 "C# unions schema support"
[v10-migration]: migrating-to-v10.md "Migrating to Swashbuckle.AspNetCore v10"
