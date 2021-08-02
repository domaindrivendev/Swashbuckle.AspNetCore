| :mega: Important notice if you're upgrading between major versions! |
|--------------|
|* If you're upgrading from 4.x to 5.x, there's several breaking changes to be aware of. See the [release notes](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v5.0.0) for details<br />* If you're making the jump from 3.x to 4.x first, there be dragons there too. See [those release notes here](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/releases/tag/v4.0.0)|

Swashbuckle.AspNetCore
=========

[![Build status](https://ci.appveyor.com/api/projects/status/xpsk2cj1xn12c0r7/branch/master?svg=true)](https://ci.appveyor.com/project/domaindrivendev/ahoy/branch/master)

[![Nuget](https://img.shields.io/nuget/v/swashbuckle.aspnetcore)](https://www.nuget.org/packages/swashbuckle.aspnetcore/)

[Swagger](http://swagger.io) tooling for APIs built with ASP.NET Core. Generate beautiful API documentation, including a UI to explore and test operations, directly from your routes, controllers and models.

In addition to its [Swagger 2.0 and OpenAPI 3.0](http://swagger.io/specification/) generator, Swashbuckle also provides an embedded version of the awesome [swagger-ui](https://github.com/swagger-api/swagger-ui) that's powered by the generated Swagger JSON. This means you can complement your API with living documentation that's always in sync with the latest code. Best of all, it requires minimal coding and maintenance, allowing you to focus on building an awesome API.

And that's not all ...

Once you have an API that can describe itself in Swagger, you've opened the treasure chest of Swagger-based tools including a client generator that can be targeted to a wide range of popular platforms. See [swagger-codegen](https://github.com/swagger-api/swagger-codegen) for more details.

# Compatibility #

|Swashbuckle Version|ASP.NET Core|Swagger / OpenAPI Spec.|swagger-ui|ReDoc UI|
|----------|----------|----------|----------|----------|
|[master](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/master/README.md)|>= 2.0.0|2.0, 3.0|3.47.1|2.0.0-rc.40|
|[6.1.5](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v6.1.5)|>= 2.0.0|2.0, 3.0|3.47.1|2.0.0-rc.40|
|[5.6.3](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v5.6.3)|>= 2.0.0|2.0, 3.0|3.32.5|2.0.0-rc.40|
|[4.0.0](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v4.0.0)|>= 2.0.0, < 3.0.0|2.0|3.19.5|1.22.2|
|[3.0.0](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v3.0.0)|>= 1.0.4, < 3.0.0|2.0|3.17.1|1.20.0|
|[2.5.0](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/tree/v2.5.0)|>= 1.0.4, < 3.0.0|2.0|3.16.0|1.20.0|

# Getting Started #

1. Install the standard Nuget package into your ASP.NET Core application.

    ```
    Package Manager : Install-Package Swashbuckle.AspNetCore -Version 6.1.5
    CLI : dotnet add package --version 6.1.5 Swashbuckle.AspNetCore
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

4. In the `Configure` method, insert middleware to expose the generated Swagger as JSON endpoint(s)

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
Package Manager : Install-Package Swashbuckle.AspNetCore.Newtonsoft -Version 6.1.5
CLI : dotnet add package --version 6.1.5 Swashbuckle.AspNetCore.Newtonsoft
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

Additionally, if you are using _[conventional routing](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing#conventional-routing)_ (as opposed to attribute routing), any controllers and the actions on those controllers that use conventional routing will not be represented in ApiExplorer, which means Swashbuckle won't be able to find those controllers and generate Swagger operations from them. For instance:

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
Refer to the [routing documentation](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing) for more information.

# Components #

Swashbuckle consists of multiple components that can be used together or individually dependening on your needs. At its core, there's a Swagger generator, middleware to expose it as JSON endpoints, and a packaged version of the [swagger-ui](https://github.com/swagger-api/swagger-ui). These 3 packages can be installed with the `Swashbuckle.AspNetCore` "metapackage" and will work together seamlessly (see [Getting Started](#getting-started)) to provide beautiful API docs that are automatically generated from your code.

Additionally, there's add-on packages (CLI tools, [an alternate UI](https://github.com/Rebilly/ReDoc) etc.) that you can optionally install and configure as needed.

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
|Swashbuckle.AspNetCore.ReDoc|Exposes an embedded version of the ReDoc UI (an alternative to swagger-ui)|

## Community Packages ##

These packages are provided by the open-source community.

|Package|Description|
|---------|-----------|
|[Swashbuckle.AspNetCore.Filters](https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters)| Some useful Swashbuckle filters which add additional documentation, e.g. request and response examples, authorization information, etc. See its Readme for more details |
|[Unchase.Swashbuckle.AspNetCore.Extensions](https://github.com/unchase/Unchase.Swashbuckle.AspNetCore.Extensions)| Some useful extensions (filters), which add additional documentation, e.g. hide PathItems for unaccepted roles, fix enums for client code generation, etc. See its Readme for more details |
|[MicroElements.Swashbuckle.FluentValidation](https://github.com/micro-elements/MicroElements.Swashbuckle.FluentValidation)| Use FluentValidation rules instead of ComponentModel attributes to augment generated Swagger Schemas |
|[MMLib.SwaggerForOcelot](https://github.com/Burgyn/MMLib.SwaggerForOcelot)| Aggregate documentations over microservices directly on Ocelot API Gateway |

# Configuration & Customization #

The steps described above will get you up and running with minimal setup. However, Swashbuckle offers a lot of flexibility to customize as you see fit. Check out the table below for the full list of options:

* [Swashbuckle.AspNetCore.Swagger](#swashbuckleaspnetcoreswagger)

    * [Change the Path for Swagger JSON Endpoints](#change-the-path-for-swagger-json-endpoints)
    * [Modify Swagger with Request Context](#modify-swagger-with-request-context)
    * [Serialize Swagger JSON in the 2.0 format](#serialize-swagger-in-the-20-format)
    * [Working with Virtual Directories and Reverse Proxies](#working-with-virtual-directories-and-reverse-proxies)

* [Swashbuckle.AspNetCore.SwaggerGen](#swashbuckleaspnetcoreswaggergen)

    * [Assign Explicit OperationIds](#assign-explicit-operationids)
    * [List Operations Responses](#list-operation-responses)
    * [Flag Required Parameters and Schema Properties](#flag-required-parameters-and-schema-properties)
    * [Handle Forms and File Uploads](#handle-forms-and-file-uploads)
    * [Include Descriptions from XML Comments](#include-descriptions-from-xml-comments)
    * [Provide Global API Metadata](#provide-global-api-metadata)
    * [Generate Multiple Swagger Documents](#generate-multiple-swagger-documents)
    * [Omit Obsolete Operations and/or Schema Properties](#omit-obsolete-operations-andor-schema-properties)
    * [Omit Arbitrary Operations](#omit-arbitrary-operations)
    * [Customize Operation Tags (e.g. for UI Grouping)](#customize-operation-tags-eg-for-ui-grouping)
    * [Change Operation Sort Order (e.g. for UI Sorting)](#change-operation-sort-order-eg-for-ui-sorting)
    * [Customize Schema Id's](#customize-schema-ids)
    * [Override Schema for Specific Types](#override-schema-for-specific-types)
    * [Extend Generator with Operation, Schema & Document Filters](#extend-generator-with-operation-schema--document-filters)
    * [Add Security Definitions and Requirements](#add-security-definitions-and-requirements)
    * [Inheritance and Polymorphism](#inheritance-and-polymorphism)

* [Swashbuckle.AspNetCore.SwaggerUI](#swashbuckleaspnetcoreswaggerui)
    * [Change Releative Path to the UI](#change-relative-path-to-the-ui)
    * [Change Document Title](#change-document-title)
    * [List Multiple Swagger Documents](#list-multiple-swagger-documents)
    * [Apply swagger-ui Parameters](#apply-swagger-ui-parameters)
    * [Inject Custom CSS](#inject-custom-css)
    * [Customize index.html](#customize-indexhtml)
    * [Enable OAuth2.0 Flows](#enable-oauth20-flows)
    * [Use client-side request and response interceptors](#use-client-side-request-and-response-interceptors)

* [Swashbuckle.AspNetCore.Annotations](#swashbuckleaspnetcoreannotations)
    * [Install and Enable Annotations](#install-and-enable-annotations)
    * [Enrich Operation Metadata](#enrich-operation-metadata)
    * [Enrich Response Metadata](#enrich-response-metadata)
    * [Enrich Parameter Metadata](#enrich-parameter-metadata)
    * [Enrich RequestBody Metadata](#enrich-requestbody-metadata)
    * [Enrich Schema Metadata](#enrich-schema-metadata)
    * [Apply Schema Filters to Specific Types](#apply-schema-filters-to-specific-types)
    * [Add Tag Metadata](#add-tag-metadata)

* [Swashbuckle.AspNetCore.Cli](#swashbuckleaspnetcorecli)
    * [Retrieve Swagger Directly from a Startup Assembly](#retrieve-swagger-directly-from-a-startup-assembly)
    * [Use the CLI Tool with a Custom Host Configuration](#use-the-cli-tool-with-a-custom-host-configuration)

* [Swashbuckle.AspNetCore.ReDoc](#swashbuckleaspnetcoreredoc)
    * [Change Releative Path to the UI](#redoc-change-relative-path-to-the-ui)
    * [Change Document Title](#redoc-change-document-title)
    * [Apply ReDoc Parameters](#apply-redoc-parameters)
    * [Inject Custom CSS](#redoc-inject-custom-css)
    * [Customize index.html](#redoc-customize-indexhtml)

## Swashbuckle.AspNetCore.Swagger ##

### Change the Path for Swagger JSON Endpoints ###

By default, Swagger JSON will be exposed at the following route - "/swagger/{documentName}/swagger.json". If necessary, you can change this when enabling the Swagger middleware. Custom routes MUST include the `{documentName}` parameter.

```csharp
app.UseSwagger(c =>
{
    c.RouteTemplate = "api-docs/{documentName}/swagger.json";
})
```

_NOTE: If you're using the SwaggerUI middleware, you'll also need to update its configuration to reflect the new endpoints:_

```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api-docs/v1/swagger.json", "My API V1");
})
```

_NOTE: If you also need to update the relative path that the UI itself is available on, you'll need to follow the instructions found in [Change Relative Path to the UI](#change-relative-path-to-the-ui)._

### Modify Swagger with Request Context ###

If you need to set some Swagger metadata based on the current request, you can configure a filter that's executed prior to serializing the document.

```csharp
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
    });
});
```

The `OpenApiDocument` and the current `HttpRequest` are both passed to the filter. This provides a lot of flexibility. For example, you can add an explicit API server based on the "Host" header (as shown), or you could inspect session information or an Authorization header and remove operations from the document based on user permissions.

### Serialize Swagger in the 2.0 format ###

By default, Swashbuckle will generate and expose Swagger JSON in version 3.0 of the specification, officially called the OpenAPI Specification. However, to support backwards compatibility, you can opt to continue exposing it in the 2.0 format with the following option:

```csharp
app.UseSwagger(c =>
{
    c.SerializeAsV2 = true;
});
```

### Working with Virtual Directories and Reverse Proxies ###

Virtual directories and reverse proxies can cause issues for applications that generate links and redirects, particularly if the app returns *absolute* URLs based on the `Host` header and other information from the current request. To avoid these issues, Swasbuckle uses *relative* URLs where possible, and encourages their use when configuring the SwaggerUI and ReDoc middleware.

For example, to wire up the SwaggerUI middleware, you provide the URL to one or more OpenAPI/Swagger documents. This is the URL that the swagger-ui, a client-side application, will call to retrieve your API metadata. To ensure this works behind virtual directories and reverse proxies, you should express this relative to the `RoutePrefix` of the swagger-ui itself:

```csharp
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
});
```

_NOTE: In previous versions of the docs, you may have seen this expressed as a root-relative link (e.g. `/swagger/v1/swagger.json`). This won't work if your app is hosted on an IIS virtual directory or behind a proxy that trims the request path before forwarding. If you switch to the *page-relative* syntax shown above, it should work in all cases._

## Swashbuckle.AspNetCore.SwaggerGen ##

### Assign Explicit OperationIds ###

In Swagger, operations MAY be assigned an `operationId`. This ID MUST be unique among all operations described in the API. Tools and libraries (e.g. client generators) MAY use the operationId to uniquely identify an operation, therefore, it is RECOMMENDED to follow common programming naming conventions.

Auto-generating an ID that matches these requirements, while also providing a name that would be meaningful in client libraries is a non-trivial task and so, Swashbuckle omits the `operationId` by default. However, if neccessary, you can assign `operationIds` by decorating individual routes OR by providing a custom strategy.

__Option 1) Decorate routes with a `Name` property__

```csharp
[HttpGet("{id}", Name = "GetProductById")]
public IActionResult Get(int id) // operationId = "GetProductById"
```

__Option 2) Provide a custom strategy__

```csharp
// Startup.cs
services.AddSwaggerGen(c =>
{
    ...
    
    // Use method name as operationId
    c.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });
})

