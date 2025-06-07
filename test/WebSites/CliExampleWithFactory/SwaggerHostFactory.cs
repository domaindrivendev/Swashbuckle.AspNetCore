namespace CliExampleWithFactory;

public class SwaggerHostFactory
{
    public static IHost CreateHost()
        => Program.CreateHostBuilder([]).Build();
}
