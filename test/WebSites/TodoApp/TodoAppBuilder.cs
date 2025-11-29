using System.Reflection;
using Microsoft.AspNetCore.WebUtilities;

namespace TodoApp;

public static class TodoAppBuilder
{
    public static WebApplicationBuilder AddTodoApp(this WebApplicationBuilder builder)
    {
        builder.Services.AddTodoApi();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen((options) =>
        {
            options.EnableAnnotations();
            options.IncludeXmlComments(Assembly.GetExecutingAssembly());
            options.SupportNonNullableReferenceTypes();

            var version = "v1";
            options.SwaggerDoc(version, new() { Title = "Todo API", Version = version });
        });

        if (!builder.Environment.IsDevelopment())
        {
            builder.Services.AddProblemDetails();
            builder.Services.Configure<ProblemDetailsOptions>((options) =>
            {
                options.CustomizeProblemDetails = (context) =>
                {
                    if (context.Exception is not null)
                    {
                        context.ProblemDetails.Detail = "An internal error occurred.";
                    }

                    context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                    context.ProblemDetails.Title = ReasonPhrases.GetReasonPhrase(context.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError);
                };
            });
        }

        return builder;
    }

    public static WebApplication UseTodoApp(this WebApplication app)
    {
        app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

        app.MapTodoApiRoutes();

        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}
