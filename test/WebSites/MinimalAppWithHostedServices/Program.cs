var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MinimalApp", Version = "v1" });
});

builder.Services.AddHostedService<HostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MinimalApp v1"));
}

app.MapGet("/ShouldContain", () => "Hello World!");

app.Run();

class HostedService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // This is intentional. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/3151#discussion_r1856678972
        throw new Exception("Crash!");
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

}
