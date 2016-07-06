Swashbuckle
=========

Seamlessly adds a [Swagger](http://swagger.io/) to API's that are built with AspNet Core! It combines the built in metadata functionality ([ApiExplorer](https://github.com/aspnet/Mvc/tree/dev/src/Microsoft.AspNetCore.Mvc.ApiExplorer)) and Swagger/swagger-ui to provide a rich discovery, documentation and playground experience to your API consumers.

In addition to its [Swagger](http://swagger.io/specification/) generator, Swashbuckle also contains an embedded version of the [swagger-ui](https://github.com/swagger-api/swagger-ui) which it will automatically serve up once Swashbuckle is installed. This means you can complement your API with a slick discovery UI to assist consumers with their integration efforts. Best of all, it requires minimal coding and maintenance, allowing you to focus on building an awesome API

And that's not all ...

Once you have a Web API that can describe itself in Swagger, you've opened the treasure chest of Swagger-based tools including a client generator that can be targeted to a wide range of popular platforms. See [swagger-codegen](https://github.com/swagger-api/swagger-codegen) for more details.

**Swashbuckle Core Features:**

* Auto-generated [Swagger 2.0](https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md)
* Seamless integration of swagger-ui
* Reflection-based Schema generation for describing API types
* Extensibility hooks for customizing the generated Swagger doc
* Extensibility hooks for customizing the swagger-ui
* Out-of-the-box support for leveraging Xml comments
* Support for describing ApiKey, Basic Auth and OAuth2 schemes ... including UI support for the Implicit OAuth2 flow

**\*Swashbuckle 6.0.0**

Because Swashbuckle 6.0.0 is built on top of the next-gen implementation of .NET and ASP.NET (AspNet Core), the source code and public interface deviate significantly from previous versions. Once a stable release of AspNet Core (RC2 at time of writing) becomes available, I'll add a transition guide for Swashbuckle. In the meantime, you'll need to figure this out yourself. Hopefully, the examples [here](https://github.com/domaindrivendev/Ahoy/tree/master/test/WebSites) and the remainder of this README will get you there!


## Getting Started ##

Currently, Swashbuckle consists of two components (with more planned for future iterations) - __Swashbuckle.SwaggerGen__ and __Swashbuckle.SwaggerUi__. The former provides functionality to generate one or more Swagger documents directly from your API implementation and expose them as JSON endpoints. The latter provides an embedded version of the excellent [swagger-ui](https://github.com/swagger-api/swagger-ui) tool that can be served by your application and powered by the generated Swagger documents to describe your API.

These can be installed as separate Nuget packages if you need one without the other. If you want both, the simplest way to get started is by installing the meta-package __"Swashbuckle"__ which bundles them together:

	Install-Package Swashbuckle -Pre
    
Next, you'll need to configure Swagger in your _Startup.cs_.

    public void ConfigureServices(IServiceCollection services)
    {
    	... Configure MVC services ...

		// Inject an implementation of ISwaggerProvider with defaulted settings applied
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
		... Enable MVC middleware ...

		// Enable middleware to serve generated Swagger as a JSON endpoint
        app.UseSwagger();
        
        // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
        app.UseSwaggerUi();
    }
    

Once this is done, you should be able to spin up you app and browse browse the following Swagger JSON and UI endpoints respectively:

***\<your-root-url\>/swagger/v1/swagger.json***

***\<your-root-url\>/swagger/ui***
    

## Customizing the Generated Swagger Docs ##

The above snippet demonstrates the minimum configuration required to get the Swagger docs and swagger-ui up and running. However, these methods expose a range of configuration and extensibility options that you can pick and choose from, combining the convenience of sensible defaults with the flexibility to customize where you see fit. Read on to learn more.

Sorry :( - still in progress but coming real soon! For now, take a look at the sample projects for inspiration:

- https://github.com/domaindrivendev/Ahoy/tree/master/test/WebSites/Basic
- https://github.com/domaindrivendev/Ahoy/tree/master/test/WebSites/CustomizedUi
- https://github.com/domaindrivendev/Ahoy/tree/master/test/WebSites/MultipleVersions
- https://github.com/domaindrivendev/Ahoy/tree/master/test/WebSites/SecuritySchemes
- https://github.com/domaindrivendev/Ahoy/tree/master/test/WebSites/VirtualDirectory

<!--

### Custom Routes ###

The default route template for the Swagger JSON endpoints is _"swagger/{apiVersion}/swagger.json"_. You're free to change this so long as the provided template includes the {apiVersion} route parameter:

	app.UseSwaggerGen("api-docs/{apiVersion}/swagger.json");

For the swagger-ui, you can also change the baseRoute where the HTML and web assets are servered from:

	app.UseSwaggerUi("api-docs/ui");
    
__NOTE:__ If you do change the Swagger JSON routes, you'll also need to pass an additional parameter when enabling the UI to make it aware of the alternative discovery URL:

	app.UseSwaggerUi("api-docs/ui", "/api-docs/v1/swagger.json");
    
With these changes you should be able to browse the following Swagger JSON and UI endpoints:

***\<your-root-url\>/api-docs/v1/swagger.json***

***\<your-root-url\>/api-docs/ui***

### Global Metadata ###

In addition to operation descriptions, Swagger 2.0 provides properties for global metadata. These can be specified in _Startup.cs_ when enabling the ISwaggerProvider service.

    services.AddSwaggerGen(c =>
    {
        c.SingleApiVersion(new Info
        {
            Version = "v1",
            Title = "Swashbuckle Sample API",
            Description = "A sample API for testing Swashbuckle",
            TermsOfService = "Some terms",
            Contact = new Contact
            {
                Name = "Some contract",
                Url = "http://tempuri.org/contact",
                Email = "some.contact@tempuri.org"
            },
            License = new License
            {
                Name = "Some license",
                Url = "http://tempuri.org/license"
            }
        });
    });
            });


#### SingleApiVersion ####

Use this to describe a single version API. Swagger 2.0 includes an "Info" object to hold additional metadata for an API. Version and title are required but you may also provide
additional fields as shown above.

#### A note on RootUrl and Schemes ####

Unlike previous versions, these config options aren't currently supported. I plan on re-adding them in the next milestone but am undecided on the exact approach. They're a little different to other settings because they will need current request context to be dynamically assigned.

In the meantime, you can hardcode values by using an IDocumentFilter. See below for details.

### Describing Multiple API Versions ###

If your API has multiple versions, use __MultipleApiVersions__ instead of __SingleApiVersion__. In this case, you provide a lambda that tells Swashbuckle which actions should be included in the docs for a given API version.

    services.AddSwaggerGen(c =>
    {
        c.MultipleApiVersions(
            new[]
            {
                new Info { Version = "v1", Title = "API V1" },
                new Info { Version = "v2", Title = "API V2" }
            },
            (apiDescription, targetVersion) =>
            {
            	... provide your own implementation ...
            } 
        );
    });
    

### Describing Security/Authorization Schemes ###

You can also add security definitions to describe security schemes for the API. You can describe "Basic", "ApiKey" and "OAuth2" schemes. See https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md for more details.

    services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("oauth2", new OAuth2Scheme
        {
            Type = "oauth2",
            Flow = "implicit",
            AuthorizationUrl = "http://petstore.swagger.io/api/oauth/dialog",
            Scopes = new Dictionary<string, string>
                {
                    { "read", "read access" },
                    { "write", "write access" }
                }
        });
    });

__NOTE:__ These only define the schemes gloabally and need to be coupled with a corresponding "security" property at the document or operation level to indicate which schemes are required for each operation.  To do this, you'll need to implement a custom IDocumentFilter and/or IOperationFilter to set these properties according to your specific authorization implementation:

    services.AddSwaggerGen(c =>
    {
    	c.OperationFilter<AssignSecurityRequirements>();
    });

### Customize the Operation Listing ###

If necessary, you can ignore obsolete actions and provide custom grouping/sorting strategies for the list of Operations in a Swagger document:

    httpConfiguration
        .EnableSwagger(c =>
            {
                c.IgnoreObsoleteActions();

                c.GroupActionsBy(apiDesc => apiDesc.HttpMethod.ToString());

                c.OrderActionGroupsBy(new DescendingAlphabeticComparer());
            });

#### IgnoreObsoleteActions ####

Set this flag to omit operation descriptions for any actions decorated with the Obsolete attribute

__NOTE__: If you want to omit specific operations but without using the Obsolete attribute, you can create an IDocumentFilter or make use of the built in ApiExplorerSettingsAttribute

#### GroupActionsBy ####

Each operation can be assigned one or more tags which are then used by consumers for various reasons. For example, the swagger-ui groups operations according to the first tag of each operation. By default, this will be the controller name but you can use this method to override with any value.

#### OrderActionGroupsBy ####

You can also specify a custom sort order for groups (as defined by __GroupActionsBy__) to dictate the order in which operations are listed. For example, if the default grouping is in place (controller name) and you specify a descending alphabetic sort order, then actions from a ProductsController will be listed before those from a CustomersController. This is typically used to customize the order of groupings in the swagger-ui.

### Modifying Generated Schemas ###

Swashbuckle makes a best attempt at generating Swagger compliant JSON schemas for the various types exposed in your API. However, there may be occasions when more control of the output is needed.  This is supported through the following options:

    httpConfiguration
        .EnableSwagger(c =>
            {
                c.MapType<ProductType>(() => new Schema { type = "integer", format = "int32" });

                c.SchemaFilter<ApplySchemaVendorExtensions>();

                //c.UseFullTypeNameInSchemaIds();

                c.SchemaId(t => t.FullName.Contains('`') ? t.FullName.Substring(0, t.FullName.IndexOf('`')) : t.FullName);
                
                c.IgnoreObsoleteProperties();

                c.DescribeAllEnumsAsStrings();
            });

#### MapType ####

Use this option to override the Schema generation for a specific type.

It should be noted that the resulting Schema will be placed "inline" for any applicable Operations. While Swagger 2.0 supports inline definitions for "all" Schema types, the swagger-ui tool does not. It expects "complex" Schemas to be defined separately and referenced. For this reason, you should only use the __MapType__ option when the resulting Schema is a primitive or array type.

If you need to alter a complex Schema, use a Schema filter.

#### SchemaFilter ####

If you want to post-modify "complex" Schemas once they've been generated, across the board or for a specific type, you can wire up one or more Schema filters.

ISchemaFilter has the following interface:

    void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type);

