using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

var documentFilter = new DocumentFilter();
var operationFilter = new OperationFilter();
for (int i = 0; i < 1; i++)
{
    var x = i;
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc($"title-{x}", new OpenApiInfo { Title = $"SMARTR ({i})", Version = "1.0" });
        c.AddDocumentFilterInstance(documentFilter);
        c.AddOperationFilterInstance(operationFilter);
    });
}

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapGet("/weatherforecast", () => 0).WithName("GetWeatherForecast").WithOpenApi();
app.Run();

public class DocumentFilter : IDocumentFilter
{
    static int count;
    public DocumentFilter() { Thread.Sleep(20); Console.WriteLine("DocumentFilter " + count++); }
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) { }
}
public class OperationFilter : IOperationFilter
{
    static int count;
    public OperationFilter() { Thread.Sleep(20); Console.WriteLine("OperationFilter " + count++); }
    public void Apply(OpenApiOperation operation, OperationFilterContext context) { }
}
