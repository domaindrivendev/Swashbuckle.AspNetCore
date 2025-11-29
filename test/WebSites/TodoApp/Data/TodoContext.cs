using Microsoft.EntityFrameworkCore;

namespace TodoApp.Data;

public class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Items { get; set; } = default!;
}
