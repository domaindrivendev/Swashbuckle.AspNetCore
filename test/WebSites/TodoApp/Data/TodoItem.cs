namespace TodoApp.Data;

public class TodoItem
{
    public Guid Id { get; set; }

    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
