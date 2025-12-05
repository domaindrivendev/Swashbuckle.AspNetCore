# Swashbuckle.AspNetCore

[![NuGet][package-badge-version]][package-download]
[![NuGet Downloads][package-badge-downloads]][package-download]

[![Build status][build-badge]][build-status]
[![Code coverage][coverage-badge]][coverage-report]
[![OpenSSF Scorecard][scorecard-badge]][scorecard-report]

[![Help Wanted][help-wanted-badge]][help-wanted-issues]

[OpenAPI][swagger] (Swagger) tooling for APIs built with ASP.NET Core.

Generate beautiful API documentation, including a UI to explore and test operations, directly from your application code.

In addition to its [Swagger 2.0 and OpenAPI 3.0/3.1][swagger-specification] generator, Swashbuckle.AspNetCore also provides
an embedded version of the awesome [swagger-ui][swagger-ui] project that's powered by the generated OpenAPI JSON documents.
This means you can complement your API with living documentation that's always in sync with the latest code. Best of all, it
requires minimal coding and maintenance, allowing you to focus on building an awesome API.

But that's not all!

Once you have an API that can describe itself with a OpenAPI document, you've opened the treasure chest of OpenAPI-based
tools including a client generator that can be targeted to a wide range of popular platforms. See [swagger-codegen][swagger-codegen]
for more details.

> [!IMPORTANT]  
> Version 10.0 of Swashbuckle.AspNetCore introduces breaking changes due to upgrading our dependency on [Microsoft.OpenApi][microsoft-openapi]
> to version 2.x.x to add support for generating OpenAPI 3.1 documents. Please see _[Migrating to Swashbuckle.AspNetCore v10][v10-migration]_ for more details.

## Compatibility

| **Swashbuckle Version**                                                                                          | **ASP.NET Core**           | **OpenAPI/Swagger Versions** | **Microsoft.OpenApi**                                                                        | **swagger-ui**                                                                            | **Redoc**                                                                    |
|:-----------------------------------------------------------------------------------------------------------------|:---------------------------|:-----------------------------|:---------------------------------------------------------------------------------------------|:------------------------------------------------------------------------------------------|:-----------------------------------------------------------------------------|
| [![CI Swashbuckle.AspNetCore version][swashbuckle-version-vnext-badge]][swashbuckle-version-vnext-release]       | >= 8.0.0                   | 3.1, **3.0**, 2.0            | [![Microsoft.OpenApi version][openapi-version-vnext-badge]][openapi-version-vnext-release]   | [![swagger-ui version][swaggerui-version-vnext-badge]][swaggerui-version-vnext-release]   | [![Redoc version][redoc-version-vnext-badge]][redoc-version-vnext-release]   |
| [![Latest Swashbuckle.AspNetCore version][swashbuckle-version-latest-badge]][swashbuckle-version-latest-release] | >= 8.0.0                   | 3.1, **3.0**, 2.0            | [![Microsoft.OpenApi version][openapi-version-latest-badge]][openapi-version-latest-release] | [![swagger-ui version][swaggerui-version-latest-badge]][swaggerui-version-latest-release] | [![Redoc version][redoc-version-latest-badge]][redoc-version-latest-release] |
| [![Last v9 Swashbuckle.AspNetCore version][swashbuckle-version-v9-badge]][swashbuckle-version-v9-release]        | 9.0.x, 8.0.x               | **3.0**, 2.0                 | [![Microsoft.OpenApi version][openapi-version-v9-badge]][openapi-version-v9-release]         | [![swagger-ui version][swaggerui-version-v9-badge]][swaggerui-version-v9-release]         | [![Redoc version][redoc-version-v9-badge]][redoc-version-v9-release]         |
| [![Last v8 Swashbuckle.AspNetCore version][swashbuckle-version-v8-badge]][swashbuckle-version-v8-release]        | 9.0.x, 8.0.x, 2.3.x        | **3.0**, 2.0                 | [![Microsoft.OpenApi version][openapi-version-v8-badge]][openapi-version-v8-release]         | [![swagger-ui version][swaggerui-version-v8-badge]][swaggerui-version-v8-release]         | [![Redoc version][redoc-version-v8-badge]][redoc-version-v8-release]         |

