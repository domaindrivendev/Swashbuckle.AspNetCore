using System.Text.Json.Serialization;

namespace TodoApp.Models;

/// <summary>
/// The priority levels for a Todo item.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TodoPriority>))]
public enum TodoPriority
{
    /// <summary>
    /// The item is of normal priority.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// The item is of low priority.
    /// </summary>
    Low,

    /// <summary>
    /// The item is of high priority.
    /// </summary>
    High,
}
