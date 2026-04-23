using System.Security.Claims;
using Authorization.Swagger;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("readAccess", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Reader", "Administrator"));

    options.AddPolicy("writeAccess", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Administrator"));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Authorization API",
        Version = "v1"
    });

    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.OperationAsyncFilter<SecurityRequirementsAsyncOperationFilter>();
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();
app.MapSwagger("swagger/{documentName}/swagger.json");

app.Run();

namespace Authorization
{
    public partial class Program
    {
    }
}
