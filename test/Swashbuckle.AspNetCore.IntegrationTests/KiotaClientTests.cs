using Microsoft.AspNetCore.Http;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using TodoApp.KiotaClient;
using TodoApp.KiotaClient.Models;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class KiotaClientTests
{
    [Fact]
    public async Task Can_Manage_Todo_Items_With_Api()
    {
        // Arrange
        await WithTodoAppClientAsync(async (client) =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act - Get all the items
            var items = await client.Api.Items.GetAsync(cancellationToken: cancellationToken);

            // Assert
            Assert.NotNull(items);
            Assert.NotNull(items.Items);

            // Arrange
            var text = "Buy eggs";

            // Act - Add a new item
            var createdItem = await client.Api.Items.PostAsync(
                new() { Text = text },
                cancellationToken: cancellationToken);

            // Assert - An item was created
            Assert.NotNull(createdItem);
            Assert.NotEqual(default, createdItem.Id);

            // Arrange - Get the new item's URL and Id
            var itemId = createdItem.Id;

            // Act - Get the item
            var item = await client.Api.Items[new(itemId)].GetAsync(cancellationToken: cancellationToken);

            // Assert - Verify the item was created correctly
            Assert.NotNull(item);
            Assert.Equal(itemId, item.Id);
            Assert.Null(item.CompletedAt);
            Assert.NotEqual(default, item.CreatedAt);
            Assert.Equal(item.CreatedAt.Value, item.LastUpdated);
            Assert.Null(item.Priority);
            Assert.Equal(text, item.Text);

            // Act - Update the item to be high priority
            await client.Api.Items[new(itemId)].Priority.PatchAsync(
                new() { Priority = TodoPriority.High },
                cancellationToken: cancellationToken);

            item = await client.Api.Items[new(itemId)].GetAsync(cancellationToken: cancellationToken);

            Assert.NotNull(item);
            Assert.Equal(itemId, item.Id);
            Assert.Null(item.CompletedAt);
            Assert.NotEqual(default, item.CreatedAt);
            Assert.Equal(item.CreatedAt.Value, item.LastUpdated);
            Assert.Equal(TodoPriority.High, item.Priority);
            Assert.Equal(text, item.Text);

            // Act - Mark the item as being completed
            await client.Api.Items[new(itemId)].Complete.PostAsync(cancellationToken: cancellationToken);

            item = await client.Api.Items[new(itemId)].GetAsync(cancellationToken: cancellationToken);

            Assert.NotNull(item);
            Assert.Equal(itemId, item.Id);
            Assert.Equal(text, item.Text);
            Assert.NotNull(item.CompletedAt);
            Assert.Equal(item.CompletedAt.Value, item.LastUpdated);
            Assert.True(item.CompletedAt.Value > item.CreatedAt);

            // Act - Get all the items
            items = await client.Api.Items.GetAsync(cancellationToken: cancellationToken);

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
            await client.Api.Items[new(itemId)].DeleteAsync(cancellationToken: cancellationToken);

            // Assert - The item no longer exists
            items = await client.Api.Items.GetAsync(cancellationToken: cancellationToken);

            Assert.NotNull(items);
            Assert.NotNull(items.Items);
            Assert.DoesNotContain(items.Items, (x) => x.Id == itemId);

            // Act
            var problem = await Assert.ThrowsAsync<ProblemDetails>(
                () => client.Api.Items[new(itemId)].GetAsync(cancellationToken: cancellationToken));

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);

            // Act
            problem = await Assert.ThrowsAsync<ProblemDetails>(
                () => client.Api.Items[new(itemId)].Complete.PostAsync(cancellationToken: cancellationToken));

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);

            // Act
            problem = await Assert.ThrowsAsync<ProblemDetails>(
                () => client.Api.Items[new(itemId)].Priority.PatchAsync(new() { Priority = TodoPriority.Low }, cancellationToken: cancellationToken));

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
            var problem = await Assert.ThrowsAsync<ProblemDetails>(
                () => client.Api.Items.PostAsync(new() { Text = string.Empty }, cancellationToken: cancellationToken));

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

            var createdItem = await client.Api.Items.PostAsync(
                new() { Text = "Something" },
                cancellationToken: cancellationToken);

            await client.Api.Items[new(createdItem.Id)].Complete.PostAsync(cancellationToken: cancellationToken);

            // Act
            var problem = await Assert.ThrowsAsync<ProblemDetails>(
                () => client.Api.Items[new(createdItem.Id)].Complete.PostAsync(cancellationToken: cancellationToken));

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

            var createdItem = await client.Api.Items.PostAsync(
                new() { Text = "Something" },
                cancellationToken: cancellationToken);

            await client.Api.Items[new(createdItem.Id)].DeleteAsync(cancellationToken: cancellationToken);

            // Act
            var problem = await Assert.ThrowsAsync<ProblemDetails>(
                () => client.Api.Items[new(createdItem.Id)].Complete.PostAsync(cancellationToken: cancellationToken));

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

            var createdItem = await client.Api.Items.PostAsync(
                new() { Text = "Something" },
                cancellationToken: cancellationToken);

            await client.Api.Items[new(createdItem.Id)].DeleteAsync(cancellationToken: cancellationToken);

            // Act
            var problem = await Assert.ThrowsAsync<ProblemDetails>(
                () => client.Api.Items[new(createdItem.Id)].DeleteAsync(cancellationToken: cancellationToken));

            // Assert
            Assert.NotNull(problem);
            Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal("Item not found.", problem.Detail);
            Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem.Type);
            Assert.Null(problem.Instance);
        });
    }

    private static async Task WithTodoAppClientAsync(Func<KiotaTodoApiClient, Task> callback)
    {
        using var httpClient = SwaggerIntegrationTests.GetHttpClientForTestApplication(typeof(TodoApp.Program));

        var provider = new AnonymousAuthenticationProvider();
        using var request = new HttpClientRequestAdapter(provider, httpClient: httpClient);

        var client = new KiotaTodoApiClient(request);

        await callback(client);
    }
}
