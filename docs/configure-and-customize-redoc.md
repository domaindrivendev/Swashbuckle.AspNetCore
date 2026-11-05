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
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L142-L147' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-RoutePrefix' title='Start of snippet'>anchor</a></sup>
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
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L149-L154' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-DocumentTitle' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

## Apply Redoc Parameters

Redoc ships with its own set of configuration parameters, all described [in the Redoc documentation][redoc-options].
In Swashbuckle.AspNetCore, the most common of these are surfaced through the Redoc middleware options:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-CustomOptions -->
<a id='snippet-Redoc-CustomOptions'></a>
```cs
app.UseReDoc(options =>
{
    options.SpecUrl("/v1/swagger.json");
    options.EnableUntrustedSpec();
    options.ScrollYOffset(10);
    options.HideDownloadButton();
    options.RequiredPropsFirst();
    options.HideLoading();
    options.OnlyRequiredInSamples();
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L156-L167' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-CustomOptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

> [!NOTE]
> Using `options.SpecUrl("/v1/swagger.json")` multiple times within the same `UseReDoc(...)` will not add multiple URLs.

Any other Redoc option, such as `layout`, `router` or `disableTelemetry`, can be set using the `AdditionalItems` property:

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-AdditionalOptions -->
<a id='snippet-Redoc-AdditionalOptions'></a>
```cs
app.UseReDoc(options =>
{
    options.ConfigObject.AdditionalItems = new Dictionary<string, object>
    {
        ["layout"] = "stacked",
        ["disableTelemetry"] = true,
    };
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L176-L185' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-AdditionalOptions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

> [!IMPORTANT]
> Swashbuckle.AspNetCore embeds [Redoc 3][redoc-migration], which removed a number of options that were available in Redoc 2.
> The corresponding members of `ConfigObject` and `ReDocOptions` (`DisableSearch`, `ExpandResponses`, `HideHostname`, `NativeScrollbars`,
> `NoAutoAuth`, `PathInMiddlePanel` and `SortPropsAlphabetically`) have been removed.
> Redoc 3 also collects anonymous usage telemetry by default, which can be turned off with the `disableTelemetry` option shown above.

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
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L169-L174' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-CustomCSS' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

Redoc 3 no longer supports the `theme` option. Instead, the theme is customized with CSS variables, which can be
injected into the page using the `HeadContent` property. The available variables are described
[in the Redoc documentation][redoc-theme].

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-ModifyTheme -->
<a id='snippet-Redoc-ModifyTheme'></a>
```cs
app.UseReDoc(options =>
{
    options.HeadContent =
        """
        <style>
            :root.light, :root.dark {
                --font-family-base: 'Segoe UI', sans-serif;
            }
        </style>
        """;
});
```
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L187-L199' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-ModifyTheme' title='Start of snippet'>anchor</a></sup>
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
<sup><a href='/test/WebSites/DocumentationSnippets/WebApplicationExtensions.cs#L201-L207' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-CustomIndexHtml' title='Start of snippet'>anchor</a></sup>
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

Redoc 3 is distributed as an ES module and no longer provides the global `Redoc` object. A custom `index.html` based on an
earlier version of the default page must load `index.js` with `<script type="module">`, as shown in the default version, rather
than loading `redoc.standalone.js` with a classic `<script>` element and calling `Redoc.init(...)`.

## Use `MapReDoc` with endpoint routing

`MapReDoc` is an endpoint-routing alternative to `UseReDoc`. The options API is the same, so all customization examples on this page also apply when using `MapReDoc`.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-MapReDoc -->
<a id='snippet-Redoc-MapReDoc'></a>
```cs
app.MapReDoc();
```
<sup><a href='/test/WebSites/WebApi.Map/Program.cs#L33-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-MapReDoc' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

The main differences are:

- **Use**ReDoc adds middleware directly to the request pipeline and returns `IApplicationBuilder`.
- **Map**ReDoc maps ReDoc via endpoint routing and returns `IEndpointConventionBuilder`.

- Because `MapReDoc` returns an endpoint convention builder, you can attach endpoint metadata/conventions (for example, authorization policies) directly to the mapped ReDoc endpoint.

<!-- markdownlint-disable MD031 MD033 -->
<!-- snippet: Redoc-MapReDoc-RequireAuthorization -->
<a id='snippet-Redoc-MapReDoc-RequireAuthorization'></a>
```cs
app.MapReDoc("redoc-auth")
   .RequireAuthorization(); // Remember to also add RequireAuthorization to MapSwagger
```
<sup><a href='/test/WebSites/WebApi.Map/Program.cs#L37-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-Redoc-MapReDoc-RequireAuthorization' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
<!-- markdownlint-enable MD031 MD033 -->

[redoc-migration]: https://redocly.com/docs/redoc/config-migration
[redoc-options]: https://redocly.com/docs/redoc/config
[redoc-theme]: https://redocly.com/docs/redoc/deployment/html#customize-the-theme-with-css-variables
