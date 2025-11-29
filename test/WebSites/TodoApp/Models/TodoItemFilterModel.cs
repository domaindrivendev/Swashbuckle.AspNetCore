namespace TodoApp.Models;

/// <summary>
/// Represents the model for searching for Todo items.
/// </summary>
public sealed class TodoItemFilterModel
{
    /// <summary>
    /// Gets or sets the text of the filter.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to search completed Todo items.
    /// </summary>
    public bool IsCompleted { get; set; }
}
