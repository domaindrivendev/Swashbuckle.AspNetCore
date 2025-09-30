# Configuration and Customization of `Swashbuckle.AspNetCore.SwaggerGen`

## Assign Explicit OperationIds

In OpenAPI, operations may be assigned an `operationId`. This ID must be unique among all operations described in the API.
Tools and libraries (e.g. client generators) may use the `operationId` to uniquely identify an operation, therefore, it is
recommended to follow common programming naming conventions.

Auto-generating an ID that matches these requirements, while also providing a name that would be meaningful in client libraries,
is a non-trivial task and thus Swashbuckle.AspNetCore omits the `operationId` by default. However, if necessary, you can assign
`operationIds` by decorating individual routes or by providing a custom strategy.

### Option 1: Decorate routes with a `Name` property

```csharp
// operationId = "GetProductById"
[HttpGet("{id}", Name = "GetProductById")]
public IActionResult Get(int id)
{
    // ...
}
```

### Option 2: Provide a custom strategy

📝 `Startup.cs`

```csharp
services.AddSwaggerGen(options =>
{
    // Other configuration...
    
    // Use method name as operationId
    options.CustomOperationIds(apiDescription =>
    {
        return apiDescription.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });
});
```

📝 `ProductsController.cs`

```csharp
// operationId = "GetProductById"
[HttpGet("{id}")]
public IActionResult GetProductById(int id)
{
    // ...
}
```

> [!NOTE]
> With either approach, API authors are responsible for ensuring the uniqueness of `operationIds` across all operations.

## List Operation Responses

By default, Swashbuckle.AspNetCore will generate an HTTP `"200"` response for each operation. If the endpoint returns a
response object, then this will be used to generate a schema for the response body. For example:

```csharp
[HttpPost("{id}")]
public Product GetById(int id)
{
    // ...
}
```

This endpoint will produce the following response metadata:

