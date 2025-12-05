namespace TodoApp.Models;

/// <summary>
/// Represents the model for creating a new Todo item.
/// </summary>
public class CreateTodoItemModel
{
    /// <summary>
    /// Gets or sets the text of the Todo item.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
