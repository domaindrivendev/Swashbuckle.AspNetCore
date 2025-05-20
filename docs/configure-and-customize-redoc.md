# Configuration & Customization of `Swashbuckle.AspNetCore.ReDoc`

## Change Relative Path to the UI

By default, the Redoc UI will be exposed at "/api-docs". If necessary, you can alter this when enabling the Redoc middleware:

```csharp
app.UseReDoc(c =>
{
    c.RoutePrefix = "docs"
    ...
}
```

## Change Document Title

By default, the Redoc UI will have a generic document title. You can alter this when enabling the Redoc middleware:

```csharp
app.UseReDoc(c =>
{
    c.DocumentTitle = "My API Docs";
    ...
}
```

## Apply Redoc Parameters

Redoc ships with its own set of configuration parameters, all described here https://github.com/Rebilly/redoc/blob/main/README.md#redoc-options-object. In Swashbuckle, most of these are surfaced through the Redoc middleware options:

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

## Inject Custom CSS

To tweak the look and feel, you can inject additional CSS stylesheets by adding them to your `wwwroot` folder and specifying the relative paths in the middleware options:

```csharp
app.UseReDoc(c =>
{
    ...
    c.InjectStylesheet("/redoc/custom.css");
}
```

It is also possible to modify the theme by using the `AdditionalItems` property, see https://github.com/Rebilly/redoc/blob/main/README.md#redoc-options-object for more information.

```csharp
app.UseReDoc(c =>
{
    ...
    c.ConfigObject.AdditionalItems = ...
}
```

## Customize index.html

To customize the UI beyond the basic options listed above, you can provide your own version of the Redoc index.html page:

```csharp
app.UseReDoc(c =>
{
    c.IndexStream = () => GetType().Assembly
        .GetManifestResourceStream("CustomIndex.ReDoc.index.html"); // requires file to be added as an embedded resource
});
```

_To get started, you should base your custom index.html on the [default version](src/Swashbuckle.AspNetCore.ReDoc/index.html)_
