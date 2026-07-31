namespace Basic;

public class StartupWithAbsoluteRoutePrefix : StartupBase
{
    public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        base.Configure(app, env);
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "/abs"; // serve the UI under an absolute prefix
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
        });
    }
}