[swashbuckle-version-vnext-badge]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2FHEAD%2FDirectory.Build.props&query=(%2F%2FProject%2FPropertyGroup%2FVersionPrefix)%5B1%5D&prefix=v&suffix=-*&logo=github&label=CI
[swashbuckle-version-vnext-release]: https://www.myget.org/gallery/domaindrivendev
[swashbuckle-version-latest-badge]: https://img.shields.io/github/v/release/domaindrivendev/Swashbuckle.AspNetCore?display_name=tag&logo=github&label=v10
[swashbuckle-version-latest-release]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/latest
[swashbuckle-version-v9-badge]: https://img.shields.io/badge/v9-v9.0.6-blue?logo=github
[swashbuckle-version-v9-release]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v9.0.6
[swashbuckle-version-v8-badge]: https://img.shields.io/badge/v8-v8.1.4-blue?logo=github
[swashbuckle-version-v8-release]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v8.1.4

[openapi-version-vnext-badge]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2FHEAD%2Fsrc%2FSwashbuckle.AspNetCore.Swagger%2FSwashbuckle.AspNetCore.Swagger.csproj&query=%2F%2FPackageReference%5B%40Include%3D'Microsoft.OpenApi'%5D%2F%40VersionOverride&logo=openapiinitiative&label=Microsoft.OpenApi
[openapi-version-vnext-release]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/HEAD/src/Swashbuckle.AspNetCore.Swagger/Swashbuckle.AspNetCore.Swagger.csproj#L20
[openapi-version-latest-badge]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv10.0.1%2Fsrc%2FSwashbuckle.AspNetCore.Swagger%2FSwashbuckle.AspNetCore.Swagger.csproj&query=%2F%2FPackageReference%5B%40Include%3D'Microsoft.OpenApi'%5D%2F%40VersionOverride&logo=openapiinitiative&label=Microsoft.OpenApi
[openapi-version-latest-release]: https://github.com/microsoft/OpenAPI.NET/releases/tag/v2.3.0
[openapi-version-v9-badge]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv9.0.6%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'Microsoft.OpenApi'%5D%2F%40Version&logo=openapiinitiative&label=Microsoft.OpenApi
[openapi-version-v9-release]: https://github.com/microsoft/OpenAPI.NET/releases/tag/v1.6.25
[openapi-version-v8-badge]: https://img.shields.io/badge/dynamic/xml?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv8.1.4%2FDirectory.Packages.props&query=%2F%2FPackageVersion%5B%40Include%3D'Microsoft.OpenApi'%5D%2F%40Version&logo=openapiinitiative&label=Microsoft.OpenApi
[openapi-version-v8-release]: https://github.com/microsoft/OpenAPI.NET/releases/tag/1.6.23

