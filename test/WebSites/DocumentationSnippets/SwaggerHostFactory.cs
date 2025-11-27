namespace DocumentationSnippets;

// begin-snippet: SwaggerHostFactory
public class SwaggerHostFactory
{
    public static IHost CreateHost()
        => MyApplication.CreateHostBuilder([]).Build();
}
// end-snippet

public static class MyApplication
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return WebApplication.CreateBuilder(args).Host;
    }
}