```yaml
responses: {
  200: {
    description: "OK",
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

### Explicit Responses

If you need to specify a different status code and/or additional responses, or your MVC actions return `IActionResult` instead
of a response object, you can explicitly describe responses with `[ProducesResponseType]` which is part of ASP.NET Core. For example:

```csharp
[HttpPost("{id}")]
[ProducesResponseType(typeof(Product), 200)]
[ProducesResponseType(typeof(IDictionary<string, string>), 400)]
[ProducesResponseType(500)]
public IActionResult GetById(int id)
{
    // ...
}
```

This endpoint will produce the following response metadata:

```yaml
responses: {
  200: {
    description: "OK",
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
    description: "Internal Server Error",
    content: {}
  }
}
```

## Flag Required Parameters and Schema Properties

In an OpenAPI document, you can flag parameters and schema properties that are required for a request. If a parameter
(top-level or property-based) is decorated with `[BindRequired]` or `[Required]`, then Swashbuckle.AspNetCore will automatically
flag it as a `required` parameter in the generated OpenAPI document:

📝 `ProductsController.cs`

```csharp
public IActionResult Search([FromQuery, BindRequired] string keywords, [FromQuery] PagingOptions paging)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // ...
}
```

📝 `PagingOptions.cs`

```csharp
public class PagingOptions
{
    [Required]
    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
```

In addition to parameters, Swashbuckle.AspNetCore will also honor `[Required]` when used in a model that's bound to the request body.
In this case, the decorated properties will be flagged as `required` properties in the body description:

📝 `ProductsController.cs`

```csharp
public IActionResult Create([FromBody] Product product)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // ...
}
```

📝 `Product.cs`

```csharp
public class Product
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }
}
```

## Handle Forms and File Uploads

This MVC controller will accept two form field values and one named file upload from the same form:

```csharp
[HttpPost]
public void UploadFile([FromForm] string description, [FromForm] DateTime clientDate, IFormFile file)
{
    // ...
}
```

> [!IMPORTANT]
> As per the [ASP.NET Core documentation](https://learn.microsoft.com/aspnet/core/mvc/models/file-uploads), you're not supposed to
> decorate `IFormFile` parameters with the `[FromForm]` attribute as the binding source is automatically inferred from the type. In fact,
> the inferred value is `BindingSource.FormFile` and if you apply the attribute it will be set to `BindingSource.Form` instead, which breaks
> `ApiExplorer`, the metadata component that ships with ASP.NET Core and is heavily relied on by Swashbuckle.AspNetCore. One particular issue here is
> that SwaggerUI will not treat the parameter as a file and so will not display a file upload button, if you do mistakenly include this attribute.

## Handle File Downloads

> [!IMPORTANT]
> `ApiExplorer` (the ASP.NET Core metadata component that Swashbuckle.AspNetCore is built on) **does not** surface the `FileResult` types by
> default and so you need to explicitly configure it to do so with `[ProducesResponseType]`:

```csharp
[HttpGet("{fileName}")]
[ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "image/jpeg")]
public FileStreamResult GetImage(string fileName)
{
    // ...
}
```

## Include Descriptions from XML Comments

To enhance the generated docs with human-friendly descriptions, you can annotate endpoints and models with
[XML Comments](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/) and configure Swashbuckle.AspNetCore
to include those comments into the generated OpenAPI document.

1. Open the Properties dialog for your project, click the "Build" tab and ensure that "XML documentation file" is checked, or add an
   `<GenerateDocumentationFile>true</GenerateDocumentationFile>` element to a `<PropertyGroup>` in your `.csproj` file. This
   will produce a file containing all XML comments at build-time.

  > At this point, any classes or methods that are **not** annotated with XML comments will trigger a build warning. To suppress this,
  > enter the warning code `1591` into the _"Suppress warnings"_ field in the Properties dialog or add `<NoWarn>$(NoWarn);1591</NoWarn>` to a
  > `<PropertyGroup>` of your `.csproj` project file.

1. Configure Swashbuckle.AspNetCore to incorporate the XML comments on file into the generated OpenAPI JSON:

   ```csharp
   services.AddSwaggerGen(options =>
   {
       options.SwaggerDoc(
           "v1",
           new OpenApiInfo
           {
               Title = "My API - V1",
               Version = "v1"
           }
        );

        options.IncludeXmlComments(Assembly.GetExecutingAssembly());
        // or options.IncludeXmlComments(typeof(MyController).Assembly));
   });
   ```

2. Annotate your endpoints with `summary`, `remarks`, `param` and/or `response` tags as desired:

   ```csharp
   /// <summary>
   /// Retrieves a specific product by unique id
   /// </summary>
   /// <remarks>Awesomeness!</remarks>
   /// <param name="id" example="123">The product id</param>
   /// <response code="200">Product retrieved</response>
   /// <response code="404">Product not found</response>
   /// <response code="500">Oops! Can't lookup your product right now</response>
   [HttpGet("{id}")]
   [ProducesResponseType(typeof(Product), 200)]
   [ProducesResponseType(404)]
   [ProducesResponseType(500)]
   public Product GetById(int id)
   {
       // ...
   }
   ```

3. Annotate your types with `summary` and `example` tags, other tags (`remarks`, `para`, etc.) are not supported:

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

        /// <summary>
        /// The sizes the product is available in
        /// </summary>
        /// <example>["Small", "Medium", "Large"]</example>
        public List<string> Sizes { get; set; }
    }
    ```

4. Rebuild your project to update the XML Comments file and navigate to the OpenAPI JSON endpoint. Note how the descriptions are
mapped onto corresponding OpenAPI properties.

> [!NOTE]
> You can also provide OpenAPI schema descriptions by annotating your API models and their properties with `<summary>` tags. If you
> have multiple XML comments files (e.g. separate libraries for controllers and models), you can invoke the `IncludeXmlComments` method
> multiple times and they will all be merged into the generated OpenAPI document.

## Provide Global API Metadata