A typical implementation will inspect the system Type and modify the Schema accordingly. If necessary, the schemaRegistry can be used to obtain or register Schemas for other Types

#### UseFullTypeNamesInSchemaIds ####

In a Swagger 2.0 document, complex types are typically declared globally and referenced by unique Schema Id. By default, Swashbuckle does NOT use the full type name in Schema Ids. In most cases, this works well because it prevents the "implementation detail" of type namespaces from leaking into your Swagger docs and UI. However, if you have multiple types in your API with the same class name, you'll need to opt out of this behavior to avoid Schema Id conflicts.  

#### SchemaId ####

Use this option to provide your own custom strategy for inferring SchemaId's for describing "complex" types in your API.

#### IgnoreObsoleteProperties ####

Set this flag to omit schema property descriptions for any type properties decorated with the Obsolete attribute 

#### DescribeAllEnumsAsStrings ####

In accordance with the built in JsonSerializer, Swashbuckle will, by default, describe enums as integers. You can change the serializer behavior by configuring the StringToEnumConverter globally or for a given enum type. Swashbuckle will honor this change out-of-the-box. However, if you use a different approach to serialize enums as strings, you can also force Swashbuckle to describe them as strings.

### Modifying Generated Operations ###

Similar to Schema filters, Swashbuckle also supports Operation and Document filters:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API"))
            {
                c.OperationFilter<AddDefaultResponse>();

                c.DocumentFilter<ApplyDocumentVendorExtensions>();
            });

