# Configuration and Customization of `Swashbuckle.AspNetCore.SwaggerUI`

## Change Relative Path to the UI

By default, the Swagger UI will be exposed at `/swagger`. If necessary, you can alter this when enabling the SwaggerUI middleware:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-CustomPath -->
<a id='snippet-SwaggerUI-CustomPath'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api-docs";
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L34-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-CustomPath' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Change Document Title

By default, the Swagger UI will have a generic document title. When you have multiple OpenAPI documents open, it can be difficult to
tell them apart. You can alter this when enabling the SwaggerUI middleware:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-CustomDocumentTitle -->
<a id='snippet-SwaggerUI-CustomDocumentTitle'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "My Swagger UI";
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L41-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-CustomDocumentTitle' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Change CSS or JS Paths

By default, the Swagger UI includes default CSS and JavaScript, but if you wish to change the path or URL (for example to use a CDN)
you can override the defaults as shown below:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-CustomAssets -->
<a id='snippet-SwaggerUI-CustomAssets'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.StylesPath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.29.1/swagger-ui.min.css";
    options.ScriptBundlePath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.29.1/swagger-ui-bundle.min.js";
    options.ScriptPresetsPath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.29.1/swagger-ui-standalone-preset.min.js";
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L48-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-CustomAssets' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## List Multiple OpenAPI Documents

When enabling the middleware, you're required to specify one or more OpenAPI endpoints (fully qualified or relative to the UI page) to
power the UI. If you provide multiple endpoints, they'll be listed in the top right corner of the page, allowing users to toggle between
the different documents. For example, the following configuration could be used to document different versions of an API.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-MultipleDocuments -->
<a id='snippet-SwaggerUI-MultipleDocuments'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L57-L63' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-MultipleDocuments' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Apply swagger-ui Parameters

[swagger-ui][swagger-ui] ships with its own set of configuration parameters, all described
[by the swagger-ui Configuration](https://github.com/swagger-api/swagger-ui/blob/HEAD/docs/usage/configuration.md#display).
In Swashbuckle.AspNetCore, most of these are surfaced through the SwaggerUI middleware options:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-CustomOptions -->
<a id='snippet-SwaggerUI-CustomOptions'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.DefaultModelExpandDepth(2);
    options.DefaultModelRendering(ModelRendering.Model);
    options.DefaultModelsExpandDepth(-1);
    options.DisplayOperationId();
    options.DisplayRequestDuration();
    options.DocExpansion(DocExpansion.None);
    options.EnableDeepLinking();
    options.EnableFilter();
    options.EnablePersistAuthorization();
    options.EnableTryItOutByDefault();
    options.MaxDisplayedTags(5);
    options.ShowExtensions();
    options.ShowCommonExtensions();
    options.EnableValidator();
    options.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Head);
    options.UseRequestInterceptor("(request) => { return request; }");
    options.UseResponseInterceptor("(response) => { return response; }");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L65-L86' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-CustomOptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Inject Custom JavaScript

To tweak the behavior, you can inject additional JavaScript files by adding them to your `wwwroot` folder and specifying
the relative paths in the middleware options:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-JavaScript -->
<a id='snippet-SwaggerUI-JavaScript'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.InjectJavascript("/swagger-ui/custom.js");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L88-L93' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-JavaScript' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Inject Custom CSS

To tweak the look and feel, you can inject additional CSS stylesheets by adding them to your `wwwroot` folder and specifying the
relative paths in the middleware options:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-CSS -->
<a id='snippet-SwaggerUI-CSS'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.InjectStylesheet("/swagger-ui/custom.css");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L95-L100' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-CSS' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Customize index.html

To customize the UI beyond the basic options listed above, you can provide your own version of the swagger-ui `index.html` page:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-CustomIndexHtml -->
<a id='snippet-SwaggerUI-CustomIndexHtml'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.IndexStream = () => typeof(Program).Assembly
        .GetManifestResourceStream("CustomUIIndex.Swagger.index.html"); // Requires file to be added as an embedded resource
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L102-L108' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-CustomIndexHtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

```xml
<Project>
  <ItemGroup>
    <EmbeddedResource Include="CustomUIIndex.Swagger.index.html" />
  </ItemGroup>
</Project>
```

> [!TIP]
> To get started, you should base your custom `index.html` on the [built-in version](../src/Swashbuckle.AspNetCore.SwaggerUI/index.html).

## Enable OAuth2.0 Flows

[swagger-ui][swagger-ui] has built-in support to participate in OAuth2.0 authorization flows. It interacts with authorization and/or token
endpoints, as specified in the OpenAPI JSON, to obtain access tokens for subsequent API calls. See
[Adding Security Definitions and Requirements](configure-and-customize-swaggergen.md#add-security-definitions-and-requirements) for
an example of adding OAuth2.0 metadata to the generated Swagger.

If your OpenAPI endpoint includes the appropriate security metadata, the UI interaction should be automatically enabled. However, you
can further customize OAuth support in the UI with the following settings below. See the
[Swagger-UI documentation](https://github.com/swagger-api/swagger-ui/blob/HEAD/docs/usage/oauth2.md) for more information.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-OAuth2 -->
<a id='snippet-SwaggerUI-OAuth2'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.OAuthClientId("test-id");
    options.OAuthClientSecret("test-secret");
    options.OAuthUsername("test-user");
    options.OAuthRealm("test-realm");
    options.OAuthAppName("test-app");
    options.OAuth2RedirectUrl("url");
    options.OAuthScopeSeparator(" ");
    options.OAuthScopes("scope1", "scope2");
    options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { ["foo"] = "bar" });
    options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    options.OAuthUsePkce();
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L110-L125' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-OAuth2' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Use client-side request and response interceptors

To use custom interceptors on requests and responses going through swagger-ui you can define them as JavaScript functions
in the configuration:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-Interceptors -->
<a id='snippet-SwaggerUI-Interceptors'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.UseRequestInterceptor("(req) => { req.headers['x-my-custom-header'] = 'MyCustomValue'; return req; }");
    options.UseResponseInterceptor("(res) => { console.log('Custom interceptor intercepted response from:', res.url); return res; }");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L127-L133' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-Interceptors' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

This can be useful in a range of scenarios where you might want to append local XSRF tokens to all requests, for example:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: SwaggerUI-Interceptor-XSRF -->
<a id='snippet-SwaggerUI-Interceptor-XSRF'></a>
```cs
app.UseSwaggerUI(options =>
{
    options.UseRequestInterceptor("(req) => { req.headers['X-XSRF-Token'] = localStorage.getItem('xsrf-token'); return req; }");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L135-L140' title='Snippet source file'>snippet source</a> | <a href='#snippet-SwaggerUI-Interceptor-XSRF' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

[swagger-ui]: https://github.com/swagger-api/swagger-ui