// ProductsController.cs
[HttpGet("{id}")]
public IActionResult GetProductById(int id) // operationId = "GetProductById"
```

_NOTE: With either approach, API authors are responsible for ensuring the uniqueness of `operationIds` across all Operations_

### List Operation Responses ###

By default, Swashbuckle will generate a "200" response for each operation. If the action returns a response DTO, then this will be used to generate a schema for the response body. For example ...

```csharp
[HttpPost("{id}")]
public Product GetById(int id)
```

Will produce the following response metadata:

```
responses: {
  200: {
    description: "Success",
    content: {
      "application/json": {
        schema: {
          $ref: "#/components/schemas/Product"
        }
      }
    }
  }
}
```

#### Explicit Responses ####

If you need to specify a different status code and/or additional responses, or your actions return `IActionResult` instead of a response DTO, you can explicitly describe responses with the `ProducesResponseTypeAttribute` that ships with ASP.NET Core. For example ...

```csharp
[HttpPost("{id}")]
[ProducesResponseType(typeof(Product), 200)]
[ProducesResponseType(typeof(IDictionary<string, string>), 400)]
[ProducesResponseType(500)]
public IActionResult GetById(int id)
```

Will produce the following response metadata:

```
responses: {
  200: {
    description: "Success",
    content: {
      "application/json": {
        schema: {
          $ref: "#/components/schemas/Product"
        }
      }
    }
  },
  400: {
    description: "Bad Request",
    content: {
      "application/json": {
        schema: {
          type: "object",
          additionalProperties: {
            type: "string"
          }
        }
      }
    }
  },
  500: {
    description: "Server Error",
    content: {}
  }
}
```

### Flag Required Parameters and Schema Properties ###

In a Swagger document, you can flag parameters and schema properties that are required for a request. If a parameter (top-level or property-based) is decorated with the `BindRequiredAttribute` or `RequiredAttribute`, then Swashbuckle will automatically flag it as a "required" parameter in the generated Swagger:

```csharp
// ProductsController.cs
public IActionResult Search([FromQuery, BindRequired]string keywords, [FromQuery]PagingParams pagingParams)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    ...
}