In addition to `"PathItems"`, `"Operations"` and `"Responses"`, which Swashbuckle.AspNetCore generates for you, OpenAPI also supports
[global metadata](https://swagger.io/specification/#openapi-object). For example, you can provide a full description for your API, terms
of service or even contact and licensing information:

```csharp
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
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
                Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
            }
        }
    );
});
```

> [!TIP]
> Use IntelliSense to see what other members are available.

## Generate Multiple OpenAPI Documents

With the setup described above, the generator will include all API operations in a single Swagger document. However, you can
create multiple documents if necessary. For example, you may want a separate document for each version of your API. To do this, start
by defining multiple Swagger documents in your application startup code:

```csharp
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API - V1", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "My API - V2", Version = "v2" });
});
```

> [!NOTE]
> Take note of the first argument to SwaggerDoc. It **must** be a URI-friendly name that uniquely identifies the document.
> It's subsequently used to make up the path for requesting the corresponding Swagger JSON. For example, with the default
> routing, the above documents will be available at `/swagger/v1/swagger.json` and `/swagger/v2/swagger.json`.

Next, you'll need to inform Swashbuckle which actions to include in each document. Although this can be customized (see below), by
default, the generator will use the `ApiDescription.GroupName` property, part of the built-in metadata layer that ships with
ASP.NET Core, to make this distinction. You can set this by decorating individual actions or by applying an application-wide convention.

### Decorate Individual Actions

To include an action in a specific Swagger document, decorate it with `[ApiExplorerSettings]` and set `GroupName` to the corresponding
document name (case sensitive):

```csharp
[HttpPost]
[ApiExplorerSettings(GroupName = "v2")]
public void Post([FromBody]Product product)
{
    // ...
}
```

### Assign Actions to Documents by Convention

To group by convention instead of decorating every action, you can apply a custom controller or action convention. For example, you
could wire up the following convention to assign actions to documents based on the controller namespace.

📝 `ApiExplorerGroupPerVersionConvention.cs`

```csharp
public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace; // e.g. "Controllers.V1"
        var apiVersion = controllerNamespace.Split('.').Last().ToLower();

        controller.ApiExplorer.GroupName = apiVersion;
    }
}
```

📝 `Startup.cs`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(options =>
        options.Conventions.Add(new ApiExplorerGroupPerVersionConvention())
    );

    //...
}
```

### Customize the Action Selection Process

When selecting actions for a given Swagger document, the generator invokes a `DocInclusionPredicate` against every `ApiDescription`
that's surfaced by the framework. The default implementation inspects `ApiDescription.GroupName` and returns `true` if the value is either null
or equal to the requested document name. However, you can also provide a custom inclusion predicate. For example, if you're using an
attribute-based approach to implement API versioning (e.g. `Microsoft.AspNetCore.Mvc.Versioning`), you could configure a custom predicate that leverages the versioning attributes instead:

```csharp
options.DocInclusionPredicate((docName, apiDesc) =>
{
    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
    {
        return false;
    }

    var versions = methodInfo.DeclaringType
        .GetCustomAttributes(true)
        .OfType<ApiVersionAttribute>()
        .SelectMany(attribute => attribute.Versions);

    return versions.Any(v => $"v{v.ToString()}" == docName);
});
```

### Exposing Multiple Documents through the UI

If you're using the `SwaggerUI` middleware, you'll need to specify any additional OpenAPI endpoints you want to expose.
See [List Multiple OpenAPI Documents](configure-and-customize-swaggerui.md#list-multiple-openapi-documents) for more information.

## Omit Obsolete Operations and/or Schema Properties

The [OpenAPI specification][swagger-specification] includes a `deprecated` flag for indicating that an operation is deprecated
(obsolete) and should be refrained from being used. The OpenAPI generator will automatically set this flag if the corresponding action is
decorated with the `[Obsolete]` attribute. However, instead of setting a flag, you can configure the generator to ignore obsolete actions altogether:

```csharp
services.AddSwaggerGen(options =>
{
    options.IgnoreObsoleteActions();
});
```

A similar approach can also be used to omit obsolete properties from `Schemas` in the OpenAPI document. That is, you can decorate
model properties with `[Obsolete]` and configure Swashbuckle.AspNetCore to omit those properties when generating JSON schemas:

```csharp
services.AddSwaggerGen(options =>
{
    options.IgnoreObsoleteProperties();
});
```

## Omit Arbitrary Operations

You can omit operations from the OpenAPI output by decorating individual actions or by applying an application-wide convention.

### Decorate Individual Actions

To omit a specific action, decorate it with `[ApiExplorerSettings]` and set the `IgnoreApi` property to `true`:

```csharp
[HttpGet("{id}")]
[ApiExplorerSettings(IgnoreApi = true)]
public Product GetById(int id)
{
    // ...
}
```

### Omit Actions by Convention

To omit actions by convention instead of decorating them individually, you can apply a custom action convention. For example, you
could wire up the following convention to only document `GET` operations:

📝 `ApiExplorerGetsOnlyConvention.cs`

```csharp
public class ApiExplorerGetsOnlyConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        action.ApiExplorer.IsVisible = action.Attributes.OfType<HttpGetAttribute>().Any();
    }
}
```

📝 `Startup.cs`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(options =>
        options.Conventions.Add(new ApiExplorerGetsOnlyConvention())
    );

    //...
}
```

