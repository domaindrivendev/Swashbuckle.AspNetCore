using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;

namespace TodoApp;

[JsonSerializable(typeof(CreateTodoItemModel))]
[JsonSerializable(typeof(CreatedTodoItemModel))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(TodoItemFilterModel))]
[JsonSerializable(typeof(TodoItemModel))]
[JsonSerializable(typeof(TodoListViewModel))]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling = JsonNumberHandling.Strict,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
public sealed partial class TodoJsonSerializerContext : JsonSerializerContext;