// SearchParams.cs
public class PagingParams
{
    [Required]
    public int PageNo { get; set; }

    public int PageSize { get; set; }
}
```

In addition to parameters, Swashbuckle will also honor the `RequiredAttribute` when used in a model that's bound to the request body. In this case, the decorated properties will be flagged as "required" properties in the body description:

```csharp
// ProductsController.cs
public IActionResult Create([FromBody]Product product)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    ...
}

// Product.cs
public class Product
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }
}
```

### Handle Forms and File Uploads ###

This controller will accept two form field values and one named file upload from the same form:

```csharp
[HttpPost]
public void UploadFile([FromForm]string description, [FromForm]DateTime clientDate, [IFormFile] file)
```

> Important note: As per the [ASP.NET Core docs](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1), you're not supposed to decorate `IFormFile` parameters with the `[FromForm]` attribute as the binding source is automatically inferred from the type. In fact, the inferred value is `BindingSource.FormFile` and if you apply the attribute it will be set to `BindingSource.Form` instead, which screws up `ApiExplorer`, the metadata component that ships with ASP.NET Core and is heavily relied on by Swashbuckle. One particular issue here is that SwaggerUI will not treat the parameter as a file and so will not display a file upload button, if you do mistakenly include this attribute.

### Include Descriptions from XML Comments ###

To enhance the generated docs with human-friendly descriptions, you can annotate controller actions and models with [Xml Comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc) and configure Swashbuckle to incorporate those comments into the outputted Swagger JSON:

1. Open the Properties dialog for your project, click the "Build" tab and ensure that "XML documentation file" is checked, or add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` element to the `<PropertyGroup>` section of your .csproj project file. This will produce a file containing all XML comments at build-time.

    _At this point, any classes or methods that are NOT annotated with XML comments will trigger a build warning. To suppress this, enter the warning code "1591" into the "Suppress warnings" field in the properties dialog._

2. Configure Swashbuckle to incorporate the XML comments on file into the generated Swagger JSON:

    ```csharp
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1",
            new OpenApiInfo
            {
                Title = "My API - V1",
                Version = "v1"
            }
         );

         var filePath = Path.Combine(System.AppContext.BaseDirectory, "MyApi.xml");
         c.IncludeXmlComments(filePath);
    }
    ```

3. Annotate your actions with summary, remarks, param and response tags:

    ```csharp
    /// <summary>
    /// Retrieves a specific product by unique id
    /// </summary>
    /// <remarks>Awesomeness!</remarks>
    /// <param name="id" example="123">The product id</param>
    /// <response code="200">Product created</response>
    /// <response code="400">Product has missing/invalid values</response>
    /// <response code="500">Oops! Can't create your product right now</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
    [ProducesResponseType(500)]
    public Product GetById(int id)
    ```

4. You can also annotate types with summary and example tags:

    ```csharp
    public class Product
    {
        /// <summary>
        /// The name of the product
        /// </summary>
        /// <example>Men's basketball shoes</example>
        public string Name { get; set; }

        /// <summary>
        /// Quantity left in stock
        /// </summary>
        /// <example>10</example>
        public int AvailableStock { get; set; }
    }
    ```

5. Rebuild your project to update the XML Comments file and navigate to the Swagger JSON endpoint. Note how the descriptions are mapped onto corresponding Swagger fields.

_NOTE: You can also provide Swagger Schema descriptions by annotating your API models and their properties with summary tags. If you have multiple XML comments files (e.g. separate libraries for controllers and models), you can invoke the IncludeXmlComments method multiple times and they will all be merged into the outputted Swagger JSON._

### Provide Global API Metadata ###

