namespace TopLevelSwaggerDoc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMvcCore().AddApiExplorer();
        builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Test API", Version = "1" }));

        var app = builder.Build();

        app.UseSwagger(c => c.RouteTemplate = c.RouteTemplate.Replace("swagger/{documentName}/swagger.", "swagger/{documentName}."));
        app.UseSwaggerUI(c => c.SwaggerEndpoint("v1.json", "Test API"));

        app.Run();
    }
}