## Customize Operation Tags (e.g. for UI Grouping)

The [OpenAPI specification][swagger-specification] allows one or more "tags" to be assigned to an operation. The OpenAPI generator
will assign the controller name as the default tag. This is important to note if you're using the `SwaggerUI` middleware as it uses this
value to group operations.

You can override the default tag by providing a function that applies tags by convention. For example, the following configuration will
tag, and therefore group operations in the UI, by HTTP method:

```csharp
services.AddSwaggerGen(options =>
{
    options.TagActionsBy(api => [api.HttpMethod]);
});
```

## Change Operation Sort Order (e.g. for UI Sorting)

By default, actions are ordered by assigned tag (see above) before they're grouped into the path-centric, nested structure of the
[OpenAPI specification][swagger-specification]. However, you can change the default ordering of actions with a custom sorting strategy:

```csharp
services.AddSwaggerGen(options =>
{
    options.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});
```

> [!NOTE]
> This dictates the sort order **before** actions are grouped and transformed into the OpenAPI format. Therefore it affects the ordering
> of groups (i.e. OpenAPI "PathItems"), **and** the ordering of operations within a group, in the OpenAPI document that is output.

## Customize Schema Ids

If the generator encounters complex parameter or response types, it will generate a corresponding JSON schema, add it to the global
`components:schemas` dictionary, and reference it from the operation description by unique Id. For example, if you have an action
that returns a `Product` type, then the generated schema will be referenced as follows:

```yaml
responses: {
  200: {
    description: "OK",
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

However, if it encounters multiple types with the same name but different namespaces (e.g. `RequestModels.Product` and `ResponseModels.Product`),
then Swashbuckle.AspNetCore will raise an exception due to _"Conflicting schemaIds"_. In this case, you'll need to provide a custom Id strategy that
further qualifies the name:

```csharp
services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds((type) => type.FullName);
});
```

> [!NOTE]
> See [this GitHub issue](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2703) for support for nested types.

## Override Schema for Specific Types

Out-of-the-box, Swashbuckle.AspNetCore performs a best-effort generating JSON schemas that accurately describe your request and response payloads.
However, if you're customizing serialization behavior for certain types in your API, you may need to help it out to get accurate output.

For example, you might have a class with multiple properties that you want to represent in JSON as a comma-separated string. To do this you
would probably implement a custom `JsonConverter`. In this case, Swashbuckle.AspNetCore doesn't know how the converter is implemented and so you would
need to provide it with a schema that accurately describes the type:

📝 `PhoneNumber.cs`

```csharp
public class PhoneNumber
{
    public string CountryCode { get; set; }

    public string AreaCode { get; set; }