In addition to "PathItems", "Operations" and "Responses", which Swashbuckle generates for you, Swagger also supports global metadata (see https://swagger.io/specification/#oasObject). For example, you can provide a full description for your API, terms of service or even contact and licensing information:

```csharp
c.SwaggerDoc("v1",
    new OpenApiInfo
    {
        Title = "My API - V1",
        Version = "v1",
        Description = "A sample API to demo Swashbuckle",
        TermsOfService = new Uri("http://tempuri.org/terms"),
        Contact = new OpenApiContact
        {
            Name = "Joe Developer",
            Email = "joe.developer@tempuri.org"
        },
        License = new OpenApiLicense
        {
            Name = "Apache 2.0",
            Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0.html")
        }
    }
);
```

_TIP: Use IntelliSense to see what other fields are available._

### Generate Multiple Swagger Documents ###

With the setup described above, the generator will include all API operations in a single Swagger document. However, you can create multiple documents if necessary. For example, you may want a separate document for each version of your API. To do this, start by defining multiple Swagger docs in `Startup.cs`:

```csharp
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API - V1", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API - V2", Version = "v2" });
})
```

_Take note of the first argument to SwaggerDoc. It MUST be a URI-friendly name that uniquely identifies the document. It's subsequently used to make up the path for requesting the corresponding Swagger JSON. For example, with the default routing, the above documents will be available at "/swagger/v1/swagger.json" and "/swagger/v2/swagger.json"._

Next, you'll need to inform Swashbuckle which actions to include in each document. Although this can be customized (see below), by default, the generator will use the `ApiDescription.GroupName` property, part of the built-in metadata layer that ships with ASP.NET Core, to make this distinction. You can set this by decorating individual actions OR by applying an application wide convention.

#### Decorate Individual Actions ####

To include an action in a specific Swagger document, decorate it with the `ApiExplorerSettingsAttribute` and set `GroupName` to the corresponding document name (case sensitive):

```csharp
[HttpPost]
[ApiExplorerSettings(GroupName = "v2")]
public void Post([FromBody]Product product)
```

#### Assign Actions to Documents by Convention ####

To group by convention instead of decorating every action, you can apply a custom controller or action convention. For example, you could wire up the following convention to assign actions to documents based on the controller namespace.

```csharp
// ApiExplorerGroupPerVersionConvention.cs
public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace; // e.g. "Controllers.V1"
        var apiVersion = controllerNamespace.Split('.').Last().ToLower();

        controller.ApiExplorer.GroupName = apiVersion;
    }
}

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(c =>
        c.Conventions.Add(new ApiExplorerGroupPerVersionConvention())
    );

    ...
}
```

#### Customize the Action Selection Process ####

When selecting actions for a given Swagger document, the generator invokes a `DocInclusionPredicate` against every `ApiDescription` that's surfaced by the framework. The default implementation inspects `ApiDescription.GroupName` and returns true if the value is either null OR equal to the requested document name. However, you can also provide a custom inclusion predicate. For example, if you're using an attribute-based approach to implement API versioning (e.g. Microsoft.AspNetCore.Mvc.Versioning), you could configure a custom predicate that leverages the versioning attributes instead:

```csharp
c.DocInclusionPredicate((docName, apiDesc) =>
{
    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

    var versions = methodInfo.DeclaringType
        .GetCustomAttributes(true)
        .OfType<ApiVersionAttribute>()
        .SelectMany(attr => attr.Versions);

    return versions.Any(v => $"v{v.ToString()}" == docName);
});
```

#### Exposing Multiple Documents through the UI ####

If you're using the `SwaggerUI` middleware, you'll need to specify any additional Swagger endpoints you want to expose. See [List Multiple Swagger Documents](#list-multiple-swagger-documents) for more.

### Omit Obsolete Operations and/or Schema Properties ###

The [Swagger spec](http://swagger.io/specification/) includes a `deprecated` flag for indicating that an operation is deprecated and should be refrained from use. The Swagger generator will automatically set this flag if the corresponding action is decorated with the `ObsoleteAttribute`. However, instead of setting a flag, you can configure the generator to ignore obsolete actions altogether:

```csharp
services.AddSwaggerGen(c =>
{
    ...
    c.IgnoreObsoleteActions();
};
```

A similar approach can also be used to omit obsolete properties from Schemas in the Swagger output. That is, you can decorate model properties with the `ObsoleteAttribute` and configure Swashbuckle to omit those properties when generating JSON Schemas:

```csharp
services.AddSwaggerGen(c =>
{
    ...
    c.IgnoreObsoleteProperties();
};
```

### Omit Arbitrary Operations ###

You can omit operations from the Swagger output by decorating individual actions OR by applying an application wide convention.

#### Decorate Individual Actions ####

To omit a specific action, decorate it with the `ApiExplorerSettingsAttribute` and set the `IgnoreApi` flag:

```csharp
[HttpGet("{id}")]
[ApiExplorerSettings(IgnoreApi = true)]
public Product GetById(int id)
```

#### Omit Actions by Convention ####

To omit actions by convention instead of decorating them individually, you can apply a custom action convention. For example, you could wire up the following convention to only document GET operations:

```csharp
// ApiExplorerGetsOnlyConvention.cs
public class ApiExplorerGetsOnlyConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        action.ApiExplorer.IsVisible = action.Attributes.OfType<HttpGetAttribute>().Any();
    }
}

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(c =>
        c.Conventions.Add(new ApiExplorerGetsOnlyConvention())
    );

    ...
}
```

### Customize Operation Tags (e.g. for UI Grouping) ###

The [Swagger spec](http://swagger.io/specification/) allows one or more "tags" to be assigned to an operation. The Swagger generator will assign the controller name as the default tag. This is important to note if you're using the `SwaggerUI` middleware as it uses this value to group operations.

You can override the default tag by providing a function that applies tags by convention. For example, the following configuration will tag, and therefore group operations in the UI, by HTTP method:

```csharp
services.AddSwaggerGen(c =>
{
    ...
    c.TagActionsBy(api => api.HttpMethod);
};
```

### Change Operation Sort Order (e.g. for UI Sorting) ###

By default, actions are ordered by assigned tag (see above) before they're grouped into the path-centric, nested structure of the [Swagger spec](http://swagger.io/specification). But, you can change the default ordering of actions with a custom sorting strategy:

```csharp
services.AddSwaggerGen(c =>
{
    ...
    c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
};
```

_NOTE: This dictates the sort order BEFORE actions are grouped and transformed into the Swagger format. So, it affects the ordering of groups (i.e. Swagger "PathItems"), AND the ordering of operations within a group, in the Swagger output._

### Customize Schema Id's ###

If the generator encounters complex parameter or response types, it will generate a corresponding JSON Schema, add it to the global `components/schemas` dictionary, and reference it from the operation description by unique Id. For example, if you have an action that returns a `Product` type, then the generated schema will be referenced as follows:

```
responses: {
  200: {
    description: "Success",
    content: {
      "application/json": {
        schema: {
          $ref: "#/components/schemas/Product"
        }
      }
    }
  }
}
```

However, if it encounters multiple types with the same name but different namespaces (e.g. `RequestModels.Product` & `ResponseModels.Product`), then Swashbuckle will raise an exception due to "Conflicting schemaIds". In this case, you'll need to provide a custom Id strategy that further qualifies the name:

```csharp
services.AddSwaggerGen(c =>
{
    ...
    c.CustomSchemaIds((type) => type.FullName);
};
```

### Override Schema for Specific Types ###

Out-of-the-box, Swashbuckle does a decent job at generating JSON Schemas that accurately describe your request and response payloads. However, if you're customizing serialization behavior for certain types in your API, you may need to help it out.

For example, you might have a class with multiple properties that you want to represent in JSON as a comma-separated string. To do this you would probably implement a custom `JsonConverter`. In this case, Swashbuckle doesn't know how the converter is implemented and so you would need to provide it with a Schema that accurately describes the type:

```csharp
// PhoneNumber.cs
public class PhoneNumber
{
    public string CountryCode { get; set; }

    public string AreaCode { get; set; }

    public string SubscriberId { get; set; }
}

// Startup.cs
services.AddSwaggerGen(c =>
{
    ...
    c.MapType<PhoneNumber>(() => new OpenApiSchema { Type = "string" });
};
```

### Extend Generator with Operation, Schema & Document Filters ###

Swashbuckle exposes a filter pipeline that hooks into the generation process. Once generated, individual metadata objects are passed into the pipeline where they can be modified further. You can wire up custom filters to enrich the generated "Operations", "Schemas" and "Documents".

#### Operation Filters ####

Swashbuckle retrieves an `ApiDescription`, part of ASP.NET Core, for every action and uses it to generate a corresponding `OpenApiOperation`. Once generated, it passes the `OpenApiOperation` and the `ApiDescription` through the list of configured Operation Filters.

In a typical filter implementation, you would inspect the `ApiDescription` for relevant information (e.g. route info, action attributes etc.) and then update the `OpenApiOperation` accordingly. For example, the following filter lists an additional "401" response for all actions that are decorated with the `AuthorizeAttribute`:

```csharp
// AuthResponsesOperationFilter.cs
public class AuthResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

        if (authAttributes.Any())
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
    }
}

// Startup.cs
services.AddSwaggerGen(c =>
{
    ...
    c.OperationFilter<AuthResponsesOperationFilter>();
};
```

_NOTE: Filter pipelines are DI-aware. That is, you can create filters with constructor parameters and if the parameter types are registered with the DI framework, they'll be automatically injected when the filters are instantiated_

#### Schema Filters ####

Swashbuckle generates a Swagger-flavored [JSONSchema](http://swagger.io/specification/#schemaObject) for every parameter, response and property type that's exposed by your controller actions. Once generated, it passes the schema and type through the list of configured Schema Filters.

The example below adds an AutoRest vendor extension (see https://github.com/Azure/autorest/blob/master/docs/extensions/readme.md#x-ms-enum) to inform the AutoRest tool how enums should be modelled when it generates the API client.

```csharp
// AutoRestSchemaFilter.cs
public class AutoRestSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type.IsEnum)
        {
            schema.Extensions.Add(
                "x-ms-enum",
                new OpenApiObject
                {
                    ["name"] = new OpenApiString(type.Name),
                    ["modelAsString"] = new OpenApiBoolean(true)
                }
            );
        };
    }
}

// Startup.cs
services.AddSwaggerGen(c =>
{
    ...
    c.SchemaFilter<AutoRestSchemaFilter>();
};
```

The example below allows for automatic schema generation of generic `Dictionary<Enum, TValue>` objects.
Note that this only generates the swagger; `System.Text.Json` is not able to parse dictionary enums by default,
so you will need [a special JsonConverter, like in the .NET docs](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0#sample-factory-pattern-converter)

```csharp
// DictionaryTKeyEnumTValueSchemaFilter.cs
public class DictionaryTKeyEnumTValueSchemaFilter : ISchemaFilter
{
  public void Apply(OpenApiSchema schema, SchemaFilterContext context)
  {
    // Only run for fields that are a Dictionary<Enum, TValue>
    if (!context.Type.IsGenericType || !context.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
    {
return;
    }

    var keyType = context.Type.GetGenericArguments()[0];
    var valueType = context.Type.GetGenericArguments()[1];

    if (!keyType.IsEnum)
    {
return;
    }

    schema.Type = "object";
    schema.Properties = keyType.GetEnumNames().ToDictionary(name => name,
name => context.SchemaGenerator.GenerateSchema(valueType,
  context.SchemaRepository));
  }
}

// Startup.cs
services.AddSwaggerGen(c =>
{
    ...
    // These will be replaced by DictionaryTKeyEnumTValueSchemaFilter, but are needed to avoid an error.
    // You will need one for every kind of Dictionary<,> you have.
    c.MapType<Dictionary<MyEnum, List<string>>>(() => new OpenApiSchema());
    c.SchemaFilter<DictionaryTKeyEnumTValueSchemaFilter>();
};
	
```
#### Document Filters ####

Once an `OpenApiDocument` has been generated, it too can be passed through a set of pre-configured Document Filters. This gives full control to modify the document however you see fit. To ensure you're still returning valid Swagger JSON, you should have a read through the [specification](http://swagger.io/specification/) before using this filter type.

The example below provides a description for any tags that are assigned to operations in the document:

```csharp
public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new List<OpenApiTag> {
            new OpenApiTag { Name = "Products", Description = "Browse/manage the product catalog" },
            new OpenApiTag { Name = "Orders", Description = "Submit orders" }
        };
    }
}
```

_NOTE: If you're using the `SwaggerUI` middleware, the `TagDescriptionsDocumentFilter` demonstrated above could be used to display additional descriptions beside each group of Operations._

### Add Security Definitions and Requirements ###

In Swagger, you can describe how your API is secured by defining one or more security schemes (e.g basic, api key, oauth2 etc.) and declaring which of those schemes are applicable globally OR for specific operations. For more details, take a look at the [Security Requirement Object in the Swagger spec.](https://swagger.io/specification/#securityRequirementObject).

In Swashbuckle, you can define schemes by invoking the `AddSecurityDefinition` method, providing a name and an instance of `OpenApiSecurityScheme`. For example you can define an [OAuth 2.0 - implicit flow](https://oauth.net/2/) as follows:

```csharp
// Startup.cs
services.AddSwaggerGen(c =>
{
    ...

    // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    { "readAccess", "Access read operations" },
                    { "writeAccess", "Access write operations" }
                }
            }
        }
    });
};
```

_NOTE: In addition to defining a scheme, you also need to indicate which operations that scheme is applicable to. You can apply schemes globally (i.e. to ALL operations) through the `AddSecurityRequirement` method. The example below indicates that the scheme called "oauth2" should be applied to all operations, and that the "readAccess" and "writeAccess" scopes are required. When applying schemes of type other than "oauth2", the array of scopes MUST be empty._

```csharp
c.AddSwaggerGen(c =>
{
    ...

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "readAccess", "writeAccess" }
        }
    });
})
```

If you have schemes that are only applicable for certain operations, you can apply them through an Operation filter. For example, the following filter adds OAuth2 requirements based on the presence of the `AuthorizeAttribute`:

```csharp
// SecurityRequirementsOperationFilter.cs
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Policy names map to scopes
        var requiredScopes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Select(attr => attr.Policy)
            .Distinct();

        if (requiredScopes.Any())
        {
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            var oAuthScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            };

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [ oAuthScheme ] = requiredScopes.ToList()
                }
            };
        }
    }
}
```

_NOTE: If you're using the `SwaggerUI` middleware, you can enable interactive OAuth2.0 flows that are powered by the emitted security metadata. See [Enabling OAuth2.0 Flows](#enable-oauth20-flows) for more details._

### Inheritance and Polymorphism ###

Swagger / OpenAPI defines the `allOf` and `oneOf` keywords for describing [inheritance and polymorphism](https://swagger.io/docs/specification/data-models/inheritance-and-polymorphism/) relationships in schema definitions. For example, if you're using a base class for models that share common properties you can use the `allOf` keyword to describe the inheritance hierarchy. Or, if your serializer supports polymorphic serializion/deserialization, you can use the `oneOf` keyword to document all the "possible" schemas for requests/responses that vary by subtype.

#### Enabling Inheritance ####

By default, Swashbuckle flattens inheritance hierarchies. That is, for derived models, the inherited properties are combined and listed alongside the declared properties. This can cause a lot of duplication in the generated Swagger, particularly when there's multiple subtypes. It's also problematic if you're using a client generator (e.g. NSwag) and would like to maintain the inheritiance hierarchy in the generated client models. To work around this, you can apply the `UseAllOfForInheritance` setting, and this will leverage the `allOf` keyword to incorporate inherited properties by reference in the generated Swagger:

```
Circle: {
  type: "object",
  allOf: [
    {
      $ref: "#/components/schemas/Shape"
    }
  ],
  properties: {
    radius: {
      type: "integer",
      format: "int32",
    }
  },
},
Shape: {
  type: "object",
  properties: {
    name: {
      type: "string",
      nullable: true,
    }
  },
}
```

#### Enabling Polymorphism ####

If your serializer supports polymorphic serialization/deserialization and you would like to list the possible subtypes for an action that accepts/returns abstract base types, you can apply the `UseOneOfForPolymorphism` setting. As a result, the generated request/response schemas will reference a collection of "possible" schemas instead of just the base class schema:

```
requestBody: {
  content: {
    application/json: {
      schema: {
      oneOf: [
        {
          $ref: "#/components/schemas/Rectangle"
        },
        {
          $ref: "#/components/schemas/Circle"
        },
      ],
    }
  }
}
```

#### Detecting Subtypes ####

As inheritance and polymorphism relationships can often become quite complex, not just in your own models but also within the .NET class library, Swashbuckle is selective about which hierarchies it does and doesn't expose in the generated Swagger. By default, it will pick up any subtypes that are defined in the same assembly as a given base type. If you'd like to override this behavior, you can provide a custom selector function:

```csharp
services.AddSwaggerGen(c =>
{
    ...

    c.UseAllOfForInheritance();

    c.SelectSubTypesUsing(baseType =>
    {
        return typeof(Startup).Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
    })
});
```

_NOTE: If you're using the [Swashbuckle Annotations library](#swashbuckleaspnetcoreannotations), it contains a custom selector that's based on the presence of `SwaggerSubType` attributes on base class definitions. This way, you can use simple attributes to explicitly list the inheritance and/or polymorphism relationships you want to expose. To enable this behavior, check out the [Annotations docs](#list-known-subtypes-for-inheritance-and-polymorphism)._

#### Describing Discriminators ####

In conjunction with the `oneOf` and/or `allOf` keywords, Swagger / OpenAPI supports a `discriminator` field on base schema definitions. This keyword points to the property that identifies the specific type being represented by a given payload. In addition to the property name, the discriminator description MAY also include a `mapping` which maps discriminator values to specific schema definitions.

For example, the Newtonsoft serializer supports polymorphic serialization/deserialization by emitting/accepting a "$type" property on JSON instances. The value of this property will be the [assembly qualified type name](https://docs.microsoft.com/en-us/dotnet/api/system.type.assemblyqualifiedname?view=netcore-3.1) of the type represented by a given JSON instance. So, to explicitly describe this behavior in Swagger, the corresponding request/respose schema could be defined as follows:

```
components: {
  schemas: {
    Shape: {
      required: [
        "$type"
      ],
      type: "object",
      properties: {
        $type: {
          type": "string"
      },
      discriminator: {
        propertyName: "$type",
        mapping: {
          Rectangle: "#/components/schemas/Rectangle",
          Circle: "#/components/schemas/Circle"
        }
      }
    },
    Rectangle: {
      type: "object",
      allOf: [
        {
          "$ref": "#/components/schemas/Shape"
        }
      ],
      ...
    },
    Circle: {
      type: "object",
      allOf: [
        {
          "$ref": "#/components/schemas/Shape"
        }
      ],
      ...
    }
  }
}
```

If `UseAllOfForInheritance` or `UseOneOfForPolymorphism` is enabled, and your serializer supports (and has enabled) emitting/accepting a discriminator property, then Swashbuckle will automatically generate the corresponding `discriminator` metadata on base schema definitions.

Alternatively, if you've customized your serializer to support polymorphic serialization/deserialization, you can provide some custom selector functions to determine the discriminator name and corresponding mapping:

```csharp
services.AddSwaggerGen(c =>
{
    ...

    c.UseOneOfForInheritance();

    c.SelectDiscriminatorNameUsing((baseType) => "TypeName");
    c.SelectDiscriminatorValueUsing((subType) => subType.Name);
});
```

_NOTE: If you're using the [Swashbuckle Annotations library](#swashbuckleaspnetcoreannotations), it contains custom selector functions that are based on the presence of `SwaggerDiscriminator` and `SwaggerSubType` attributes on base class definitions. This way, you can use simple attributes to explicitly provide discriminator metadata. To enable this behavior, check out the [Annotations docs](#enrich-polymorphic-base-classes-with-discriminator-metadata)._

## Swashbuckle.AspNetCore.SwaggerUI ##

### Change Relative Path to the UI ###

By default, the Swagger UI will be exposed at "/swagger". If necessary, you can alter this when enabling the SwaggerUI middleware:

```csharp
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "api-docs"
    ...
}
```

### Change Document Title ###

By default, the Swagger UI will have a generic document title. When you have multiple Swagger pages open, it can be difficult to tell them apart. You can alter this when enabling the SwaggerUI middleware:

```csharp
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "My Swagger UI";
    ...
}
```

### List Multiple Swagger Documents ###

When enabling the middleware, you're required to specify one or more Swagger endpoints (fully qualified or relative to the UI page) to power the UI. If you provide multiple endpoints, they'll be listed in the top right corner of the page, allowing users to toggle between the different documents. For example, the following configuration could be used to document different versions of an API.

```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
}
```

### Apply swagger-ui Parameters ###

The swagger-ui ships with its own set of configuration parameters, all described here https://github.com/swagger-api/swagger-ui/blob/v3.8.1/docs/usage/configuration.md#display. In Swashbuckle, most of these are surfaced through the SwaggerUI middleware options:

```csharp
app.UseSwaggerUI(c =>
{
    c.DefaultModelExpandDepth(2);
    c.DefaultModelRendering(ModelRendering.Model);
    c.DefaultModelsExpandDepth(-1);
    c.DisplayOperationId();
    c.DisplayRequestDuration();
    c.DocExpansion(DocExpansion.None);
    c.EnableDeepLinking();
    c.EnableFilter();
    c.MaxDisplayedTags(5);
    c.ShowExtensions();
    c.ShowCommonExtensions();
    c.EnableValidator();
    c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Head);
    c.UseRequestInterceptor("(request) => { return request; }");
    c.UseResponseInterceptor("(response) => { return response; }");
});
```

_NOTE: The `InjectOnCompleteJavaScript` and `InjectOnFailureJavaScript` options have been removed because the latest version of swagger-ui doesn't expose the neccessary hooks. Instead, it provides a [flexible customization system](https://github.com/swagger-api/swagger-ui/blob/master/docs/customization/overview.md) based on concepts and patterns from React and Redux. To leverage this, you'll need to provide a custom version of index.html as described [below](#customize-indexhtml)._

The [custom index sample app](test/WebSites/CustomUIIndex/Swagger/index.html) demonstrates this approach, using the swagger-ui plugin system provide a custom topbar, and to hide the info component.

### Inject Custom CSS ###

To tweak the look and feel, you can inject additional CSS stylesheets by adding them to your `wwwroot` folder and specifying the relative paths in the middleware options:

```csharp
app.UseSwaggerUI(c =>
{
    ...
    c.InjectStylesheet("/swagger-ui/custom.css");
}
```

### Customize index.html ###

To customize the UI beyond the basic options listed above, you can provide your own version of the swagger-ui index.html page:

```csharp
app.UseSwaggerUI(c =>
{
    c.IndexStream = () => GetType().Assembly
        .GetManifestResourceStream("CustomUIIndex.Swagger.index.html"); // requires file to be added as an embedded resource
});
```

_To get started, you should base your custom index.html on the [default version](src/Swashbuckle.AspNetCore.SwaggerUI/index.html)_

### Enable OAuth2.0 Flows ###

The swagger-ui has built-in support to participate in OAuth2.0 authorization flows. It interacts with authorization and/or token endpoints, as specified in the Swagger JSON, to obtain access tokens for subsequent API calls. See [Adding Security Definitions and Requirements](#add-security-definitions-and-requirements) for an example of adding OAuth2.0 metadata to the generated Swagger.

If you're Swagger endpoint includes the appropriate security metadata, the UI interaction should be automatically enabled. However, you can further customize OAuth support in the UI with the following settings below. See https://github.com/swagger-api/swagger-ui/blob/v3.10.0/docs/usage/oauth2.md for more info:

```csharp
app.UseSwaggerUI(c =>
{
    ...

    c.OAuthClientId("test-id");
    c.OAuthClientSecret("test-secret");
    c.OAuthRealm("test-realm");
    c.OAuthAppName("test-app");
    c.OAuthScopeSeparator(" ");
    c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "foo", "bar" }}); 
    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
});
```

### Use client-side request and response interceptors ###

To use custom interceptors on requests and responses going through swagger-ui you can define them as javascript functions in the configuration:

```csharp
app.UseSwaggerUI(c =>
{
    ...

    c.UseRequestInterceptor("(req) => { req.headers['x-my-custom-header'] = 'MyCustomValue'; return req; }");
    c.UseResponseInterceptor("(res) => { console.log('Custom interceptor intercepted response from:', res.url); return res; }");
});
```

This can be useful in a range of scenarios where you might want to append local xsrf tokens to all requests for example:

```csharp
app.UseSwaggerUI(c =>
{
    ...

    c.UseRequestInterceptor("(req) => { req.headers['X-XSRF-Token'] = localStorage.getItem('xsrf-token'); return req; }");
});
```

## Swashbuckle.AspNetCore.Annotations ##

### Install and Enable Annotations ###

1. Install the following Nuget package into your ASP.NET Core application.

    ```
    Package Manager : Install-Package Swashbuckle.AspNetCore.Annotations
    CLI : dotnet add package Swashbuckle.AspNetCore.Annotations
    ```

2. In the `ConfigureServices` method of `Startup.cs`, enable annotations within in the Swagger config block:

    ```csharp
    services.AddSwaggerGen(c =>
    {
       ...

       c.EnableAnnotations();
    });
    ```

### Enrich Operation Metadata ###

Once annotations have been enabled, you can enrich the generated Operation metadata by decorating actions with a `SwaggerOperationAttribute`.

```csharp
[HttpPost]

[SwaggerOperation(
    Summary = "Creates a new product",
    Description = "Requires admin privileges",
    OperationId = "CreateProduct",
    Tags = new[] { "Purchase", "Products" }
)]
public IActionResult Create([FromBody]Product product)
```

### Enrich Response Metadata ###

ASP.NET Core provides the `ProducesResponseTypeAttribute` for listing the different responses that can be returned by an action. These attributes can be combined with XML comments, as described [above](#include-descriptions-from-xml-comments), to include human friendly descriptions with each response in the generated Swagger. If you'd prefer to do all of this with a single attribute, and avoid the use of XML comments, you can use `SwaggerResponseAttribute`s instead:

```csharp
[HttpPost]
[SwaggerResponse(201, "The product was created", typeof(Product))]
[SwaggerResponse(400, "The product data is invalid")]
public IActionResult Create([FromBody]Product product)
```

### Enrich Parameter Metadata ###

You can annotate "path", "query" or "header" bound parameters or properties (i.e. decorated with `[FromRoute]`, `[FromQuery]` or `[FromHeader]`) with a `SwaggerParameterAttribute` to enrich the corresponding `Parameter` metadata that's generated by Swashbuckle:

```csharp
[HttpGet]
public IActionResult GetProducts(
    [FromQuery, SwaggerParameter("Search keywords", Required = true)]string keywords)
```

### Enrich RequestBody Metadata ###

You can annotate "body" bound parameters or properties (i.e. decorated with `[FromBody]`) with a `SwaggerRequestBodyAttribute` to enrich the corresponding `RequestBody` metadata that's generated by Swashbuckle:

```csharp
[HttpPost]
public IActionResult CreateProduct(
    [FromBody, SwaggerRequestBody("The product payload", Required = true)]Product product)
```

### Enrich Schema Metadata ###

You can annotate classes or properties with a `SwaggerSchemaAttribute` to enrich the corresponding `Schema` metadata that's generated by Swashbuckle:

```csharp
[SwaggerSchema(Required = new[] { "Description" })]
public class Product
{
	[SwaggerSchema("The product identifier", ReadOnly = true)]
	public int Id { get; set; }

	[SwaggerSchema("The product description")]
	public string Description { get; set; }

	[SwaggerSchema("The date it was created", Format = "date")]
	public DateTime DateCreated { get; set; }
}
```

_NOTE: In Swagger / OpenAPI, serialized objects AND contained properties are represented as `Schema` instances, hence why this annotation can be applied to both classes and properties. Also worth noting, "required" properties are specified as an array of property names on the top-level schema as opposed to a flag on each individual property._

### Apply Schema Filters to Specific Types ###

The `SwaggerGen` package provides several extension points, including Schema Filters ([described here](#extend-generator-with-operation-schema--document-filter)) for customizing ALL generated Schemas. However, there may be cases where it's preferable to apply a filter to a specific Schema. For example, if you'd like to include an example for a specific type in your API. This can be done by decorating the type with a `SwaggerSchemaFilterAttribute`:

```csharp
// Product.cs
[SwaggerSchemaFilter(typeof(ProductSchemaFilter))
public class Product
{
    ...
}

// ProductSchemaFilter.cs
public class ProductSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        schema.Example = new OpenApiObject
        {
            [ "Id" ] = new OpenApiInteger(1),
            [ "Description" ] = new OpenApiString("An awesome product")
        };
    }
}
```

### Add Tag Metadata

By default, the Swagger generator will tag all operations with the controller name. This tag is then used to drive the operation groupings in the swagger-ui. If you'd like to provide a description for each of these groups, you can do so by adding metadata for each controller name tag via the `SwaggerTagAttribute`:

```csharp
[SwaggerTag("Create, read, update and delete Products")]
public class ProductsController
{
    ...
}
```

_NOTE: This will add the above description specifically to the tag named "Products". Therefore, you should avoid using this attribute if you're tagging Operations with something other than controller name - e.g. if you're customizing the tagging behavior with `TagActionsBy`._

### List Known Subtypes for Inheritance and Polymorphism ###

If you want to use Swashbuckle's [inheritance and/or polymorphism behavior](#inheritance-and-polymorphism), you can use annotations to _explicitly_ indicate the "known" subtypes for a given base type. This will override the default selector function, which selects _all_ subtypes in the same assembly as the base type, and therefore needs to be explicitly enabled when you enable Annotations:

```csharp
// Startup.cs
services.AddSwaggerGen(c =>
{
    c.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
});

// Shape.cs
[SwaggerSubType(typeof(Rectangle))]
[SwaggerSubType(typeof(Circle))]
public abstract class Shape
{
}
```

### Enrich Polymorphic Base Classes with Discriminator Metadata ###

If you're using annotations to _explicitly_ indicate the "known" subtypes for a polymorphic base type, you can combine the `SwaggerDiscriminatorAttribute` with the `SwaggerSubTypeAttribute` to provide additional metadata about the "discriminator" property, which will then be incorporated into the generated schema definition:


```csharp
// Startup.cs
services.AddSwaggerGen(c =>
{
    c.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
});

// Shape.cs
[SwaggerDiscriminator("shapeType")]
[SwaggerSubType(typeof(Rectangle), DiscriminatorValue = "rectangle")]
[SwaggerSubType(typeof(Circle), DiscriminatorValue = "circle")]
public abstract class Shape
{
    public ShapeType { get; set; }
}
```

This indicates that the corresponding payload will have a "shapeType" property to discriminate between subtypes, and that property will have a value of "rectangle" if the payload represents a `Rectangle` type and a value of "circle" if it represents a `Circle` type. This detail will be described in the generated schema definition as follows:

```
schema: {
  oneOf: [
    {
      $ref: "#/components/schemas/Rectangle"
    },
    {
      $ref: "#/components/schemas/Circle"
    },
  ],
  discriminator: {
    propertyName: shapeType,
    mapping: {
      rectangle: "#/components/schemas/Rectangle",
      circle: "#/components/schemas/Circle",
    }
  }
}
```

## Swashbuckle.AspNetCore.Cli ##

### Retrieve Swagger Directly from a Startup Assembly ###

Once your application has been setup with Swashbuckle (see [Getting Started](#getting-started)), you can use the Swashbuckle CLI tool to retrieve Swagger / OpenAPI JSON directly from your application's startup assembly, and write it to file. This can be useful if you want to incorporate Swagger generation into a CI/CD process, or if you want to serve it from static file at run-time.

It's packaged as a [.NET Core Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) that can be installed and used via the dotnet SDK.

> :warning: The tool needs to load your Startup DLL and it's dependencies at runtime. Therefore, you should use a version of the `dotnet` SDK that is compatible with your application. For example, if your app targets `netcoreapp2.1`, then you should use version 2.1 of the SDK to run the CLI tool. If it targetes `netcoreapp3.0`, then you should use version 3.0 of the SDK and so on.

#### Using the tool with the .NET Core 2.1 SDK

1. Install as a [global tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-global-tool)

    ```
    dotnet tool install -g --version 6.1.5 Swashbuckle.AspNetCore.Cli
    ```

2. Verify that the tool was installed correctly

    ```
    swagger tofile --help
    ```

3. Generate a Swagger/ OpenAPI document from your application's startup assembly

	```
	swagger tofile --output [output] [startupassembly] [swaggerdoc]
	```

	Where ...
	* [output] is the relative path where the Swagger JSON will be output to
	* [startupassembly] is the relative path to your application's startup assembly
	* [swaggerdoc] is the name of the swagger document you want to retrieve, as configured in your startup class

#### Using the tool with the .NET Core 3.0 SDK or later

1. In your project root, create a tool manifest file:

    ```
    dotnet new tool-manifest
    ```

2. Install as a [local tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool)

    ```
    dotnet tool install --version 6.1.5 Swashbuckle.AspNetCore.Cli
    ```

3. Verify that the tool was installed correctly

    ```
    dotnet swagger tofile --help
    ```

4. Generate a Swagger / OpenAPI document from your application's startup assembly

	```
	dotnet swagger tofile --output [output] [startupassembly] [swaggerdoc]
	```

	Where ...
	* [output] is the relative path where the Swagger JSON will be output to
	* [startupassembly] is the relative path to your application's startup assembly
	* [swaggerdoc] is the name of the swagger document you want to retrieve, as configured in your startup class

### Use the CLI Tool with a Custom Host Configuration

Out-of-the-box, the tool will execute in the context of a "default" web host. However, in some cases you may want to bring your own host environment, for example if you've configured a custom DI container such as Autofac. For this scenario, the Swashbuckle CLI tool exposes a convention-based hook for your application.

That is, if your application contains a class that meets either of the following naming conventions, then that class will be used to provide a host for the CLI tool to run in.

- `public class SwaggerHostFactory`, containing a public static method called `CreateHost` with return type `IHost`
- `public class SwaggerWebHostFactory`, containing a public static method called `CreateWebHost` with return type `IWebHost`

For example, the following class could be used to leverage the same host configuration as your application:

```csharp
public class SwaggerHostFactory
{
    public static IHost CreateHost()
    {
        return Program.CreateHostBuilder(new string[0]).Build();
    }
}
```

## Swashbuckle.AspNetCore.ReDoc ##

<h3 id="redoc-change-relative-path-to-the-ui">Change Relative Path to the UI</h3>

By default, the ReDoc UI will be exposed at "/api-docs". If necessary, you can alter this when enabling the ReDoc middleware:

```csharp
app.UseReDoc(c =>
{
    c.RoutePrefix = "docs"
    ...
}
```

<h3 id="redoc-change-document-title">Change Document Title</h3>

By default, the ReDoc UI will have a generic document title. You can alter this when enabling the ReDoc middleware:

```csharp
app.UseReDoc(c =>
{
    c.DocumentTitle = "My API Docs";
    ...
}
```

### Apply ReDoc Parameters ###

ReDoc ships with its own set of configuration parameters, all described here https://github.com/Rebilly/ReDoc/blob/master/README.md#redoc-options-object. In Swashbuckle, most of these are surfaced through the ReDoc middleware options:

```csharp
app.UseReDoc(c =>
{
    c.SpecUrl("/v1/swagger.json");
    c.EnableUntrustedSpec();
    c.ScrollYOffset(10);
    c.HideHostname();
    c.HideDownloadButton();
    c.ExpandResponses("200,201");
    c.RequiredPropsFirst();
    c.NoAutoAuth();
    c.PathInMiddlePanel();
    c.HideLoading();
    c.NativeScrollbars();
    c.DisableSearch();
    c.OnlyRequiredInSamples();
    c.SortPropsAlphabetically();
});
```

_Using `c.SpecUrl("/v1/swagger.json")` multiple times within the same `UseReDoc(...)` will not add multiple urls._

<h3 id="redoc-inject-custom-css">Inject Custom CSS</h3>

To tweak the look and feel, you can inject additional CSS stylesheets by adding them to your `wwwroot` folder and specifying the relative paths in the middleware options:

```csharp
app.UseReDoc(c =>
{
    ...
    c.InjectStylesheet("/redoc/custom.css");
}
```

It is also possible to modify the theme by using the `AdditionalItems` property, see https://github.com/Rebilly/ReDoc/blob/master/README.md#redoc-options-object for more information.

```csharp
app.UseReDoc(c =>
{
    ...
    c.ConfigObject.AdditionalItems = ...
}
```

<h3 id="redoc-customize-indexhtml">Customize index.html</h3>

To customize the UI beyond the basic options listed above, you can provide your own version of the ReDoc index.html page:

```csharp
app.UseReDoc(c =>
{
    c.IndexStream = () => GetType().Assembly
        .GetManifestResourceStream("CustomIndex.ReDoc.index.html"); // requires file to be added as an embedded resource
});
```

_To get started, you should base your custom index.html on the [default version](src/Swashbuckle.AspNetCore.ReDoc/index.html)_
