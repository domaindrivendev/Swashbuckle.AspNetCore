using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp;

public static class ApiEndpoints
{
    public static IServiceCollection AddTodoApi(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<TodoRepository>();
        services.AddScoped<TodoService>();

        services.AddDbContext<TodoContext>((options) => options.UseInMemoryDatabase("Todos"));

        services.ConfigureHttpJsonOptions((options) =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = TodoJsonSerializerContext.Default.Options.DefaultIgnoreCondition;
            options.SerializerOptions.NumberHandling = TodoJsonSerializerContext.Default.Options.NumberHandling;
            options.SerializerOptions.PropertyNamingPolicy = TodoJsonSerializerContext.Default.Options.PropertyNamingPolicy;
            options.SerializerOptions.WriteIndented = TodoJsonSerializerContext.Default.Options.WriteIndented;
            options.SerializerOptions.TypeInfoResolverChain.Add(TodoJsonSerializerContext.Default);
        });

        return services;
    }

    public static IEndpointRouteBuilder MapTodoApiRoutes(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/items")
                           .WithTags("TodoApp");
        {
            group.MapGet("/", async (TodoService service, CancellationToken cancellationToken) => await service.GetListAsync(cancellationToken))
                 .WithName("ListTodos")
                 .WithSummary("Get all Todo items")
                 .WithDescription("Gets all of the current user's todo items.")
                 .Produces<TodoListViewModel>(StatusCodes.Status200OK);

            group.MapGet(
                "/{id}",
                async (
                    [Description("The Todo item's ID.")] Guid id,
                    TodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var model = await service.GetAsync(id, cancellationToken);
                    return model switch
                    {
                        null => Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                        _ => Results.Ok(model),
                    };
                })
                .WithName("GetTodo")
                .WithSummary("Get a specific Todo item")
                .WithDescription("Gets the todo item with the specified ID.")
                .Produces<TodoItemModel>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapPost(
                "/",
                async (
                    [Description("The Todo item to create.")] CreateTodoItemModel model,
                    TodoService service,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(model.Text))
                    {
                        return Results.Problem("No item text specified.", statusCode: StatusCodes.Status400BadRequest);
                    }

                    var id = await service.AddItemAsync(model.Text, cancellationToken);

                    return Results.Created($"/api/items/{id}", new CreatedTodoItemModel() { Id = id });
                })
                .WithName("CreateTodo")
                .WithSummary("Create a new Todo item")
                .WithDescription("Creates a new todo item for the current user and returns its ID.")
                .Produces<CreatedTodoItemModel>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest);

            group.MapPost(
                "/{id}/complete",
                async (
                    [Description("The Todo item's ID.")] Guid id,
                    TodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var wasCompleted = await service.CompleteItemAsync(id, cancellationToken);

                    return wasCompleted switch
                    {
                        true => Results.NoContent(),
                        false => Results.Problem("Item already completed.", statusCode: StatusCodes.Status400BadRequest),
                        _ => Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .WithName("CompleteTodo")
                .WithSummary("Mark a Todo item as completed")
                .WithDescription("Marks the todo item with the specified ID as complete.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id}",
                async (
                    [Description("The Todo item's ID.")] Guid id,
                    TodoService service,
                    CancellationToken cancellationToken) =>
                {
                    var wasDeleted = await service.DeleteItemAsync(id, cancellationToken);
                    return wasDeleted switch
                    {
                        true => Results.NoContent(),
                        false => Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .WithName("DeleteTodo")
                .WithSummary("Delete a Todo item")
                .WithDescription("Deletes the todo item with the specified ID.")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound);

            group.MapGet("/find", FindTodoItem)
                 .WithName("FindTodo")
                 .Produces<TodoListViewModel>();

            group.MapGet("/getAfter", GetAfterDate)
                 .WithName("GetAfterDate")
                 .Produces<TodoListViewModel>();
        }

        builder.MapGet("/", () => Results.Redirect("/swagger"))
               .ExcludeFromDescription();

        return builder;
    }

    private static async Task<TodoListViewModel> FindTodoItem(
        [AsParameters] TodoItemFilterModel filter,
        TodoService service,
        CancellationToken cancellationToken) =>
        await service.FindAsync(filter, cancellationToken);

    private static async Task<TodoListViewModel> GetAfterDate(
        DateTime value,
        TodoService service,
        CancellationToken cancellationToken) =>
        await service.GetAfterDateAsync(value, cancellationToken);
}
