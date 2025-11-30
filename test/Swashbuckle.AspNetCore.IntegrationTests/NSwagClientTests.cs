#if NET10_0_OR_GREATER

using Microsoft.AspNetCore.Http;
using TodoApp.NSwagClient;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class NSwagClientTests
{
    [Fact]
    public async Task Can_Manage_Todo_Items_With_Api()
    {
        // Arrange
        await WithTodoAppClientAsync(async (client) =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act - Get all the items
            var items = await client.ListTodosAsync(cancellationToken);

            // Assert
            Assert.NotNull(items);
            Assert.NotNull(items.Items);

            // Arrange
            var text = "Buy eggs";

            // Act - Add a new item
            var createdItem = await client.CreateTodoAsync(
                new() { Text = text },
                cancellationToken);

            // Assert - An item was created
            Assert.NotNull(createdItem);
            Assert.NotEqual(default, createdItem.Id);

            // Arrange - Get the new item's URL and Id
            var itemId = createdItem.Id;

            // Act - Get the item
            var item = await client.GetTodoAsync(new(itemId), cancellationToken);

            // Assert - Verify the item was created correctly
            Assert.NotNull(item);
            Assert.Equal(itemId, item.Id);
            Assert.Null(item.CompletedAt);
            Assert.NotEqual(default, item.CreatedAt);
            Assert.Equal(item.CreatedAt, item.LastUpdated);
            Assert.Null(item.Priority);
            Assert.Equal(text, item.Text);

            // Act - Update the item to be high priority
            await client.UpdateTodoPriorityAsync(
                new(itemId),
                new() { Priority = TodoPriority.High },
                cancellationToken);

            item = await client.GetTodoAsync(new(itemId), cancellationToken);

            Assert.NotNull(item);
            Assert.Equal(itemId, item.Id);
            Assert.Null(item.CompletedAt);
            Assert.NotEqual(default, item.CreatedAt);
            Assert.Equal(item.CreatedAt, item.LastUpdated);
            Assert.Equal(TodoPriority.High, item.Priority);
            Assert.Equal(text, item.Text);

            // Act - Mark the item as being completed
            await client.CompleteTodoAsync(new(itemId), cancellationToken);

            item = await client.GetTodoAsync(new(itemId), cancellationToken);

            Assert.NotNull(item);
            Assert.Equal(itemId, item.Id);
            Assert.Equal(text, item.Text);
            Assert.NotNull(item.CompletedAt);
            Assert.Equal(item.CompletedAt.Value, item.LastUpdated);
            Assert.True(item.CompletedAt.Value > item.CreatedAt);

            // Act - Get all the items
            items = await client.ListTodosAsync(cancellationToken);

            // Assert - The item was completed
            Assert.NotNull(items);
            Assert.NotNull(items.Items);
            Assert.Contains(items.Items, (x) => x.Id == itemId);

            item = items.Items.Single((p) => p.Id == itemId);

            Assert.NotNull(item);
            Assert.Equal(itemId, item.Id);
            Assert.Equal(text, item.Text);
            Assert.NotNull(item.CompletedAt);
            Assert.Equal(item.CompletedAt.Value, item.LastUpdated);
            Assert.True(item.CompletedAt.Value > item.CreatedAt);

            // Act - Delete the item
            await client.DeleteTodoAsync(new(itemId), cancellationToken);

            // Assert - The item no longer exists
            items = await client.ListTodosAsync(cancellationToken);

            Assert.NotNull(items);
            Assert.NotNull(items.Items);
            Assert.DoesNotContain(items.Items, (x) => x.Id == itemId);

            // Act
            var error = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.GetTodoAsync(new(itemId), cancellationToken));

            var problem = error.Result;

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);

            // Act
            error = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CompleteTodoAsync(new(itemId), cancellationToken));

            problem = error.Result;

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);

            // Act
            error = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.UpdateTodoPriorityAsync(new(itemId), new() { Priority = TodoPriority.Low }, cancellationToken));

            problem = error.Result;

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);
        });
    }

    [Fact]
    public async Task Cannot_Create_Todo_Item_With_No_Text()
    {
        // Arrange
        await WithTodoAppClientAsync(async (client) =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            var error = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CreateTodoAsync(new() { Text = string.Empty }, cancellationToken));

            var problem = error.Result;

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
            Assert.Equal("Bad Request", problem.Title);
            Assert.Equal("No item text specified.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problem.Type);
            Assert.Null(problem.Instance);
        });
    }

    [Fact]
    public async Task Cannot_Complete_Todo_Item_Multiple_Times()
    {
        // Arrange
        await WithTodoAppClientAsync(async (client) =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            var createdItem = await client.CreateTodoAsync(
                new() { Text = "Something" },
                cancellationToken);

            await client.CompleteTodoAsync(new(createdItem.Id), cancellationToken);

            // Act
            var error = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CompleteTodoAsync(new(createdItem.Id), cancellationToken));

            var problem = error.Result;

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
            Assert.Equal("Bad Request", problem.Title);
            Assert.Equal("Item already completed.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.1", problem.Type);
            Assert.Null(problem.Instance);
        });
    }

    [Fact]
    public async Task Cannot_Complete_Deleted_Todo_Item()
    {
        // Arrange
        await WithTodoAppClientAsync(async (client) =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            var createdItem = await client.CreateTodoAsync(
                new() { Text = "Something" },
                cancellationToken);

            await client.DeleteTodoAsync(new(createdItem.Id), cancellationToken);

            // Act
            var error = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CompleteTodoAsync(new(createdItem.Id), cancellationToken));

            var problem = error.Result;

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);
        });
    }

    [Fact]
    public async Task Cannot_Delete_Todo_Item_Multiple_Times()
    {
        // Arrange
        await WithTodoAppClientAsync(async (client) =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            var createdItem = await client.CreateTodoAsync(
                new() { Text = "Something" },
                cancellationToken);

            await client.DeleteTodoAsync(new(createdItem.Id), cancellationToken);

            // Act
            var error = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.DeleteTodoAsync(new(createdItem.Id), cancellationToken));

            var problem = error.Result;

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);
        });
    }

    private static async Task WithTodoAppClientAsync(Func<NSwagTodoApiClient, Task> callback)
    {
        using var httpClient = SwaggerIntegrationTests.GetHttpClientForTestApplication(typeof(TodoApp.Program));

        var client = new NSwagTodoApiClient(httpClient.BaseAddress.ToString(), httpClient);

        await callback(client);
    }
}

#endif
