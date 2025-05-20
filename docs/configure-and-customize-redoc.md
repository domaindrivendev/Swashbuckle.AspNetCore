# Configuration & Customization of `Swashbuckle.AspNetCore.ReDoc`

## Change Relative Path to the UI

By default, the Redoc UI will be exposed at `"/api-docs"`. If necessary, you can alter this when enabling the Redoc middleware:

```csharp
app.UseReDoc(options =>
{
    options.RoutePrefix = "docs"
    //...
});
```

## Change Document Title

By default, the Redoc UI will have a generic document title. You can alter this when enabling the Redoc middleware:

```csharp
app.UseReDoc(options =>
{
    options.DocumentTitle = "My API Docs";
    //...
});
```

## Apply Redoc Parameters

Redoc ships with its own set of configuration parameters, all described [here](https://github.com/Rebilly/redoc/blob/main/README.md#redoc-options-object). In Swashbuckle, most of these are surfaced through the Redoc middleware options:

```csharp
app.UseReDoc(options =>
{
    options.SpecUrl("/v1/swagger.json");
    options.EnableUntrustedSpec();
    options.ScrollYOffset(10);
    options.HideHostname();
    options.HideDownloadButton();
    options.ExpandResponses("200,201");
    options.RequiredPropsFirst();
    options.NoAutoAuth();
    options.PathInMiddlePanel();
    options.HideLoading();
    options.NativeScrollbars();
    options.DisableSearch();
    options.OnlyRequiredInSamples();
    options.SortPropsAlphabetically();
});
```

> [!NOTE]
> Using `options.SpecUrl("/v1/swagger.json")` multiple times within the same `UseReDoc(...)` will not add multiple URL.

## Inject Custom CSS

To tweak the look and feel, you can inject additional CSS stylesheets by adding them to your `wwwroot` folder and specifying the relative paths in the middleware options:

```csharp
app.UseReDoc(options =>
{
    //...
    options.InjectStylesheet("/redoc/custom.css");
});
```

It is also possible to modify the theme by using the `AdditionalItems` property, more information can be found [here](https://github.com/Rebilly/redoc/blob/main/README.md#redoc-options-object).

```csharp
app.UseReDoc(options =>
{
    //...
    options.ConfigObject.AdditionalItems = ...
});
```

## Customize index.html

To customize the UI beyond the basic options listed above, you can provide your own version of the Redoc `index.html` page:

```csharp
app.UseReDoc(options =>
{
    options.IndexStream = () => GetType().Assembly
        .GetManifestResourceStream("CustomIndex.ReDoc.index.html"); // requires file to be added as an embedded resource
});
```

> [!TIP]
> To get started, you should base your custom `index.html` on the [default version](../src/Swashbuckle.AspNetCore.ReDoc/index.html).
