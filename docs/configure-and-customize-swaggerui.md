# Configuration & Customization of `Swashbuckle.AspNetCore.SwaggerUI`

## Change Relative Path to the UI

By default, the Swagger UI will be exposed at `"/swagger"`. If necessary, you can alter this when enabling the SwaggerUI middleware:

```csharp
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api-docs"
}
```

## Change Document Title

By default, the Swagger UI will have a generic document title. When you have multiple Swagger pages open, it can be difficult to tell them apart. You can alter this when enabling the SwaggerUI middleware:

```csharp
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "My Swagger UI";
}
```

## Change CSS or JS Paths

By default, the Swagger UI include default CSS and JS, but if you wish to change the path or URL (for example to use a CDN):

```csharp
app.UseSwaggerUI(options =>
{
    options.StylesPath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.10/swagger-ui.min.css";
    options.ScriptBundlePath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.10/swagger-ui-bundle.min.js";
    options.ScriptPresetsPath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.17.10/swagger-ui-standalone-preset.min.js";
}
```

## List Multiple Swagger Documents

When enabling the middleware, you're required to specify one or more Swagger endpoints (fully qualified or relative to the UI page) to power the UI. If you provide multiple endpoints, they'll be listed in the top right corner of the page, allowing users to toggle between the different documents. For example, the following configuration could be used to document different versions of an API.

```csharp
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
}
```

## Apply swagger-ui Parameters

The swagger-ui ships with its own set of configuration parameters, all described [here](https://github.com/swagger-api/swagger-ui/blob/master/docs/usage/configuration.md#display). In Swashbuckle, most of these are surfaced through the SwaggerUI middleware options:

```csharp
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
    options.Plugins = ["myCustomPlugin"];
    options.EnableValidator();
    options.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Head);
    options.UseRequestInterceptor("(request) => { return request; }");
    options.UseResponseInterceptor("(response) => { return response; }");
});
```

 > [!NOTE]
 > When adding custom plugins, make sure you add any custom `js` files that define the plugin function(s).

## Inject Custom JavaScript

To tweak the behavior, you can inject additional JavaScript files by adding them to your `wwwroot` folder and specifying the relative paths in the middleware options:

```csharp
app.UseSwaggerUI(options =>
{
    options.InjectJavascript("/swagger-ui/custom.js");
}
```

> [!NOTE] 
> The `InjectOnCompleteJavaScript` and `InjectOnFailureJavaScript` options have been removed because the latest version of swagger-ui doesn't expose the necessary hooks. Instead, it provides a [flexible customization system](https://github.com/swagger-api/swagger-ui/blob/master/docs/customization/overview.md) based on concepts and patterns from React and Redux. To leverage this, you'll need to provide a custom version of index.html as described [below](#customize-indexhtml).

> [!TIP]
> The [custom index sample app](../test/WebSites/CustomUIIndex/Swagger/index.html) demonstrates this approach, using the swagger-ui plugin system provide a custom topbar, and to hide the info component.

## Inject Custom CSS

To tweak the look and feel, you can inject additional CSS stylesheets by adding them to your `wwwroot` folder and specifying the relative paths in the middleware options:

```csharp
app.UseSwaggerUI(options =>
{
    options.InjectStylesheet("/swagger-ui/custom.css");
}
```

## Customize index.html

To customize the UI beyond the basic options listed above, you can provide your own version of the swagger-ui index.html page:

```csharp
app.UseSwaggerUI(options =>
{
    options.IndexStream = () => GetType().Assembly
        .GetManifestResourceStream("CustomUIIndex.Swagger.index.html"); // requires file to be added as an embedded resource
});
```

> [!TIP]
> To get started, you should base your custom `index.html` on the [default version](../src/Swashbuckle.AspNetCore.SwaggerUI/index.html)

## Enable OAuth2.0 Flows

The swagger-ui has built-in support to participate in OAuth2.0 authorization flows. It interacts with authorization and/or token endpoints, as specified in the Swagger JSON, to obtain access tokens for subsequent API calls. See [Adding Security Definitions and Requirements](configure-and-customize-swaggergen.md#add-security-definitions-and-requirements) for an example of adding OAuth2.0 metadata to the generated Swagger.

If your Swagger endpoint includes the appropriate security metadata, the UI interaction should be automatically enabled. However, you can further customize OAuth support in the UI with the following settings below. See [Swagger-UI documentation](https://github.com/swagger-api/swagger-ui/blob/master/docs/usage/oauth2.md) for more info:

```csharp
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
    options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "foo", "bar" }}); 
    options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
    options.OAuthUsePkce();
});
```

## Use client-side request and response interceptors

To use custom interceptors on requests and responses going through swagger-ui you can define them as javascript functions in the configuration:

```csharp
app.UseSwaggerUI(options =>
{
    options.UseRequestInterceptor("(req) => { req.headers['x-my-custom-header'] = 'MyCustomValue'; return req; }");
    options.UseResponseInterceptor("(res) => { console.log('Custom interceptor intercepted response from:', res.url); return res; }");
});
```

This can be useful in a range of scenarios where you might want to append local xsrf tokens to all requests for example:

```csharp
app.UseSwaggerUI(options =>
{
    options.UseRequestInterceptor("(req) => { req.headers['X-XSRF-Token'] = localStorage.getItem('xsrf-token'); return req; }");
});
```
