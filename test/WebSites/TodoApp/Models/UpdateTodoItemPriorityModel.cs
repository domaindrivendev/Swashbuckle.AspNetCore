namespace TodoApp.Models;

/// <summary>
/// Represents the model for updating the priority of a Todo item.
/// </summary>
public class UpdateTodoItemPriorityModel
{
    /// <summary>
    /// Gets or sets the new priority of the Todo item.
    /// </summary>
    public TodoPriority Priority { get; set; }
}
