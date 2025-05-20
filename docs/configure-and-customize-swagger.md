# Configuration & Customization of `Swashbuckle.AspNetCore.Swagger`

## Change the Path for Swagger JSON Endpoints

By default, Swagger JSON will be exposed at the following route - `"/swagger/{documentName}/swagger.json"`. If necessary, you can change this when enabling the Swagger middleware. 

> [!IMPORTANT]
> Custom routes **must** include the `{documentName}` parameter.

```csharp
app.UseSwagger(options =>
{
    options.RouteTemplate = "api-docs/{documentName}/swagger.json";
});
```

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
> If you also need to update the relative path that the UI itself is available on, you'll need to follow the instructions found in [Change Relative Path to the UI](configure-and-customize-swaggerui.md#change-relative-path-to-the-ui).

## Modify Swagger with Request Context

If you need to set some Swagger metadata based on the current request, you can configure a filter that's executed prior to serializing the document.

```csharp
app.UseSwagger(options =>
{
    options.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
    });
});
```

The `OpenApiDocument` and the current `HttpRequest` are both passed to the filter. This provides a lot of flexibility. For example, you can add an explicit API server based on the "Host" header (as shown), or you could inspect session information or an Authorization header and remove operations from the document based on user permissions.

## Serialize Swagger in the 2.0 format

By default, Swashbuckle will generate and expose Swagger JSON in version 3.0 of the specification, officially called the OpenAPI Specification. However, to support backwards compatibility, you can opt to continue exposing it in the 2.0 format with the following option:

```csharp
app.UseSwagger(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
});
```

## Working with Virtual Directories and Reverse Proxies

Virtual directories and reverse proxies can cause issues for applications that generate links and redirects, particularly if the app returns *absolute* URLs based on the `Host` header and other information from the current request. To avoid these issues, Swashbuckle uses *relative* URLs where possible, and encourages their use when configuring the SwaggerUI and Redoc middleware.

For example, to wire up the SwaggerUI middleware, you provide the URL to one or more OpenAPI/Swagger documents. This is the URL that the swagger-ui, a client-side application, will call to retrieve your API metadata. To ensure this works behind virtual directories and reverse proxies, you should express this relative to the `RoutePrefix` of the swagger-ui itself:

```csharp
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("v1/swagger.json", "My API V1");
});
```

> [!NOTE] 
> In previous versions of the docs, you may have seen this expressed as a root-relative link (e.g. `/swagger/v1/swagger.json`). This won't work if your app is hosted on an IIS virtual directory or behind a proxy that trims the request path before forwarding. If you switch to the *page-relative* syntax shown above, it should work in all cases.

## Customizing how the OpenAPI document is serialized

By default, Swashbuckle will serialize the OpenAPI document using the Serialize methods on the OpenAPI document object. If a customized serialization is desired, 
it is possible to create a custom document serializer that implements the `ISwaggerDocumentSerializer` interface. This can be set on the `SwaggerOptions` in the service collection using `ConfigureSwagger()`:

> [!NOTE]
> If you plan on using the command line tool to generate OpenAPI specification files, this must be done on the service collection using `ConfigureSwagger()`.

```csharp
services.ConfigureSwagger(options =>
{
    option.SetCustomDocumentSerializer<CustomDocumentSerializer>();
});
```

When the command line tool is not used, it can also be done on the application host:

```csharp
app.UseSwagger(options =>
{
    options.SetCustomDocumentSerializer<CustomDocumentSerializer>();
});
```
