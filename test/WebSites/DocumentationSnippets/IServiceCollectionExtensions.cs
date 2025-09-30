using Microsoft.OpenApi;

namespace DocumentationSnippets;

public static class IServiceCollectionExtensions
{
    public static void Configure(this IServiceCollection services)
    {
        // begin-snippet: README-Newtonsoft.Json
        services.AddMvc();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });

        services.AddSwaggerGenNewtonsoftSupport();
        // end-snippet

        // begin-snippet: README-MvcCore
        services.AddMvcCore()
                .AddApiExplorer();
        // end-snippet

        // begin-snippet: Annotations-Enable
        services.AddSwaggerGen(options =>
        {
            // Other setup, then...
            options.EnableAnnotations();
        });
        // end-snippet

        // begin-snippet: Annotations-EnablePolymorphism
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
        });
        // end-snippet
    }
}
