# Configuration and Customization of `Swashbuckle.AspNetCore.Swagger`

## Change the Path for OpenAPI JSON Endpoints

By default, OpenAPI (Swagger) JSON will be exposed at the following route - `/swagger/{documentName}/swagger.json`.
If necessary, you can change this when enabling the Swagger middleware.

> [!IMPORTANT]
> Custom routes **must** include the `{documentName}` parameter.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-RouteTemplate -->
<a id='snippet-Swagger-RouteTemplate'></a>
```cs
app.UseSwagger(options =>
{
    options.RouteTemplate = "api-docs/{documentName}/swagger.json";
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L201-L206' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-RouteTemplate' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

> [!NOTE]
> If you're using the SwaggerUI middleware, you'll also need to update its configuration to reflect the new endpoints:
>
> ```csharp
> app.UseSwaggerUI(options =>
> {
>     options.SwaggerEndpoint("/api-docs/v1/swagger.json", "My API V1");
> });
> ```
>
> If you also need to update the relative path that the UI itself is available on, you'll need to follow the instructions
> found in [Change Relative Path to the UI](configure-and-customize-swaggerui.md#change-relative-path-to-the-ui).

## Modify OpenAPI with Request Context

If you need to set some OpenAPI metadata based on the current request, you can configure a filter that's executed prior to serializing the document.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-ModifyWithHttpRequest -->
<a id='snippet-Swagger-ModifyWithHttpRequest'></a>
```cs
app.UseSwagger(options =>
{
    options.PreSerializeFilters.Add((document, request) =>
    {
        document.Servers = [new OpenApiServer { Url = $"{request.Scheme}://{request.Host.Value}" }];
    });
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L208-L216' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-ModifyWithHttpRequest' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

The `OpenApiDocument` and the current `HttpRequest` are both passed to the filter, which provides a lot of flexibility.
For example, you can add an explicit API server based on the `Host` header (as shown), or you could inspect session
information or an `Authorization` header and remove operations from the document based on user permissions.

## Serialize OpenAPI in the 3.1 format

By default, Swashbuckle.AspNetCore will generate and expose OpenAPI JSON in version 3.0 of the specification.
However, if you wish to use the latest version of the OpenAPI specification, you can opt into version 3.1
format with the following option:

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

## Serialize Swagger in the 2.0 format

By default, Swashbuckle will generate and expose OpenAPI JSON in version 3.0 of the specification, officially called the
OpenAPI Specification. However, to support backwards compatibility, you can opt to continue exposing it in the Swagger 2.0
format with the following option:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-Swagger2.0 -->
<a id='snippet-Swagger-Swagger2.0'></a>
```cs
app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L225-L230' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-Swagger2.0' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Working with Virtual Directories and Reverse Proxies

Virtual directories and reverse proxies can cause issues for applications that generate links and redirects, particularly
if the app returns *absolute* URLs based on the `Host` header and other information from the current request. To avoid these
issues, Swashbuckle.AspNetCore uses *relative* URLs where possible, and encourages their use when configuring the SwaggerUI
and ReDoc middleware.

For example, to wire up the SwaggerUI middleware, you provide the URL to one or more OpenAPI documents. This is the URL
that [swagger-ui](https://github.com/swagger-api/swagger-ui), a client-side application, will call to retrieve your API metadata. To ensure this works behind
virtual directories and reverse proxies, you should express this relative to the `RoutePrefix` of swagger-ui itself:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-ReverseProxy -->
<a id='snippet-Swagger-ReverseProxy'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("v1/swagger.json", "My API V1");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L232-L238' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-ReverseProxy' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Customizing how the OpenAPI document is serialized

By default, Swashbuckle.AspNetCore will serialize the OpenAPI document using the `Serialize*` methods on the OpenAPI document object. If a
customized serialization is desired,  it is possible to create a custom document serializer that implements the `ISwaggerDocumentSerializer` interface.
This can be set on the `SwaggerOptions` in the service collection using `ConfigureSwagger()`:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-CustomSerializerServices -->
<a id='snippet-Swagger-CustomSerializerServices'></a>
```cs
services.ConfigureSwagger(options =>
{
    options.SetCustomDocumentSerializer<CustomDocumentSerializer>();
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/IServiceCollectionExtensions.cs#L42-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-CustomSerializerServices' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

> [!NOTE]
> If you plan on using the command line tool to generate OpenAPI specification files, this must be done on the service
> collection using `ConfigureSwagger()`.

When the command line tool is not used, it can also be done on the application host:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Swagger-CustomSerializerMiddleware -->
<a id='snippet-Swagger-CustomSerializerMiddleware'></a>
```cs
app.UseSwagger(options =>
{
    options.SetCustomDocumentSerializer<CustomDocumentSerializer>();
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L240-L245' title='Snippet source file'>snippet source</a> | <a href='#snippet-Swagger-CustomSerializerMiddleware' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->