#### OperationFilter ####

Post-modify Operation descriptions once they've been generated by wiring up one or more Operation filters.

IOperationFilter has the following interface:

    void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription);

A typical implementation will inspect the ApiDescription and modify the Operation accordingly. If necessary, the schemaRegistry can be used to obtain or register Schemas for Types that are used in the Operation.

#### DocumentFilter ####

Post-modify the entire Swagger document by wiring up one or more Document filters.

IDocumentFilter has the following interface:

    void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer);

This gives full control to modify the final SwaggerDocument. You can gain additional context from the provided SwaggerDocument (e.g. version) and IApiExplorer. You should have a good understanding of the [Swagger 2.0 spec.](https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md) before using this option.

### Wrapping the SwaggerGenerator with Additional Behavior ###

The default implementation of ISwaggerProvider, the interface used to obtain Swagger metadata for a given API, is the SwaggerGenerator. If neccessary, you can inject your own implementation or wrap the existing one with additional behavior. For example, you could use this option to inject a "Caching Proxy" that attempts to retrieve the SwaggerDocument from a cache before delegating to the built-in generator:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API"))
            {
				c.CustomProvider((defaultProvider) => new CachingSwaggerProvider(defaultProvider));
            });

### Including XML Comments ###

If you annotate Controllers and API Types with [Xml Comments](http://msdn.microsoft.com/en-us/library/b2s063f7(v=vs.110).aspx), you can incorporate those comments into the generated docs and UI. The Xml tags are mapped to Swagger properties as follows:

* **Action summary** -> Operation.summary
* **Action remarks** -> Operation.description
* **Parameter summary** -> Parameter.description
* **Type summary** -> Schema.descripton
* **Property summary** -> Schema.description (i.e. on a property Schema)

You can enable this by providing the path to one or more XML comments files:

    httpConfiguration
        .EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "A title for your API");
                c.IncludeXmlComments(GetXmlCommentsPathForControllers());
                c.IncludeXmlComments(GetXmlCommentsPathForModels());
            });

