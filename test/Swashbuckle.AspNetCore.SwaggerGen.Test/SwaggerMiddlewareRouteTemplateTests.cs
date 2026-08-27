using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NSubstitute;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class SwaggerMiddlewareRouteTemplateTests
{
    [Fact]
    public static void UseSwagger_Does_Not_Require_Regex_Route_Constraint()
    {
        // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2951.
        // AddRoutingCore(), as used by WebApplication.CreateSlimBuilder(), does not
        // register the regex route constraint, so building the pipeline must not
        // require it for the default route template.
        var (pipeline, _) = CreatePipeline();
        Assert.NotNull(pipeline);
    }

    [Theory]
    [InlineData("/swagger/v1/swagger.json", 200, "application/json;charset=utf-8")]
    [InlineData("/swagger/v1/swagger.yaml", 200, "text/yaml;charset=utf-8")]
    [InlineData("/swagger/v1/swagger.yml", 200, "text/yaml;charset=utf-8")]
    [InlineData("/swagger/v1/swagger.JSON", 200, "application/json;charset=utf-8")]
    [InlineData("/swagger/v1/swagger.YAML", 200, "application/json;charset=utf-8")]
    [InlineData("/swagger/v1/swagger.txt", 404, null)]
    [InlineData("/swagger/v1/swagger.html", 404, null)]
    [InlineData("/swagger/v1/swagger.jsonx", 404, null)]
    public static async Task UseSwagger_Default_Route_Template_Only_Matches_Supported_Extensions(
        string path,
        int expectedStatusCode,
        string expectedContentType)
    {
        var (pipeline, services) = CreatePipeline();

        using var scope = services.CreateScope();

        var context = new DefaultHttpContext()
        {
            RequestServices = scope.ServiceProvider,
        };

        context.Request.Method = HttpMethods.Get;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        await pipeline(context);

        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        Assert.Equal(expectedContentType, context.Response.ContentType);
    }

    private static (RequestDelegate Pipeline, IServiceProvider Services) CreatePipeline()
    {
        var swaggerProvider = Substitute.For<ISwaggerProvider>();
        swaggerProvider
            .GetSwagger("v1", Arg.Any<string>(), Arg.Any<string>())
            .Returns(new OpenApiDocument()
            {
                Info = new OpenApiInfo() { Title = "Test API", Version = "v1" },
            });
        var services = new ServiceCollection()
            .AddRoutingCore()
            .AddSingleton(swaggerProvider)
            .BuildServiceProvider();

        var app = new ApplicationBuilder(services);
        app.UseSwagger(new SwaggerOptions());

        return (app.Build(), services);
    }
}

