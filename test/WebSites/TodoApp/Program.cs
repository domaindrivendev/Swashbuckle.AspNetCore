using TodoApp;

var builder = WebApplication.CreateBuilder(args);

builder.AddTodoApp();

var app = builder.Build();

app.UseTodoApp();

app.Run();

namespace TodoApp
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
