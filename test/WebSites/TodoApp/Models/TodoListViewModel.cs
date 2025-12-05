namespace TodoApp.Models;

/// <summary>
/// Represents a collection of Todo items.
/// </summary>
public class TodoListViewModel
{
    /// <summary>
    /// Gets or sets the Todo item(s).
    /// </summary>
    public ICollection<TodoItemModel> Items { get; set; } = [];
}
