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
    }
}
