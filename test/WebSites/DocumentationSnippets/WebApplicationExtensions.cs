using Swashbuckle.AspNetCore.SwaggerUI;

namespace DocumentationSnippets;

public static class WebApplicationExtensions
{
    public static void Configure(WebApplication app)
    {
        // begin-snippet: README-MapSwagger
        // Your own endpoints go here, and then...
        app.MapSwagger();
        // end-snippet

        // begin-snippet: README-UseSwagger
        app.UseSwagger();
        // end-snippet

        // begin-snippet: README-UseSwaggerUI
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", "My API V1");
        });
        // end-snippet

        // begin-snippet: README-MvcConventionalRouting
        app.UseMvc(routes =>
        {
            // SwaggerGen won't find controllers that are routed via this technique.
            routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
        });
        // end-snippet

        // begin-snippet: SwaggerUI-CustomPath
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "api-docs";
        });
        // end-snippet

        // begin-snippet: SwaggerUI-CustomDocumentTitle
        app.UseSwaggerUI(options =>
        {
            options.DocumentTitle = "My Swagger UI";
        });
        // end-snippet

        // begin-snippet: SwaggerUI-CustomAssets
        app.UseSwaggerUI(options =>
        {
            options.StylesPath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.29.1/swagger-ui.min.css";
            options.ScriptBundlePath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.29.1/swagger-ui-bundle.min.js";
            options.ScriptPresetsPath = "https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.29.1/swagger-ui-standalone-preset.min.js";
        });
        // end-snippet

        // begin-snippet: SwaggerUI-MultipleDocuments
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
        });
        // end-snippet

        // begin-snippet: SwaggerUI-CustomOptions
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
        // end-snippet

        // begin-snippet: SwaggerUI-JavaScript
        app.UseSwaggerUI(options =>
        {
            options.InjectJavascript("/swagger-ui/custom.js");
        });
        // end-snippet

        // begin-snippet: SwaggerUI-CSS
        app.UseSwaggerUI(options =>
        {
            options.InjectStylesheet("/swagger-ui/custom.css");
        });
        // end-snippet

        // begin-snippet: SwaggerUI-CustomIndexHtml
        app.UseSwaggerUI(options =>
        {
            options.IndexStream = () => typeof(Program).Assembly
                .GetManifestResourceStream("CustomUIIndex.Swagger.index.html"); // Requires file to be added as an embedded resource
        });
        // end-snippet

        // begin-snippet: SwaggerUI-OAuth2
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
        // end-snippet

        // begin-snippet: SwaggerUI-Interceptors
        app.UseSwaggerUI(options =>
        {
            options.UseRequestInterceptor("(req) => { req.headers['x-my-custom-header'] = 'MyCustomValue'; return req; }");
            options.UseResponseInterceptor("(res) => { console.log('Custom interceptor intercepted response from:', res.url); return res; }");
        });
        // end-snippet

        // begin-snippet: SwaggerUI-Interceptor-XSRF
        app.UseSwaggerUI(options =>
        {
            options.UseRequestInterceptor("(req) => { req.headers['X-XSRF-Token'] = localStorage.getItem('xsrf-token'); return req; }");
        });
        // end-snippet

        // begin-snippet: Redoc-RoutePrefix
        app.UseReDoc(options =>
        {
            options.RoutePrefix = "docs";
        });
        // end-snippet

        // begin-snippet: Redoc-DocumentTitle
        app.UseReDoc(options =>
        {
            options.DocumentTitle = "My API Docs";
        });
        // end-snippet

        // begin-snippet: Redoc-CustomOptions
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
        // end-snippet

        // begin-snippet: Redoc-CustomCSS
        app.UseReDoc(options =>
        {
            options.InjectStylesheet("/redoc/custom.css");
        });
        // end-snippet

        // begin-snippet: Redoc-ModifyTheme
        app.UseReDoc(options =>
        {
            options.ConfigObject.AdditionalItems = new Dictionary<string, object>
            {
                // Configured additional options
            };
        });
        // end-snippet

        // begin-snippet: Redoc-CustomIndexHtml
        app.UseReDoc(options =>
        {
            options.IndexStream = () => typeof(Program).Assembly
                .GetManifestResourceStream("CustomIndex.ReDoc.index.html"); // Requires file to be added as an embedded resource
        });
        // end-snippet
    }
}
