namespace TodoApp.Models;

/// <summary>
/// Represents a Todo item.
/// </summary>
public class TodoItemModel
{
    /// <summary>
    /// Gets or sets the ID of the Todo item.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets the text of the Todo item.
    /// </summary>
    public string Text { get; set; } = default!;

    /// <summary>
    /// Gets or sets the date and time the item was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time the item was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time the Todo item was last updated.
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; } = default!;
}
