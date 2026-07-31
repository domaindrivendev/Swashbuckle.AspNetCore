namespace Basic;

public class StartupWithRelativeRoutePrefix : StartupBase
{
    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        base.Configure(app, env);
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "rel"; // serve the UI under a relative prefix
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
        });
    }
}