[swaggerui-version-vnext-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2FHEAD%2Fsrc%2FSwashbuckle.AspNetCore.SwaggerUI%2Fpackage.json&query=%24.dependencies.swagger-ui-dist&style=flat&label=swagger-ui
[swaggerui-version-vnext-release]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/HEAD/src/Swashbuckle.AspNetCore.SwaggerUI/package.json#L6
[swaggerui-version-latest-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv10.0.1%2Fsrc%2FSwashbuckle.AspNetCore.SwaggerUI%2Fpackage.json&query=%24.dependencies.swagger-ui-dist&style=flat&label=swagger-ui
[swaggerui-version-latest-release]: https://github.com/swagger-api/swagger-ui/releases/tag/v5.30.2
[swaggerui-version-v9-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv9.0.6%2Fsrc%2FSwashbuckle.AspNetCore.SwaggerUI%2Fpackage.json&query=%24.dependencies.swagger-ui-dist&style=flat&label=swagger-ui
[swaggerui-version-v9-release]: https://github.com/swagger-api/swagger-ui/releases/tag/v5.29.2
[swaggerui-version-v8-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv8.1.4%2Fsrc%2FSwashbuckle.AspNetCore.SwaggerUI%2Fpackage.json&query=%24.dependencies.swagger-ui-dist&style=flat&label=swagger-ui
[swaggerui-version-v8-release]: https://github.com/swagger-api/swagger-ui/releases/tag/v5.22.0

[redoc-version-vnext-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2FHEAD%2Fsrc%2FSwashbuckle.AspNetCore.ReDoc%2Fpackage.json&query=%24.dependencies.redoc&style=flat&label=Redoc
[redoc-version-vnext-release]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/HEAD/src/Swashbuckle.AspNetCore.ReDoc/package.json#L6
[redoc-version-latest-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv10.0.1%2Fsrc%2FSwashbuckle.AspNetCore.ReDoc%2Fpackage.json&query=%24.dependencies.redoc&style=flat&label=Redoc
[redoc-version-latest-release]: https://github.com/Redocly/redoc/releases/tag/v2.5.2
[redoc-version-v9-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv9.0.6%2Fsrc%2FSwashbuckle.AspNetCore.ReDoc%2Fpackage.json&query=%24.dependencies.redoc&style=flat&label=Redoc
[redoc-version-v9-release]: https://github.com/Redocly/redoc/releases/tag/v2.5.1
[redoc-version-v8-badge]: https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fraw.githubusercontent.com%2Fdomaindrivendev%2FSwashbuckle.AspNetCore%2Frefs%2Ftags%2Fv8.1.4%2Fsrc%2FSwashbuckle.AspNetCore.ReDoc%2Fpackage.json&query=%24.dependencies.redoc&style=flat&label=Redoc
[redoc-version-v8-release]: https://github.com/Redocly/redoc/releases/tag/v2.5.0

## Getting Started

First install the kitchen-sink NuGet package into your ASP.NET Core application:

```terminal
dotnet add package Swashbuckle.AspNetCore
```

Next, register the OpenAPI (Swagger) generator in your application's startup path, defining one or more OpenAPI documents. For example:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-configure -->
<a id='snippet-README-configure'></a>
```cs
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();

app.Run();
```
<sup><a href='/test/WebSites/DocumentationSnippets/Program.cs#L1-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-configure' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- markdownlint-enable MD031 MD033 -->

Ensure your API endpoints and any parameters are decorated with `[Http*]` and `[From*]` attributes, where appropriate.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-endpoints -->
<a id='snippet-README-endpoints'></a>
```cs
[HttpPost]
public void CreateProduct([FromBody] Product product)
{
    // Implementation goes here
}

[HttpGet]
public IEnumerable<Product> SearchProducts([FromQuery] string keywords)
{
    // Implementation goes here
    return [];
}
```
<sup><a href='/test/WebSites/DocumentationSnippets/ProductsController.cs#L9-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-endpoints' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

> [!NOTE]
> If you omit the explicit parameter bindings, the generator will describe them as "query" parameters by default.

Then, expose the OpenAPI JSON document endpoint(s) using one of following methods:

- Add endpoints if you're using endpoint-based routing:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-MapSwagger -->
<a id='snippet-README-MapSwagger'></a>
```cs
// Your own endpoints go here, and then...
app.MapSwagger();
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L10-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-MapSwagger' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

- Adding the OpenAPI middleware:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-UseSwagger -->
<a id='snippet-README-UseSwagger'></a>
```cs
app.UseSwagger();
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L15-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-UseSwagger' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

At this point, you can launch your application and view the generated OpenAPI document at `/swagger/v1/swagger.json`.

Finally, you can optionally add the [swagger-ui][swagger-ui] middleware to expose interactive documentation, specifying the OpenAPI document(s) to power it from:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-UseSwaggerUI -->
<a id='snippet-README-UseSwaggerUI'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "My API V1");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L19-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-UseSwaggerUI' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

Now you can restart your application and view the auto-generated, interactive documentation at `/swagger`.

## System.Text.Json (STJ) vs Newtonsoft.Json (Json.NET)

In versions of Swashbuckle.AspNetCore prior to `5.0.0`, Swashbuckle.AspNetCore would generate Schemas (descriptions of the data types exposed by an API) based
on the behavior of the [Newtonsoft.Json serializer][newtonsoft-json]. This made sense because that was the serializer that shipped with ASP.NET Core
at the time. However, since ASP.NET Core 3.0, ASP.NET Core introduces a new serializer, [System.Text.Json (STJ)][system-text-json] out-of-the-box.

If you want to use Newtonsoft.Json instead, you must install a separate package and explicitly opt-in. By default Swashbuckle.AspNetCore will assume
that you're using the System.Text.Json serializer and generate schemas based on its behavior. If you're using Newtonsoft.Json, then you'll need to install a
separate Swashbuckle.AspNetCore package, [Swashbuckle.AspNetCore.Newtonsoft][swashbuckle-aspnetcore-newtonsoft] to explicitly opt-in.

Below is an example of how to do this for ASP.NET Core MVC:

```terminal
dotnet add package Swashbuckle.AspNetCore.Newtonsoft
```

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-Newtonsoft.Json -->
<a id='snippet-README-Newtonsoft.Json'></a>
```cs
services.AddMvc();

services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

services.AddSwaggerGenNewtonsoftSupport();
```
<sup><a href='/test/WebSites/DocumentationSnippets/IServiceCollectionExtensions.cs#L11-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-Newtonsoft.Json' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Swashbuckle, ApiExplorer, and Routing

Swashbuckle relies heavily on [`ApiExplorer`][api-explorer], the API metadata layer that ships with ASP.NET Core. If you're using the `AddMvc(...)`
helper methods to bootstrap the MVC stack, then API Explorer will be automatically registered and Swashbuckle.AspNetCore should work without issue.

However, if you're using `AddMvcCore(...)` for a more paired-down MVC stack, you'll need to explicitly add the API Explorer services:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-MvcCore -->
<a id='snippet-README-MvcCore'></a>
```cs
services.AddMvcCore()
        .AddApiExplorer();
```
<sup><a href='/test/WebSites/DocumentationSnippets/IServiceCollectionExtensions.cs#L22-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-MvcCore' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

Additionally, if you are using _[conventional routing][conventional-routing]_ (as opposed to attribute routing), any controllers and the actions on those
controllers that use conventional routing will not be represented in API Explorer, which means Swashbuckle won't be able to find those controllers and
generate OpenAPI operations for them.

For instance:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-MvcConventionalRouting -->
<a id='snippet-README-MvcConventionalRouting'></a>
```cs
app.UseMvc(routes =>
{
    // SwaggerGen won't find controllers that are routed via this technique.
    routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L26-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-MvcConventionalRouting' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

You **must** use attribute routing for any controllers that you want represented in your OpenAPI document(s):

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: README-AttributeRouting -->
<a id='snippet-README-AttributeRouting'></a>
```cs
[Route("example")]
public class ExampleController : Controller
{
    [HttpGet("")]
    public IActionResult DoStuff()
    {
        // Your implementation
        return Empty;
    }
}
```
<sup><a href='/test/WebSites/DocumentationSnippets/ExampleController.cs#L5-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-README-AttributeRouting' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

Refer to the [ASP.NET Core MVC routing documentation][mvc-routing] for more information.

## Components

Swashbuckle.AspNetCore consists of multiple components that can be used together or individually depending on your needs.

At its core, there's an OpenAPI generator, middleware to expose OpenAPI (Swagger) documentation as JSON endpoints, and a
packaged version of the [swagger-ui][swagger-ui]. These three packages can be installed with the [`Swashbuckle.AspNetCore`][package-download]
_"metapackage"_ and will work together (see [Getting Started](#getting-started)) to provide API documentation that is automatically generated from your code.

Additionally, there are add-on packages (CLI tools, [an alternate UI using Redoc][redoc] etc.) that you can install and configure as needed.

### "Core" Packages

| **Package** | **NuGet** | **Description** |
| ----------- | --------- | --------------- |
| [Swashbuckle.AspNetCore.Swagger][swashbuckle-aspnetcore-swagger] | [![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore.Swagger?logo=nuget&label=Latest&color=blue)][swashbuckle-aspnetcore-swagger] | Exposes OpenAPI JSON endpoints. It expects an implementation of `ISwaggerProvider` to be registered in the DI container, which it queries to retrieve `OpenApiDocument` instance(s) that are then exposed as serialized JSON. |
| [Swashbuckle.AspNetCore.SwaggerGen][swashbuckle-aspnetcore-swaggergen] | [![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore.SwaggerGen?logo=nuget&label=Latest&color=blue)][swashbuckle-aspnetcore-swaggergen] | Injects an implementation of `ISwaggerProvider` that can be used by the above component. This particular implementation generates `OpenApiDocument` instance(s) from your application endpoints (controllers, minimal endpoints etc.). |
| [Swashbuckle.AspNetCore.SwaggerUI][swashbuckle-aspnetcore-swaggerui] | [![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore.SwaggerUI?logo=nuget&label=Latest&color=blue)][swashbuckle-aspnetcore-swaggerui] | Exposes an embedded version of [swagger-ui][swagger-ui]. You specify the API endpoints where it can obtain OpenAPI documents from, and it uses them to power interactive documentation for your API. |

### Additional Packages

| **Package** | **NuGet** | **Description** |
| ----------- | --------- | --------------- |
| [Swashbuckle.AspNetCore.Annotations][swashbuckle-aspnetcore-annotations] | [![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore.Annotations?logo=nuget&label=Latest&color=blue)][swashbuckle-aspnetcore-annotations] | Includes a set of custom attributes that can be applied to controllers/endpoints, actions and models to enrich the generated documentation. |
| [Swashbuckle.AspNetCore.Cli][swashbuckle-aspnetcore-cli] | [![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore.Cli?logo=nuget&label=Latest&color=blue)][swashbuckle-aspnetcore-cli] | Provides a command line interface (CLI) for retrieving OpenAPI documents directly from an application start-up assembly and then writing to a file. |
| [Swashbuckle.AspNetCore.ReDoc][swashbuckle-aspnetcore-redoc] | [![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore.ReDoc?logo=nuget&label=Latest&color=blue)][swashbuckle-aspnetcore-redoc] | Exposes an embedded version of the [Redoc UI][redoc] (an alternative to [swagger-ui][swagger-ui]). |

### Community Packages

These packages are provided by the .NET open-source community.

| **Package** | **NuGet** | **Description** |
| ----------- | --------- | --------------- |
| [Swashbuckle.AspNetCore.Filters][swashbuckle-aspnetcore-filters] | [![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore.Filters?logo=nuget&label=Latest&color=blue)][swashbuckle-aspnetcore-filters] | Some useful Swashbuckle.AspNetCore filters which add additional documentation, e.g. request and response examples, authorization information, etc. See its README for more details. |
| [Unchase.Swashbuckle.AspNetCore.Extensions][unchase-swashbuckle-aspnetcore-extensions] | [![NuGet](https://img.shields.io/nuget/v/Unchase.Swashbuckle.AspNetCore.Extensions?logo=nuget&label=Latest&color=blue)][unchase-swashbuckle-aspnetcore-extensions] | Some useful extensions (filters), which add additional documentation, e.g. hide `PathItems` for unaccepted roles, fix enumerations for client code generation, etc. See its README for more details. |
| [MicroElements.Swashbuckle.FluentValidation][microelements-swashbuckle-fluentvalidation] | [![NuGet](https://img.shields.io/nuget/v/MicroElements.Swashbuckle.FluentValidation?logo=nuget&label=Latest&color=blue)][microelements-swashbuckle-fluentvalidation] | Use [FluentValidation][fluentvalidation] rules instead of ComponentModel attributes to augment generated OpenAPI schemas. |
| [MMLib.SwaggerForOcelot][mmlib-swaggerforocelot] | [![NuGet](https://img.shields.io/nuget/v/MMLib.SwaggerForOcelot?logo=nuget&label=Latest&color=blue)][mmlib-swaggerforocelot] | Aggregate documentations over microservices directly on [Ocelot API Gateway][ocelot]. |

## Configuration and Customization

The steps described above will get you up and running with minimal set up. However, Swashbuckle.AspNetCore offers a lot of flexibility to customize as you see fit.

Check out the table below for the full list of possible configuration options.

| **Component**                          | **Configuration and Customization**                                                                                                                                         |
| -------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Swashbuckle.AspNetCore.Swagger**     | [Change the Path for OpenAPI JSON Endpoints](docs/configure-and-customize-swagger.md#change-the-path-for-openapi-json-endpoints)                                            |
|                                        | [Modify OpenAPI with Request Context](docs/configure-and-customize-swagger.md#modify-openapi-with-request-context)                                                          |
|                                        | [Serialize OpenAPI JSON in the 3.1 format](docs/configure-and-customize-swagger.md#serialize-openapi-in-the-31-format)                                                      |
|                                        | [Serialize Swagger JSON in the 2.0 format](docs/configure-and-customize-swagger.md#serialize-swagger-in-the-20-format)                                                      |
|                                        | [Working with Virtual Directories and Reverse Proxies](docs/configure-and-customize-swagger.md#working-with-virtual-directories-and-reverse-proxies)                        |
|                                        | [Customizing how the OpenAPI document is serialized](docs/configure-and-customize-swagger.md#customizing-how-the-openapi-document-is-serialized)                            |
| **Swashbuckle.AspNetCore.SwaggerGen**  | [Assign Explicit OperationIds](docs/configure-and-customize-swaggergen.md#assign-explicit-operationids)                                                                     |
|                                        | [List Operations Responses](docs/configure-and-customize-swaggergen.md#list-operation-responses)                                                                            |
|                                        | [Flag Required Parameters and Schema Properties](docs/configure-and-customize-swaggergen.md#flag-required-parameters-and-schema-properties)                                 |
|                                        | [Handle Forms and File Uploads](docs/configure-and-customize-swaggergen.md#handle-forms-and-file-uploads)                                                                   |
|                                        | [Handle File Downloads](docs/configure-and-customize-swaggergen.md#handle-file-downloads)                                                                                   |
|                                        | [Include Descriptions from XML Comments](docs/configure-and-customize-swaggergen.md#include-descriptions-from-xml-comments)                                                 |
|                                        | [Provide Global API Metadata](docs/configure-and-customize-swaggergen.md#provide-global-api-metadata)                                                                       |
|                                        | [Generate Multiple OpenAPI Documents](docs/configure-and-customize-swaggergen.md#generate-multiple-openapi-documents)                                                       |
|                                        | [Omit Obsolete Operations and/or Schema Properties](docs/configure-and-customize-swaggergen.md#omit-obsolete-operations-andor-schema-properties)                            |
|                                        | [Omit Arbitrary Operations](docs/configure-and-customize-swaggergen.md#omit-arbitrary-operations)                                                                           |
|                                        | [Customize Operation Tags (e.g. for UI Grouping)](docs/configure-and-customize-swaggergen.md#customize-operation-tags-eg-for-ui-grouping)                                   |
|                                        | [Change Operation Sort Order (e.g. for UI Sorting)](docs/configure-and-customize-swaggergen.md#change-operation-sort-order-eg-for-ui-sorting)                               |
|                                        | [Customize Schema Ids](docs/configure-and-customize-swaggergen.md#customize-schema-ids)                                                                                     |
|                                        | [Override Schema for Specific Types](docs/configure-and-customize-swaggergen.md#override-schema-for-specific-types)                                                         |
|                                        | [Extend Generator with Operation, Schema & Document Filters](docs/configure-and-customize-swaggergen.md#extend-generator-with-operation-schema-and-document-filters)        |
|                                        | [Add Security Definitions and Requirements](docs/configure-and-customize-swaggergen.md#add-security-definitions-and-requirements)                                           |
|                                        | [Add Security Definitions and Requirements for Bearer auth](docs/configure-and-customize-swaggergen.md#add-security-definitions-and-requirements-for-bearer-authentication) |
|                                        | [Inheritance and Polymorphism](docs/configure-and-customize-swaggergen.md#inheritance-and-polymorphism)                                                                     |
| **Swashbuckle.AspNetCore.SwaggerUI**   | [Change Relative Path to the UI](docs/configure-and-customize-swaggerui.md#change-relative-path-to-the-ui)                                                                  |
|                                        | [Change Document Title](docs/configure-and-customize-swaggerui.md#change-document-title)                                                                                    |
|                                        | [Change CSS or JS Paths](docs/configure-and-customize-swaggerui.md#change-css-or-js-paths)                                                                                  |
|                                        | [List Multiple OpenAPI Documents](docs/configure-and-customize-swaggerui.md#list-multiple-openapi-documents)                                                                |
|                                        | [Apply swagger-ui Parameters](docs/configure-and-customize-swaggerui.md#apply-swagger-ui-parameters)                                                                        |
|                                        | [Inject Custom JavaScript](docs/configure-and-customize-swaggerui.md#inject-custom-javascript)                                                                              |
|                                        | [Inject Custom CSS](docs/configure-and-customize-swaggerui.md#inject-custom-css)                                                                                            |
|                                        | [Customize index.html](docs/configure-and-customize-swaggerui.md#customize-indexhtml)                                                                                       |
|                                        | [Enable OAuth2.0 Flows](docs/configure-and-customize-swaggerui.md#enable-oauth20-flows)                                                                                     |
|                                        | [Use client-side request and response interceptors](docs/configure-and-customize-swaggerui.md#use-client-side-request-and-response-interceptors)                            |
| **Swashbuckle.AspNetCore.Annotations** | [Install and Enable Annotations](docs/configure-and-customize-annotations.md#install-and-enable-annotations)                                                                |
|                                        | [Enrich Operation Metadata](docs/configure-and-customize-annotations.md#enrich-operation-metadata)                                                                          |
|                                        | [Enrich Response Metadata](docs/configure-and-customize-annotations.md#enrich-response-metadata)                                                                            |
|                                        | [Enrich Parameter Metadata](docs/configure-and-customize-annotations.md#enrich-parameter-metadata)                                                                          |
|                                        | [Enrich RequestBody Metadata](docs/configure-and-customize-annotations.md#enrich-requestbody-metadata)                                                                      |
|                                        | [Enrich Schema Metadata](docs/configure-and-customize-annotations.md#enrich-schema-metadata)                                                                                |
|                                        | [Apply Schema Filters to Specific Types](docs/configure-and-customize-annotations.md#apply-schema-filters-to-specific-types)                                                |
|                                        | [Add Tag Metadata](docs/configure-and-customize-annotations.md#add-tag-metadata)                                                                                            |
| **Swashbuckle.AspNetCore.Cli**         | [Retrieve OpenAPI Directly from a Startup Assembly](docs/configure-and-customize-cli.md#retrieve-openapi-directly-from-a-startup-assembly)                                  |
|                                        | [Use the CLI Tool with a Custom Host Configuration](docs/configure-and-customize-cli.md#use-the-cli-tool-with-a-custom-host-configuration)                                  |
| **Swashbuckle.AspNetCore.ReDoc**       | [Change Relative Path to the UI](docs/configure-and-customize-redoc.md#change-relative-path-to-the-ui)                                                                      |
|                                        | [Change Document Title](docs/configure-and-customize-redoc.md#change-document-title)                                                                                        |
|                                        | [Apply Redoc Parameters](docs/configure-and-customize-redoc.md#apply-redoc-parameters)                                                                                      |
|                                        | [Inject Custom CSS](docs/configure-and-customize-redoc.md#inject-custom-css)                                                                                                |
|                                        | [Customize index.html](docs/configure-and-customize-redoc.md#customize-indexhtml)                                                                                           |

<!-- markdownlint-disable MD013 -->

[api-explorer]: https://andrewlock.net/introduction-to-the-apiexplorer-in-asp-net-core/ "Introduction to the ApiExplorer in ASP.NET Core"
[build-badge]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/actions/workflows/build.yml/badge.svg?branch=master&event=push
[build-status]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/actions?query=workflow%3Abuild+branch%3Amaster+event%3Apush "Continuous Integration for this project"
[conventional-routing]: https://learn.microsoft.com/aspnet/core/mvc/controllers/routing#conventional-routing "Routing to controller actions in ASP.NET Core - Conventional routing"
[coverage-badge]: https://codecov.io/gh/domaindrivendev/Swashbuckle.AspNetCore/branch/master/graph/badge.svg
[coverage-report]: https://codecov.io/gh/domaindrivendev/Swashbuckle.AspNetCore "Code coverage report for this project"
[fluentvalidation]: https://docs.fluentvalidation.net "FluentValidation documentation"
[help-wanted-badge]: https://img.shields.io/github/issues/domaindrivendev/Swashbuckle.AspNetCore/help-wanted?style=flat&color=%24EC820&label=Help%20wanted
[help-wanted-issues]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/labels/help-wanted "Issues with help wanted for this project"
[microelements-swashbuckle-fluentvalidation]: https://github.com/micro-elements/MicroElements.Swashbuckle.FluentValidation "MicroElements.Swashbuckle.FluentValidation on GitHub"
[microsoft-openapi]: https://www.nuget.org/packages/Microsoft.OpenApi "The Microsoft.OpenApi NuGet package"
[mmlib-swaggerforocelot]: MMLib.SwaggerForOcelot "MMLib.SwaggerForOcelot on GitHub"
[mvc-routing]: https://learn.microsoft.com/aspnet/core/mvc/controllers/routing "Routing to controller actions in ASP.NET Core"
[newtonsoft-json]: https://www.nuget.org/packages/Newtonsoft.Json/ "Newtonsoft.Json NuGet package"
[ocelot]: https://github.com/ThreeMammals/Ocelot "Ocelot on GitHub"
[package-badge-downloads]: https://img.shields.io/nuget/dt/Swashbuckle.AspNetCore?logo=nuget&label=Downloads&color=blue
[package-badge-version]: https://img.shields.io/nuget/v/Swashbuckle.AspNetCore?logo=nuget&label=Latest&color=blue
[package-download]: https://www.nuget.org/packages/Swashbuckle.AspNetCore/ "Download Swashbuckle.AspNetCore from NuGet"
[redoc]: https://github.com/Redocly/redoc "The Redoc project in GitHub"
[scorecard-badge]: https://api.securityscorecards.dev/projects/github.com/domaindrivendev/Swashbuckle.AspNetCore/badge
[scorecard-report]: https://securityscorecards.dev/viewer/?uri=github.com/domaindrivendev/Swashbuckle.AspNetCore "OpenSSF Scorecard for this project"
[swashbuckle-aspnetcore-annotations]: https://www.nuget.org/packages/Swashbuckle.AspNetCore.Annotations "Swashbuckle.AspNetCore.Annotations NuGet package"
[swashbuckle-aspnetcore-cli]: https://www.nuget.org/packages/Swashbuckle.AspNetCore.Cli "Swashbuckle.AspNetCore.Cli NuGet package"
[swashbuckle-aspnetcore-filters]: https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters "Swashbuckle.AspNetCore.Filters in GitHub"
[swashbuckle-aspnetcore-newtonsoft]: https://www.nuget.org/packages/Swashbuckle.AspNetCore.Newtonsoft/ "Swashbuckle.AspNetCore.Newtonsoft NuGet package"
[swashbuckle-aspnetcore-redoc]: https://www.nuget.org/packages/Swashbuckle.AspNetCore.ReDoc "Swashbuckle.AspNetCore.ReDoc NuGet package"
[swashbuckle-aspnetcore-swagger]: https://www.nuget.org/packages/Swashbuckle.AspNetCore.Swagger "Swashbuckle.AspNetCore.Swagger NuGet package"
[swashbuckle-aspnetcore-swaggergen]: https://www.nuget.org/packages/Swashbuckle.AspNetCore.SwaggerGen "Swashbuckle.AspNetCore.SwaggerGen NuGet package"
[swashbuckle-aspnetcore-swaggerui]: https://www.nuget.org/packages/Swashbuckle.AspNetCore.SwaggerUI "Swashbuckle.AspNetCore.SwaggerUI NuGet package"
[swagger]: https://swagger.io "Swagger website"
[swagger-codegen]: https://github.com/swagger-api/swagger-codegen "The Swagger Codegen project in GitHub"
[swagger-specification]: https://swagger.io/specification/ "OpenAPI Specification"
[swagger-ui]: https://github.com/swagger-api/swagger-ui "The swagger-ui project in GitHub"
[system-text-json]: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview "JSON serialization and deserialization in .NET - overview"
[unchase-swashbuckle-aspnetcore-extensions]: https://github.com/unchase/Unchase.Swashbuckle.AspNetCore.Extensions "Unchase.Swashbuckle.AspNetCore.Extensions on GitHub"
[v10-migration]: docs/migrating-to-v10.md "Migrating to Swashbuckle.AspNetCore v10"
