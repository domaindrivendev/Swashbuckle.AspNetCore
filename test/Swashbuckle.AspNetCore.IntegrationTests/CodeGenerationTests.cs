#if NET10_0_OR_GREATER

using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using DocumentationSnippets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.OpenApi;
using TodoApp.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.IntegrationTests;

/// <summary>
/// Tests that validate that OpenAPI documents produce valid C# code when used with code generation tools.
/// </summary>
public class CodeGenerationTests(ITestOutputHelper outputHelper)
{
    public static TheoryData<ClientGeneratorTool, string> SnapshotTestCases()
    {
        var testCases = new TheoryData<ClientGeneratorTool, string>();
        
        foreach (var path in Directory.EnumerateFiles(Path.Combine(GetProjectRoot(), "snapshots"), "*.txt", SearchOption.AllDirectories))
        {
            // Deduplicate by ignoring snapshots for other TFMs
            if (!path.EndsWith(".DotNet10_0.verified.txt", StringComparison.Ordinal))
            {
                continue;
            }

            using var snapshot = File.OpenRead(path);
            using var document = JsonDocument.Parse(snapshot);

            if (!document.RootElement.TryGetProperty("openapi", out var property) &&
                !document.RootElement.TryGetProperty("swagger", out property))
            {
                continue;
            }

            if (!Version.TryParse(property.GetString(), out var documentVersion))
            {
                continue;
            }

            var version = documentVersion switch
            {
                { Major: 2 } => OpenApiSpecVersion.OpenApi2_0,
                { Major: 3, Minor: 0 } => OpenApiSpecVersion.OpenApi3_0,
                { Major: 3, Minor: 1 } => OpenApiSpecVersion.OpenApi3_1,
                _ => throw new NotSupportedException(path),
            };

            foreach (var tool in Enum.GetValues<ClientGeneratorTool>())
            {
                if (tool is ClientGeneratorTool.NSwag && Path.GetFileNameWithoutExtension(path).Contains("Basic.Startup"))
                {
                    // NSwag doesn't generate valid compilation due to a missing FileResponse type
                    continue;
                }

                if (ClientGenerator.IsSupported(tool, "json", version))
                {
                    testCases.Add(tool, path);
                }
            }
        }

        return testCases;
    }

    [Theory]
    [MemberData(nameof(SnapshotTestCases))]
    public async Task OpenApiDocument_Generates_Valid_Client_Code_From_Snapshot(ClientGeneratorTool tool, string path)
    {
        // Arrange
        var generator = new ClientGenerator(outputHelper);
        var document = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

        using var project = await generator.GenerateFromStringAsync(tool, document);

        // Act and Assert
        await generator.CompileAsync(project.Path);
    }

    [Fact]
    public async Task Can_Manage_Todo_Items_With_Api()
    {
        // Arrange
        await WithTodoAppClientAsync(async (client) =>
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            // Act - Get all the items
            var items = await client.Api.Items.GetAsync(cancellationToken: cancellationToken);

            // Assert - There should be no items
            Assert.NotNull(items);
            Assert.NotNull(items.Items);
            Assert.Empty(items.Items);

            var beforeCount = items.Items.Count;

            // Arrange
            var text = "Buy eggs";
            var newItem = new TodoApp.Client.Models.CreateTodoItemModel { Text = text };

            // Act - Add a new item
            var createdItem = await client.Api.Items.PostAsync(newItem, cancellationToken: cancellationToken);

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
            Assert.Equal(beforeCount + 1, items.Items.Count);
            Assert.Contains(items.Items, (x) => x.Id == itemId);

            item = items.Items.Last();

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
            Assert.Equal(beforeCount, items.Items.Count);
            Assert.DoesNotContain(items.Items, (x) => x.Id == itemId);

            // Act
            var problem = await Assert.ThrowsAsync<TodoApp.Client.Models.ProblemDetails>(
                () => client.Api.Items[new(itemId)].GetAsync(cancellationToken: cancellationToken));

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
            var item = new TodoApp.Client.Models.CreateTodoItemModel { Text = string.Empty };

            // Act
            var problem = await Assert.ThrowsAsync<TodoApp.Client.Models.ProblemDetails>(
                () => client.Api.Items.PostAsync(item, cancellationToken: cancellationToken));

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
            var item = new TodoApp.Client.Models.CreateTodoItemModel { Text = "Something" };

            var createdItem = await client.Api.Items.PostAsync(item, cancellationToken: cancellationToken);

            await client.Api.Items[new(createdItem.Id)].Complete.PostAsync(cancellationToken: cancellationToken);

            // Act
            var problem = await Assert.ThrowsAsync<TodoApp.Client.Models.ProblemDetails>(
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
            var item = new TodoApp.Client.Models.CreateTodoItemModel { Text = "Something" };

            var createdItem = await client.Api.Items.PostAsync(item, cancellationToken: cancellationToken);

            await client.Api.Items[new(createdItem.Id)].DeleteAsync(cancellationToken: cancellationToken);

            // Act
            var problem = await Assert.ThrowsAsync<TodoApp.Client.Models.ProblemDetails>(
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
            var item = new TodoApp.Client.Models.CreateTodoItemModel { Text = "Something" };

            var createdItem = await client.Api.Items.PostAsync(item, cancellationToken: cancellationToken);

            await client.Api.Items[new(createdItem.Id)].DeleteAsync(cancellationToken: cancellationToken);

            // Act
            var problem = await Assert.ThrowsAsync<TodoApp.Client.Models.ProblemDetails>(
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

    private static string GetProjectRoot() =>
        typeof(CodeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "ProjectRoot")
            .Value!;

    private static async Task WithTodoAppClientAsync(Func<TodoApp.Client.TodoApiClient, Task> callback)
    {
        using var httpClient = SwaggerIntegrationTests.GetHttpClientForTestApplication(typeof(TodoApp.Program));

        var provider = new AnonymousAuthenticationProvider();
        using var request = new HttpClientRequestAdapter(provider, httpClient: httpClient);

        var client = new TodoApp.Client.TodoApiClient(request);

        await callback(client);
    }
}

#endif