    public string SubscriberId { get; set; }
}
```

📝 `Startup.cs`

```csharp
services.AddSwaggerGen(options =>
{
    options.MapType<PhoneNumber>(() => new OpenApiSchema { Type = JsonSchemaType.String });
});
```

## Extend Generator with Operation, Schema and Document Filters

Swashbuckle.AspNetCore exposes a filter pipeline that hooks into the generation process. Once generated, individual metadata objects are passed
into the pipeline where they can be modified further. You can wire up custom filters to enrich the generated `Operations`, `Schemas`
and `Documents`.

### Operation Filters

Swashbuckle.AspNetCore retrieves an `ApiDescription`, part of ASP.NET Core, for every action and uses it to generate a corresponding `OpenApiOperation`.
Once generated, it passes the `OpenApiOperation` and the `ApiDescription` through the list of configured Operation Filters.

In a typical filter implementation, you would inspect the `ApiDescription` for relevant information (e.g. route information, action attributes etc.)
and then update the `OpenApiOperation` accordingly. For example, the following filter lists an additional `401` response for all actions that
are decorated with `[Authorize]`:

📝 `AuthResponsesOperationFilter.cs`

```csharp
public class AuthResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>()
            .Any() ?? false;

        if (hasAuthAttributes)
        {
            operation.Responses ??= [];
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
        }
    }
}
```

📝 `Startup.cs`

```csharp
services.AddSwaggerGen(options =>
{
    options.OperationFilter<AuthResponsesOperationFilter>();
});
```

> [!NOTE]
> Filter pipelines are DI-aware. That is, you can create filters with constructor parameters and if the parameter types
> are registered with the DI framework, they'll be automatically injected when the filters are instantiated.

### Schema Filters

Swashbuckle.AspnetCore generates an OpenAPI-flavored [JSONSchema](https://swagger.io/specification/#schema-object) for every parameter, response
and property type that's exposed by your endpoints. Once generated, it passes the schema and type through the list of configured
Schema Filters.

The example below adds an [AutoRest vendor extension](https://github.com/Azure/autorest/blob/main/docs/extensions/readme.md#x-ms-enum)
to inform the AutoRest tool how enums should be modelled when it generates the API client.

📝 `AutoRestSchemaFilter.cs`

```csharp
public class AutoRestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type.IsEnum && schema is OpenApiSchema concrete)
        {
            concrete.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            concrete.Extensions.Add(
                "x-ms-enum",
                new JsonNodeExtension(
                    new JsonObject
                    {
                        ["name"] = type.Name,
                        ["modelAsString"] = true
                    }
                )
            );
        }
    }
}
```

📝 `Startup.cs`

```csharp
services.AddSwaggerGen(options =>
{
    options.SchemaFilter<AutoRestSchemaFilter>();
});
```

The example below allows for automatic schema generation of generic `Dictionary<Enum, TValue>` objects.
Note that this only generates the OpenAPI document; `System.Text.Json` is not able to parse dictionary enums by default,
so you will need [a special JsonConverter, as shown in the .NET documentation](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/converters-how-to#sample-factory-pattern-converter).

📝 `DictionaryTKeyEnumTValueSchemaFilter.cs`

```csharp
public class DictionaryTKeyEnumTValueSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete)
        {
            return;
        }

        // Only run for fields that are a Dictionary<Enum, TValue>
        if (!context.Type.IsGenericType || !context.Type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
        {
            return;
        }

        var genericArgs = context.Type.GetGenericArguments();
        var keyType = genericArgs[0];
        var valueType = genericArgs[1];

        if (!keyType.IsEnum)
        {
            return;
        }

        concrete.Type = JsonSchemaType.Object;
        concrete.Properties = keyType.GetEnumNames().ToDictionary(
            name => name,
            name => context.SchemaGenerator.GenerateSchema(valueType, context.SchemaRepository));
    }
}
```

📝 `Startup.cs`

```csharp
services.AddSwaggerGen(options =>
{
    // These will be replaced by DictionaryTKeyEnumTValueSchemaFilter, but are needed to avoid
    // an error. You will need one for every kind of Dictionary<,> you have.
    options.MapType<Dictionary<MyEnum, List<string>>>(() => new OpenApiSchema());
    options.SchemaFilter<DictionaryTKeyEnumTValueSchemaFilter>();
});
```

### Document Filters

Once an `OpenApiDocument` has been generated, it too can be passed through a set of pre-configured Document Filters.
This gives full control to modify the document however you see fit. To ensure you're still returning valid OpenAPI JSON, you
should have a read through the [specification][swagger-specification] before using this filter type.

The example below provides a description for any tags that are assigned to operations in the document:

```csharp
public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new HashSet<OpenApiTag>()
        {
            new() { Name = "Products", Description = "Browse/manage the product catalog" },
            new() { Name = "Orders", Description = "Submit orders" }
        };
    }
}
```

> [!NOTE]
> If you're using the `SwaggerUI` middleware, the `TagDescriptionsDocumentFilter` demonstrated above
> could be used to display additional descriptions beside each group of operations.

## Add Security Definitions and Requirements

In OpenAPI, you can describe how your API is secured by defining one or more security schemes (e.g. Basic, API key, OAuth2 etc.)
and declaring which of those schemes are applicable globally or for specific operations. For more details, take a look at the
[Security Requirement Object in the OpenAPI specification](https://swagger.io/specification/#security-requirement-object).

In Swashbuckle.AspNetCore, you can define schemes by invoking the `AddSecurityDefinition` method, providing a name and an instance of
`OpenApiSecurityScheme`. For example you can define an [OAuth 2.0 - implicit flow](https://oauth.net/2/) as follows:

📝 `Startup.cs`

```csharp
services.AddSwaggerGen(options =>
{
    // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    ["readAccess"] = "Access read operations",
                    ["writeAccess"] = "Access write operations"
                }
            }
        }
    });
});
```

> [!NOTE]
> In addition to defining a scheme, you also need to indicate which operations that scheme is applicable to. You can apply schemes
> globally (i.e. to **all** operations) through the `AddSecurityRequirement` method. The example below indicates that the scheme called
> `"oauth2"` should be applied to all operations, and that the `"readAccess"` and `"writeAccess"` scopes are required. When applying
> schemes of type other than `"oauth2"`, the array of scopes **must** be empty.

📝 `Startup.cs`

```csharp
services.AddSwaggerGen(options =>
{
    options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement()
    {
        [new OpenApiSecuritySchemeReference("oauth2", document)] = ["readAccess", "writeAccess"]
    });
});
```

If you have schemes that are only applicable for certain operations, you can apply them through an Operation filter. For
example, the following filter adds OAuth2 requirements based on the presence of the `AuthorizeAttribute`:

📝 `SecurityRequirementsOperationFilter.cs`

```csharp
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Policy names map to scopes
        var requiredScopes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Select(attribute => attribute.Policy!)
            .Distinct()
            .ToList();

        if (requiredScopes.Count > 0)
        {
            operation.Responses ??= [];
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            var scheme = new OpenApiSecuritySchemeReference("oauth2", context.Document);

            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [scheme] = requiredScopes
                }
            ];
        }
    }
}
```

> [!NOTE]
> If you're using the `SwaggerUI` middleware, you can enable interactive OAuth2.0 flows that are powered by the emitted
> security metadata. See [Enabling OAuth2.0 Flows](configure-and-customize-swaggerui.md#enable-oauth20-flows) for more details.

## Add Security Definitions and Requirements for Bearer authentication

```csharp
services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
```

## Inheritance and Polymorphism

OpenAPI defines the `allOf` and `oneOf` keywords for describing
[inheritance and polymorphism](https://swagger.io/docs/specification/v3_0/data-models/inheritance-and-polymorphism/#polymorphism) relationships
in schema definitions. For example, if you're using a base class for models that share common properties you can use the `allOf`
keyword to describe the inheritance hierarchy. Or, if your serializer supports polymorphic serialization/deserialization, you can use
the `oneOf` keyword to document all the "possible" schemas for requests/responses that vary by subtype.

### Enabling Inheritance

By default, Swashbuckle.AspNetCore flattens inheritance hierarchies. That is, for derived models, the inherited properties are combined and listed
alongside the declared properties. This can cause a lot of duplication in the generated OpenAPI document, particularly when there's multiple subtypes.
It's also problematic if you're using a client generator (e.g. NSwag) and would like to maintain the inheritance hierarchy in the generated
client models. To work around this, you can apply the `UseAllOfForInheritance` setting, and this will leverage the `allOf` keyword to
incorporate inherited properties by reference in the generated OpenAPI document:

```yaml
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

