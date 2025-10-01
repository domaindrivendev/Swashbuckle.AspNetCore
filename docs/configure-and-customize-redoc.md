# Configuration and Customization of `Swashbuckle.AspNetCore.ReDoc`

## Change Relative Path to the UI

By default, the Redoc UI will be exposed at `/api-docs`. If necessary, you can alter this when enabling the Redoc middleware:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-RoutePrefix -->
<a id='snippet-Redoc-RoutePrefix'></a>
```cs
app.UseReDoc(options =>
{
    options.RoutePrefix = "docs";
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L143-L148' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-RoutePrefix' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Change Document Title

By default, the Redoc UI will have a generic document title. You can alter this when enabling the Redoc middleware:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-DocumentTitle -->
<a id='snippet-Redoc-DocumentTitle'></a>
```cs
app.UseReDoc(options =>
{
    options.DocumentTitle = "My API Docs";
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L150-L155' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-DocumentTitle' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Apply Redoc Parameters

Redoc ships with its own set of configuration parameters, all described [in the Redoc documentation][redoc-options].
In Swashbuckle.AspNetCore, most of these are surfaced through the Redoc middleware options:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-CustomOptions -->
<a id='snippet-Redoc-CustomOptions'></a>
```cs
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
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L157-L175' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-CustomOptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

> [!NOTE]
> Using `options.SpecUrl("/v1/swagger.json")` multiple times within the same `UseReDoc(...)` will not add multiple URLs.

## Inject Custom CSS

To tweak the look and feel, you can inject additional CSS stylesheets by adding them to your `wwwroot` folder and specifying
the relative paths in the middleware options:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-CustomCSS -->
<a id='snippet-Redoc-CustomCSS'></a>
```cs
app.UseReDoc(options =>
{
    options.InjectStylesheet("/redoc/custom.css");
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L177-L182' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-CustomCSS' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

It is also possible to modify the theme by using the `AdditionalItems` property. More information can be found
[in the Redoc documentation][redoc-options].

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-ModifyTheme -->
<a id='snippet-Redoc-ModifyTheme'></a>
```cs
app.UseReDoc(options =>
{
    options.ConfigObject.AdditionalItems = new Dictionary<string, object>
    {
        // Configured additional options
    };
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L184-L192' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-ModifyTheme' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Customize index.html

To customize the UI beyond the basic options listed above, you can provide your own version of the Redoc `index.html` page:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-CustomIndexHtml -->
<a id='snippet-Redoc-CustomIndexHtml'></a>
```cs
app.UseReDoc(options =>
{
    options.IndexStream = () => typeof(Program).Assembly
        .GetManifestResourceStream("CustomIndex.ReDoc.index.html"); // Requires file to be added as an embedded resource
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L194-L200' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-CustomIndexHtml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

```xml
<Project>
  <ItemGroup>
    <EmbeddedResource Include="CustomIndex.ReDoc.index.html" />
  </ItemGroup>
</Project>
```

> [!TIP]
> To get started, you should base your custom `index.html` on the [default version](../src/Swashbuckle.AspNetCore.ReDoc/index.html).

[redoc-options]: https://github.com/Redocly/redoc/blob/main/docs/deployment/html.md#the-redoc-object
