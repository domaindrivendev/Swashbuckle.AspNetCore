| :mega: Important notice if you're upgrading between major versions! |
|--------------|
|* If you're upgrading from 4.x to 5.x, there's several breaking changes to be aware of. See the [release notes](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v5.0.0) for details<br />* If you're making the jump from 3.x to 4.x first, there be dragons there too. See [those release notes here](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v4.0.0)|

Swashbuckle.AspNetCore
=========

[![Build status](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/actions/workflows/build.yml/badge.svg?branch=master&event=push)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/actions?query=workflow%3Abuild+branch%3Amaster+event%3Apush) [![Code coverage](https://codecov.io/gh/domaindrivendev/Swashbuckle.AspNetCore/branch/master/graph/badge.svg)](https://codecov.io/gh/domaindrivendev/Swashbuckle.AspNetCore) [![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/domaindrivendev/Swashbuckle.AspNetCore/badge)](https://securityscorecards.dev/viewer/?uri=github.com/domaindrivendev/Swashbuckle.AspNetCore)

[![NuGet](https://img.shields.io/nuget/v/Swashbuckle.AspNetCore?logo=nuget&label=Latest&color=blue)](https://www.nuget.org/packages/Swashbuckle.AspNetCore/ "Download Swashbuckle.AspNetCore from NuGet.org")
[![NuGet](https://img.shields.io/nuget/dt/Swashbuckle.AspNetCore?logo=nuget&label=Downloads&color=blue)](https://www.nuget.org/packages/Swashbuckle.AspNetCore/ "Download Swashbuckle.AspNetCore from NuGet.org")

[![Help Wanted](https://img.shields.io/github/issues/domaindrivendev/Swashbuckle.AspNetCore/help-wanted?style=flat&color=%24EC820&label=Help%20wanted)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/labels/help-wanted)

[Swagger](https://swagger.io) tooling for APIs built with ASP.NET Core. Generate beautiful API documentation, including a UI to explore and test operations, directly from your routes, controllers and models.

In addition to its [Swagger 2.0 and OpenAPI 3.0](https://swagger.io/specification/) generator, Swashbuckle also provides an embedded version of the awesome [swagger-ui](https://github.com/swagger-api/swagger-ui) that's powered by the generated Swagger JSON. This means you can complement your API with living documentation that's always in sync with the latest code. Best of all, it requires minimal coding and maintenance, allowing you to focus on building an awesome API.

And that's not all ...

Once you have an API that can describe itself in Swagger, you've opened the treasure chest of Swagger-based tools including a client generator that can be targeted to a wide range of popular platforms. See [swagger-codegen](https://github.com/swagger-api/swagger-codegen) for more details.

# Compatibility #

| Swashbuckle Version | ASP.NET Core | Swagger / OpenAPI Spec. | swagger-ui | Redoc UI |
|----------|----------|----------|----------|----------|
| [CI](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/commits/master/) | >= 8.0.0, 2.3.x | 2.0, 3.0 | [5.x.x](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/src/Swashbuckle.AspNetCore.SwaggerUI/package.json#L6) | [2.x.x](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/src/Swashbuckle.AspNetCore.ReDoc/package.json#L6) |
| [8.1.1](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v8.1.1) | >= 8.0.0, 2.3.x | 2.0, 3.0 | 5.20.8 | 2.4.0 |
| [7.3.2](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v7.3.2) | >= 8.0.0, 6.0.x, 2.1.x | 2.0, 3.0 | 5.20.1 | 2.4.0 |
| [6.9.0](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v6.9.0) | >= 6.0.0, 2.1.x | 2.0, 3.0 | 5.17.14 | 2.1.5 |
| [5.6.3](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v5.6.3) | >= 2.0.0 | 2.0, 3.0 | 3.32.5 | 2.0.0-rc.40 |
| [4.0.0](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v4.0.0) | >= 2.0.0, < 3.0.0 | 2.0 | 3.19.5 | 1.22.2 |
| [3.0.0](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v3.0.0) | >= 1.0.4, < 3.0.0 | 2.0 | 3.17.1 | 1.20.0 |
| [2.5.0](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v2.5.0) | >= 1.0.4, < 3.0.0 | 2.0 | 3.16.0 | 1.20.0 |

# Getting Started #

1. Install the standard Nuget package into your ASP.NET Core application.

    ```
    Package Manager : Install-Package Swashbuckle.AspNetCore
    CLI : dotnet add package Swashbuckle.AspNetCore
    ```

2. In the `ConfigureServices` method of `Startup.cs`, register the Swagger generator, defining one or more Swagger documents.

    ```csharp
    using Microsoft.OpenApi.Models;
    ```
    
    ```csharp
    services.AddMvc();

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    });
    ```

3. Ensure your API actions and parameters are decorated with explicit "Http" and "From" bindings.

    ```csharp
    [HttpPost]
    public void CreateProduct([FromBody]Product product)
    ...
    ```

    ```csharp
    [HttpGet]
    public IEnumerable<Product> SearchProducts([FromQuery]string keywords)
    ...
    ```

    _NOTE: If you omit the explicit parameter bindings, the generator will describe them as "query" params by default._

4. In the `Configure` method,you should expose the generated Swagger as JSON endpoint(s) by one of following method:

    - Add endpoints if you're using endpoint-based routing.

    ```csharp
    app.MapEndpoints(endpoints =>
    {
        // ...
        endpoints.MapSwagger();
    });
    ```

    - Insert middleware

    ```csharp
    app.UseSwagger();
    ```

    _At this point, you can spin up your application and view the generated Swagger JSON at "/swagger/v1/swagger.json."_

5. Optionally, insert the swagger-ui middleware if you want to expose interactive documentation, specifying the Swagger JSON endpoint(s) to power it from.

    ```csharp
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "My API V1");
    });
    ```

    _Now you can restart your application and check out the auto-generated, interactive docs at "/swagger"._

# System.Text.Json (STJ) vs Newtonsoft #

In versions prior to `5.0.0`, Swashbuckle will generate Schema's (descriptions of the data types exposed by an API) based on the behavior of the *Newtonsoft* serializer. This made sense because that was the serializer that shipped with ASP.NET Core at the time. However, since version `3.0.0`, ASP.NET Core introduces a new serializer *System.Text.Json (STJ)* out-of-the-box, and if you want to continue using *Newtonsoft*, you need to install a separate package and explicitly opt-in. From Swashbuckle `5.0.0` and beyond a similar pattern is used. That is, out-of-the-box Swashbuckle will assume you're using the *STJ* serializer and generate Schema's based on its behavior. If you're using *Newtonsoft*, then you'll need to install a separate Swashbuckle package and explicitly opt-in. **This is a required step, regardless of which version of ASP.NET Core you're using**.

In summary ...

If you're using **System.Text.Json (STJ)**, then the setup described above will be sufficient, and *STJ* options/attributes will be automatically honored by the Swagger generator.

If you're using **Newtonsoft**, then you'll need to install a separate package and explicitly opt-in to ensure that *Newtonsoft* settings/attributes are automatically honored by the Swagger generator:

```
Package Manager : Install-Package Swashbuckle.AspNetCore.Newtonsoft
CLI : dotnet add package Swashbuckle.AspNetCore.Newtonsoft
```

```csharp
services.AddMvc();

services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
services.AddSwaggerGenNewtonsoftSupport(); // explicit opt-in - needs to be placed after AddSwaggerGen()
```

# Swashbuckle, ApiExplorer, and Routing #

Swashbuckle relies heavily on `ApiExplorer`, the API metadata layer that ships with ASP.NET Core. If you're using the `AddMvc` helper to bootstrap the MVC stack, then ApiExplorer will be automatically registered and SB will work without issue. However, if you're using `AddMvcCore` for a more paired-down MVC stack, you'll need to explicitly add the ApiExplorer service:

```csharp
services.AddMvcCore()
    .AddApiExplorer();
```

Additionally, if you are using _[conventional routing](https://learn.microsoft.com/aspnet/core/mvc/controllers/routing#conventional-routing)_ (as opposed to attribute routing), any controllers and the actions on those controllers that use conventional routing will not be represented in ApiExplorer, which means Swashbuckle won't be able to find those controllers and generate Swagger operations from them. For instance:

```csharp
app.UseMvc(routes =>
{
   // SwaggerGen won't find controllers that are routed via this technique.
   routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
});
```

You **must** use attribute routing for any controllers that you want represented in your Swagger document(s):

```csharp
[Route("example")]
public class ExampleController : Controller
{
    [HttpGet("")]
    public IActionResult DoStuff() { /**/ }
}
```
Refer to the [routing documentation](https://learn.microsoft.com/aspnet/core/mvc/controllers/routing) for more information.

# Components #

Swashbuckle consists of multiple components that can be used together or individually depending on your needs. At its core, there's a Swagger generator, middleware to expose it as JSON endpoints, and a packaged version of the [swagger-ui](https://github.com/swagger-api/swagger-ui). These 3 packages can be installed with the `Swashbuckle.AspNetCore` "metapackage" and will work together seamlessly (see [Getting Started](#getting-started)) to provide beautiful API docs that are automatically generated from your code.

Additionally, there's add-on packages (CLI tools, [an alternate UI](https://github.com/Rebilly/redoc) etc.) that you can optionally install and configure as needed.

## "Core" Packages (i.e. installed via Swashbuckle.AspNetCore)

|Package|Description|
|---------|-----------|
|Swashbuckle.AspNetCore.Swagger|Exposes Swagger JSON endpoints. It expects an implementation of `ISwaggerProvider` to be registered in the DI container, which it queries to retrieve `OpenAPIDocument(s)` that are then exposed as serialized JSON|
|Swashbuckle.AspNetCore.SwaggerGen|Injects an implementation of `ISwaggerProvider` that can be used by the above component. This particular implementation generates `OpenApiDocument(s)` from your routes, controllers and models|
|Swashbuckle.AspNetCore.SwaggerUI|Exposes an embedded version of the swagger-ui. You specify the API endpoints where it can obtain Swagger JSON, and it uses them to power interactive docs for your API|

## Additional Packages ##

|Package|Description|
|---------|-----------|
|Swashbuckle.AspNetCore.Annotations|Includes a set of custom attributes that can be applied to controllers, actions and models to enrich the generated Swagger|
|Swashbuckle.AspNetCore.Cli|Provides a command line interface for retrieving Swagger directly from a startup assembly, and writing to file|
|Swashbuckle.AspNetCore.ReDoc|Exposes an embedded version of the Redoc UI (an alternative to swagger-ui)|

## Community Packages ##

These packages are provided by the open-source community.

|Package|Description|
|---------|-----------|
|[Swashbuckle.AspNetCore.Filters](https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters)| Some useful Swashbuckle filters which add additional documentation, e.g. request and response examples, authorization information, etc. See its Readme for more details |
|[Unchase.Swashbuckle.AspNetCore.Extensions](https://github.com/unchase/Unchase.Swashbuckle.AspNetCore.Extensions)| Some useful extensions (filters), which add additional documentation, e.g. hide PathItems for unaccepted roles, fix enums for client code generation, etc. See its Readme for more details |
|[MicroElements.Swashbuckle.FluentValidation](https://github.com/micro-elements/MicroElements.Swashbuckle.FluentValidation)| Use FluentValidation rules instead of ComponentModel attributes to augment generated Swagger Schemas |
|[MMLib.SwaggerForOcelot](https://github.com/Burgyn/MMLib.SwaggerForOcelot)| Aggregate documentations over microservices directly on Ocelot API Gateway |

# Configuration & Customization #

The steps described above will get you up and running with minimal setup. However, Swashbuckle offers a lot of flexibility to customize as you see fit. 

Check out the table below for the full list of options:

| Component                              | Configuration and Customization                                                                                                                                   |
| -------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Swashbuckle.AspNetCore.Swagger**     | [Change the Path for Swagger JSON Endpoints](docs/configure-and-customize-swagger.md#change-the-path-for-swagger-json-endpoints)                                  |
|                                        | [Modify Swagger with Request Context](docs/configure-and-customize-swagger.md#modify-swagger-with-request-context)                                                |
|                                        | [Serialize Swagger JSON in the 2.0 format](docs/configure-and-customize-swagger.md#serialize-swagger-in-the-20-format)                                            |
|                                        | [Working with Virtual Directories and Reverse Proxies](docs/configure-and-customize-swagger.md#working-with-virtual-directories-and-reverse-proxies)              |
|                                        | [Customizing how the OpenAPI document is serialized](docs/configure-and-customize-swagger.md#customizing-how-the-openapi-document-is-serialized)                  |
| **Swashbuckle.AspNetCore.SwaggerGen**  | [Assign Explicit OperationIds](docs/configure-and-customize-swaggergen.md#assign-explicit-operationids)                                                           |
|                                        | [List Operations Responses](docs/configure-and-customize-swaggergen.md#list-operation-responses)                                                                  |
|                                        | [Flag Required Parameters and Schema Properties](docs/configure-and-customize-swaggergen.md#flag-required-parameters-and-schema-properties)                       |
|                                        | [Handle Forms and File Uploads](docs/configure-and-customize-swaggergen.md#handle-forms-and-file-uploads)                                                         |
|                                        | [Handle File Downloads](docs/configure-and-customize-swaggergen.md#handle-file-downloads)                                                                         |
|                                        | [Include Descriptions from XML Comments](docs/configure-and-customize-swaggergen.md#include-descriptions-from-xml-comments)                                       |
|                                        | [Provide Global API Metadata](docs/configure-and-customize-swaggergen.md#provide-global-api-metadata)                                                             |
|                                        | [Generate Multiple Swagger Documents](docs/configure-and-customize-swaggergen.md#generate-multiple-swagger-documents)                                             |
|                                        | [Omit Obsolete Operations and/or Schema Properties](docs/configure-and-customize-swaggergen.md#omit-obsolete-operations-andor-schema-properties)                  |
|                                        | [Omit Arbitrary Operations](docs/configure-and-customize-swaggergen.md#omit-arbitrary-operations)                                                                 |
|                                        | [Customize Operation Tags (e.g. for UI Grouping)](docs/configure-and-customize-swaggergen.md#customize-operation-tags-eg-for-ui-grouping)                         |
|                                        | [Change Operation Sort Order (e.g. for UI Sorting)](docs/configure-and-customize-swaggergen.md#change-operation-sort-order-eg-for-ui-sorting)                     |
|                                        | [Customize Schema Id's](docs/configure-and-customize-swaggergen.md#customize-schema-ids)                                                                          |
|                                        | [Override Schema for Specific Types](docs/configure-and-customize-swaggergen.md#override-schema-for-specific-types)                                               |
|                                        | [Extend Generator with Operation, Schema & Document Filters](docs/configure-and-customize-swaggergen.md#extend-generator-with-operation-schema--document-filters) |
|                                        | [Add Security Definitions and Requirements](docs/configure-and-customize-swaggergen.md#add-security-definitions-and-requirements)                                 |
|                                        | [Add Security Definitions and Requirements for Bearer auth](docs/configure-and-customize-swaggergen.md#add-security-definitions-and-requirements-for-bearer-auth) |
|                                        | [Inheritance and Polymorphism](docs/configure-and-customize-swaggergen.md#inheritance-and-polymorphism)                                                           |
| **Swashbuckle.AspNetCore.SwaggerUI**   | [Change Relative Path to the UI](docs/configure-and-customize-swaggerui.md#change-relative-path-to-the-ui)                                                        |
|                                        | [Change Document Title](docs/configure-and-customize-swaggerui.md#change-document-title)                                                                          |
|                                        | [Change CSS or JS Paths](docs/configure-and-customize-swaggerui.md#change-css-or-js-paths)                                                                        |
|                                        | [List Multiple Swagger Documents](docs/configure-and-customize-swaggerui.md#list-multiple-swagger-documents)                                                      |
|                                        | [Apply swagger-ui Parameters](docs/configure-and-customize-swaggerui.md#apply-swagger-ui-parameters)                                                              |
|                                        | [Inject Custom JavaScript](docs/configure-and-customize-swaggerui.md#inject-custom-javascript)                                                                    |
|                                        | [Inject Custom CSS](docs/configure-and-customize-swaggerui.md#inject-custom-css)                                                                                  |
|                                        | [Customize index.html](docs/configure-and-customize-swaggerui.md#customize-indexhtml)                                                                             |
|                                        | [Enable OAuth2.0 Flows](docs/configure-and-customize-swaggerui.md#enable-oauth20-flows)                                                                           |
|                                        | [Use client-side request and response interceptors](docs/configure-and-customize-swaggerui.md#use-client-side-request-and-response-interceptors)                  |
| **Swashbuckle.AspNetCore.Annotations** | [Install and Enable Annotations](docs/configure-and-customize-annotations.md#install-and-enable-annotations)                                                      |
|                                        | [Enrich Operation Metadata](docs/configure-and-customize-annotations.md#enrich-operation-metadata)                                                                |
|                                        | [Enrich Response Metadata](docs/configure-and-customize-annotations.md#enrich-response-metadata)                                                                  |
|                                        | [Enrich Parameter Metadata](docs/configure-and-customize-annotations.md#enrich-parameter-metadata)                                                                |
|                                        | [Enrich RequestBody Metadata](docs/configure-and-customize-annotations.md#enrich-requestbody-metadata)                                                            |
|                                        | [Enrich Schema Metadata](docs/configure-and-customize-annotations.md#enrich-schema-metadata)                                                                      |
|                                        | [Apply Schema Filters to Specific Types](docs/configure-and-customize-annotations.md#apply-schema-filters-to-specific-types)                                      |
|                                        | [Add Tag Metadata](docs/configure-and-customize-annotations.md#add-tag-metadata)                                                                                  |
| **Swashbuckle.AspNetCore.Cli**         | [Retrieve Swagger Directly from a Startup Assembly](docs/configure-and-customize-cli.md#retrieve-swagger-directly-from-a-startup-assembly)                        |
|                                        | [Use the CLI Tool with a Custom Host Configuration](docs/configure-and-customize-cli.md#use-the-cli-tool-with-a-custom-host-configuration)                        |
| **Swashbuckle.AspNetCore.ReDoc**       | [Change Relative Path to the UI](docs/configure-and-customize-redoc.md#change-relative-path-to-the-ui)                                                            |
|                                        | [Change Document Title](docs/configure-and-customize-redoc.md#change-document-title)                                                                              |
|                                        | [Apply Redoc Parameters](docs/configure-and-customize-redoc.md#apply-redoc-parameters)                                                                            |
|                                        | [Inject Custom CSS](docs/configure-and-customize-redoc.md#inject-custom-css)                                                                                      |
|                                        | [Customize index.html](docs/configure-and-customize-redoc.md#customize-indexhtml)                                                                                 |
