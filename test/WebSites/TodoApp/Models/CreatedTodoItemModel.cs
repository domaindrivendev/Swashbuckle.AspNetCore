namespace TodoApp.Models;

/// <summary>
/// Represents the model for a created Todo item. This class cannot be inherited.
/// </summary>
public class CreatedTodoItemModel
{
    /// <summary>
    /// Gets or sets the ID of the created Todo item.
    /// </summary>
    public string Id { get; set; } = string.Empty;
}