NOTE: You will need to enable output of the XML documentation file. This is enabled by going to project properties -> Build -> Output. The "XML documentation file" needs to be checked and a path assigned, such as "bin\Debug\MyProj.XML". You will also want to verify this across each build configuration. Here's an example of reading the file, but it may need to be modified according to your specific project settings:

    httpConfiguration
        .EnableSwagger(c =>
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
                var commentsFile = Path.Combine(baseDirectory, commentsFileName);

                c.SingleApiVersion("v1", "A title for your API");
                c.IncludeXmlComments(commentsFile);
                c.IncludeXmlComments(GetXmlCommentsPathForModels());
            });

#### Response Codes ####

Swashbuckle will automatically create a "success" response for each operation based on the action's return type. If it's a void, the status code will be 204 (No content), otherwise 200 (Ok). This mirrors WebApi's default behavior. If you need to change this and/or list additional response codes, you can use the non-standard "response" tag:

    /// <response code="201">Account created</response>
    /// <response code="400">Username already in use</response>
    public int Create(Account account)

### Working Around Swagger 2.0 Constraints ###

In contrast to Web API, Swagger 2.0 does not include the query string component when mapping a URL to an action. As a result, Swashbuckle will raise an exception if it encounters multiple actions with the same path (sans query string) and HTTP method. You can workaround this by providing a custom strategy to pick a winner or merge the descriptions for the purposes of the Swagger docs 

    httpConfiguration
        .EnableSwagger((c) =>
            {
                c.SingleApiVersion("v1", "A title for your API"));
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

See the following discussion for more details:

<https://github.com/domaindrivendev/Swashbuckle/issues/142>

## Customizing the swagger-ui ##

The swagger-ui is a JavaScript application hosted in a single HTML page (index.html), and it exposes several customization settings. Swashbuckle ships with an embedded version and includes corresponding configuration methods for each of the UI settings. If you require further customization, you can also inject your own version of "index.html". Read on to learn more.

### Customizations via the configuration API ###

If you're happy with the basic look and feel but want to make some minor tweaks, the following options may be sufficient:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API"))
        .EnableSwaggerUi(c =>
            {
                c.InjectStylesheet(containingAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
                c.InjectJavaScript(containingAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
                c.BooleanValues(new[] { "0", "1" });
                c.SetValidatorUrl("http://localhost/validator");
                c.DisableValidator();
                c.DocExpansion(DocExpansion.List);
				c.SupportedSubmitMethods("GET", "HEAD")
            });

#### InjectStylesheet ####

Use this to enrich the UI with one or more additional CSS stylesheets. The file(s) must be included in your project as an "Embedded Resource", and then the resource's "Logical Name" is passed to the method as shown above. See [Injecting Custom Content](#injecting-custom-content) for step by step instructions.

#### InjectJavaScript ####

Use this to invoke one or more custom JavaScripts after the swagger-ui has loaded. The file(s) must be included in your project as an "Embedded Resource", and then the resource's "Logical Name" is passed to the method as shown above. See [Injecting Custom Content](#injecting-custom-content) for step by step instructions.

#### BooleanValues ####

The swagger-ui renders boolean data types as a dropdown. By default, it provides "true" and "false" strings as the possible choices. You can use this option to change these to something else, for example 0 and 1.

#### SetValidatorUrl/DisableValidator ####

By default, swagger-ui will validate specs against swagger.io's online validator and display the result in a badge at the bottom of the page. Use these options to set a different validator URL or to disable the feature entirely.

#### DocExpansion ####

Use this option to control how the Operation listing is displayed. It can be set to "None" (default), "List" (shows operations for each resource), or "Full" (fully expanded: shows operations and their details).

#### SupportedSubmitMethods ####

Specify which HTTP operations will have the 'Try it out!' option. An empty paramter list disables it for all operations.

### Provide your own "index" file ###

As an alternative, you can inject your own version of "index.html" and customize the markup and swagger-ui directly. Use the __CustomAsset__ option to instruct Swashbuckle to return your version instead of the default when a request is made for "index". As with all custom content, the file must be included in your project as an "Embedded Resource", and then the resource's "Logical Name" is passed to the method as shown below. See [Injecting Custom Content](#injecting-custom-content) for step by step instructions.

For compatibility, you should base your custom "index.html" off [this version](https://github.com/domaindrivendev/Swashbuckle/blob/v5.2.1/Swashbuckle.Core/SwaggerUi/CustomAssets/index.html)

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API"))
        .EnableSwaggerUi(c =>
            {
                c.CustomAsset("index", yourAssembly, "YourWebApiProject.SwaggerExtensions.index.html");
            });

### Injecting Custom Content ###

The __InjectStylesheet__, __InjectJavaScript__ and __CustomAsset__ options all share the same mechanism for providing custom content. In each case, the file must be included in your project as an "Embedded Resource". The steps to do this are described below:

1. Add a new file to your Web API project.
2. In Solution Explorer, right click the file and open its properties window. Change the "Build Action" to "Embedded Resource".

This will embed the file in your assembly and register it with a "Logical Name". This can then be passed to the relevant configuration method. It's based on the Project's default namespace, file location and file extension. For example, given a default namespace of "YourWebApiProject" and a file located at "/SwaggerExtensions/index.html", then the resource will be assigned the name - "YourWebApiProject.SwaggerExtensions.index.html".

## Transitioning to Swashbuckle 5.0 ##

This version of Swashbuckle makes the transition to Swagger 2.0. The 2.0 specification is significantly different to its predecessor (1.2) and forces several breaking changes to Swashbuckle's configuration API. If you're using Swashbuckle without any customizations, i.e. App_Start/SwaggerConfig.cs has never been modified, then you can overwrite it with the new version. The defaults are the same and so the swagger-ui should behave as before.

\* If you have consumers of the raw Swagger document, you should ensure they can accept Swagger 2.0 before making the upgrade.

If you're using the existing configuration API to customize the final Swagger document and/or swagger-ui, you will need to port the code manually. The static __Customize__ methods on SwaggerSpecConfig and SwaggerUiConfig have been replaced with extension methods on HttpConfiguration - __EnableSwagger__ and __EnableSwaggerUi__. All options from version 4.0 are made available through these methods, albeit with slightly different naming and syntax. Refer to the tables below for a summary of changes:


| 4.0 | 5.0 Equivalant | Additional Notes |
| --------------- | --------------- | ---------------- |
| ResolveBasePathUsing | RootUrl | |
| ResolveTargetVersionUsing | N/A | version is now implicit in the docs URL e.g. "swagger/docs/{apiVersion}" |
| ApiVersion | SingleApiVersion| now supports additional metadata for the version | 
| SupportMultipleApiVersions | MultipleApiVersions | now supports additional metadata for each version |
| Authorization | BasicAuth/ApiKey/OAuth2 | | 
| GroupDeclarationsBy | GroupActionsBy | |
| SortDeclarationsBy | OrderActionGroupsBy | |
| MapType | MapType | now accepts Func&lt;Schema&gt; instead of Func&lt;DataType&gt; |
| ModelFilter | SchemaFilter | IModelFilter is now ISchemaFilter, DataTypeRegistry is now SchemaRegistry |
| OperationFilter | OperationFilter | DataTypeRegistry is now SchemaRegistry |
| PolymorphicType | N/A | not currently supported |
| SupportHeaderParams | N/A | header params are implicitly supported |
| SupportedSubmitMethods | N/A | all HTTP verbs are implicitly supported |
| CustomRoute | CustomAsset | &nbsp; |

## Troubleshooting and FAQ's ##

1. [Swagger-ui showing "Can't read swagger JSON from ..."](#swagger-ui-showing-cant-read-swagger-json-from)
2. [Page not found when accessing the UI](#page-not-found-when-accessing-the-ui)
3. [Swagger-ui broken by Visual Studio 2013](#swagger-ui-broken-by-visual-studio-2013)
4. [OWIN Hosted in IIS - Incorrect VirtualPathRoot Handling](#owin-hosted-in-iis---incorrect-virtualpathroot-handling)
5. [How to add vendor extensions](#how-to-add-vendor-extensions)

### Swagger-ui showing "Can't read swagger JSON from ..."

If you see this message, it means the swagger-ui received an unexpected response when requesting the Swagger document. You can troubleshoot further by navigating directly to the discovery URL included in the error message. This should provide more details.

If the discovery URL returns a 404 Not Found response, it may be due to a full-stop in the version name (e.g. "1.0"). This will cause IIS to treat it as a static file (i.e. with an extension) and bypass the URL Routing Module and therefore, Web API. 

To workaround, you can update the version name specified in SwaggerConfig.cs. For example, to "v1", "1-0" etc. Alternatively, you can change the route template being used for the swagger docs (as shown [here](#custom-routes)) so that the version parameter is not at the end of the route.

### Page not found when accessing the UI ###

Swashbuckle serves an embedded version of the swagger-ui through the Web API pipeline. But, most of the URLs contain extensions (.html, .js, .css) and many IIS environments are configured to bypass the managed pipeline for paths containing extensions.

In previous versions of Swashbuckle, this was resolved by adding the following setting to your Web.config:

    <system.webServer>
      <modules runAllManagedModulesForAllRequests="true">
    </modules>

This is no longer neccessary in Swashbuckle 5.0 because it serves the swagger-ui through extensionless URL's.

However, if you're using the SingleApiVersion, MultipleApiVersions or CustomAsset configuration settings you could still get this error. Check to ensure you're not specifying a value that causes a URL with an extension to be referenced in the UI. For example a full-stop in a version number ...

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi();

will result in a discovery URL like this "/swagger/docs/1.0" where the full-stop is treated as a file extension.

### Swagger-ui broken by Visual Studio 2013 ###

VS 2013 ships with a new feature - Browser Link - that improves the web development workflow by setting up a channel between the IDE and pages being previewed in a local browser. It does this by dynamically injecting JavaScript into your files.

Although this JavaScript SHOULD have no affect on your production code, it appears to be breaking the swagger-ui.

I hope to find a permanent fix, but in the meantime, you'll need to workaround this issue by disabling the feature in your web.config:

    <appSettings>
        <add key="vs:EnableBrowserLink" value="false"/>
    </appSettings>< appSettings>

### OWIN Hosted in IIS - Incorrect VirtualPathRoot Handling

When you host Web API 2 on top of OWIN/SystemWeb, Swashbuckle cannot correctly resolve VirtualPathRoot by default.

You must either explicitly set VirtualPathRoot in your HttpConfiguration at startup, or perform customization like this to fix automatic discovery:

    SwaggerSpecConfig.Customize(c =>
    {
        c.ResolveBasePathUsing(req =>
            req.RequestUri.GetLeftPart(UriPartial.Authority) +
            req.GetRequestContext().VirtualPathRoot.TrimEnd('/'));
    }

### How to add vendor extensions ###

Swagger 2.0 allows additional meta-data (aka vendor extensions) to be added at various points in the Swagger document. Swashbuckle supports this by including a "vendorExtensions" dictionary with each of the extensible Swagger types. Meta-data can be added to these dictionaries from custom Schema, Operation or Document filters. For example:

    public class ApplySchemaVendorExtensions : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            schema.vendorExtensions.Add("x-foo", "bar");
        }
    }

As per the specification, all extension properties should be prefixed by "x-
-->