### Enabling Polymorphism

If your serializer supports polymorphic serialization/deserialization and you would like to list the possible subtypes for an action
that accepts/returns abstract base types, you can apply the `UseOneOfForPolymorphism` setting. As a result, the generated request/response
schemas will reference a collection of "possible" schemas instead of just the base class schema:

```json
"requestBody": {
  "content": {
    "application/json": {
      "schema": {
        "oneOf": [
          {
            "$ref": "#/components/schemas/Rectangle"
          },
          {
            "$ref": "#/components/schemas/Circle"
          }
        ]
      }
    }
  }
}
```

### Detecting Subtypes

As inheritance and polymorphism relationships can often become quite complex, not just in your own models but also within the .NET class
library, Swashbuckle.AspNetCore is selective about which hierarchies it does and doesn't expose in the generated OpenAPI document. By default, it will
pick up any subtypes that are defined in the same assembly as a given base type. If you'd like to override this behavior, you can provide a
custom selector method:

```csharp
services.AddSwaggerGen(options =>
{
    options.UseAllOfForInheritance();

    options.SelectSubTypesUsing(baseType =>
    {
        return typeof(Program).Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
    })
});
```

> [!NOTE]
> If you're using the [Swashbuckle Annotations library](configure-and-customize-annotations.md#configuration--customization-of-swashbuckleaspnetcoreannotations), it
> contains a custom selector that's based on the presence of `[JsonDerivedType]` attributes on base class definitions. This
> way, you can use simple attributes to explicitly list the inheritance and/or polymorphism relationships you want to expose.
> To enable this behavior, check out the [Annotations docs](configure-and-customize-annotations.md#list-known-subtypes-for-inheritance-and-polymorphism).

### Describing Discriminators

In conjunction with the `oneOf` and/or `allOf` keywords, OpenAPI supports a `discriminator` field on base schema definitions.
This keyword points to the property that identifies the specific type being represented by a given payload. In addition to the property
name, the discriminator description may also include a `mapping` which maps discriminator values to specific schema definitions.

For example, the Newtonsoft serializer supports polymorphic serialization/deserialization by emitting/accepting a `"$type"` property on
JSON instances. The value of this property will be the [assembly qualified type name](https://learn.microsoft.com/dotnet/api/system.type.assemblyqualifiedname)
of the type represented by a given JSON instance. So, to explicitly describe this behavior in OpenAPI, the corresponding request/response
schema could be defined as follows:

```yaml
components: {
  schemas: {
    Shape: {
      required: [
        "$type"
      ],
      type: "object",
      properties: {
        $type: {
          type: "string"
        },
        discriminator: {
          propertyName: "$type",
          mapping: {
            Rectangle: "#/components/schemas/Rectangle",
            Circle: "#/components/schemas/Circle"
          }
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

If `UseAllOfForInheritance` or `UseOneOfForPolymorphism` is enabled, and your serializer supports (and has enabled) emitting/accepting
a discriminator property, then Swashbuckle will automatically generate the corresponding `discriminator` metadata on base schema definitions.

Alternatively, if you've customized your serializer to support polymorphic serialization/deserialization, you can provide some custom
selector functions to determine the discriminator name and corresponding mapping:

```csharp
services.AddSwaggerGen(options =>
{
    options.UseAllOfForInheritance();

    options.SelectDiscriminatorNameUsing((baseType) => "TypeName");
    options.SelectDiscriminatorValueUsing((subType) => subType.Name);
});
```

> [!NOTE]
> If you're using the [Swashbuckle Annotations library](configure-and-customize-annotations.md#configuration--customization-of-swashbuckleaspnetcoreannotations), it
> contains custom selector functions that are based on the presence of `[JsonPolymorphic]` and `[JsonDerivedType]` attributes on base
> class definitions. This way, you can use simple attributes to explicitly provide discriminator metadata. To enable this behavior, check
> out the [Annotations documentation](configure-and-customize-annotations.md#enrich-polymorphic-base-classes-with-discriminator-metadata).

[swagger-specification]: https://swagger.io/specification/ "OpenAPI Specification"
