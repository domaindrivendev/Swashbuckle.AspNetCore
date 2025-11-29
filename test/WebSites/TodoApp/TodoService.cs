using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp;

public class TodoService(TodoRepository repository)
{
    public async Task<string> AddItemAsync(string text, CancellationToken cancellationToken)
    {
        var item = await repository.AddItemAsync(text, cancellationToken);
        return item.Id.ToString();
    }

    public async Task<bool?> CompleteItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return await repository.CompleteItemAsync(itemId, cancellationToken);
    }

    public async Task<bool> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        return await repository.DeleteItemAsync(itemId, cancellationToken);
    }

    public async Task<TodoItemModel?> GetAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await repository.GetItemAsync(itemId, cancellationToken);
        return item is null ? null : MapItem(item);
    }

    public async Task<TodoListViewModel> GetListAsync(CancellationToken cancellationToken)
    {
        var items = await repository.GetItemsAsync(cancellationToken);
        return MapItems(items);
    }

    public async Task<TodoListViewModel> FindAsync(TodoItemFilterModel filter, CancellationToken cancellationToken)
    {
        var items = await repository.FindAsync(filter.Text, filter.IsCompleted, cancellationToken);
        return MapItems(items);
    }

    public async Task<TodoListViewModel> GetAfterDateAsync(DateTime value, CancellationToken cancellationToken)
    {
        var items = await repository.GetAfterDateAsync(value, cancellationToken);
        return MapItems(items);
    }

    private static TodoListViewModel MapItems(IList<TodoItem> items)
    {
        var result = new List<TodoItemModel>(items.Count);

        foreach (var todo in items)
        {
            result.Add(MapItem(todo));
        }

        return new() { Items = result };
    }

    private static TodoItemModel MapItem(TodoItem item)
    {
        return new()
        {
            Id = item.Id.ToString(),
            CompletedAt = item.CompletedAt,
            CreatedAt = item.CreatedAt,
            LastUpdated = item.CompletedAt ?? item.CreatedAt,
            Text = item.Text,
        };
    }
}
