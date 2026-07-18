using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using ToDo.Application.Todos.Dtos;
using ToDo.Domain.Enums;
using Xunit;

namespace ToDo.Api.Tests;

/// <summary>
/// End-to-end tests that spin up the real Minimal API in-process (with the
/// in-memory database) and drive it over HTTP, exactly like the Angular client would.
/// Every test authenticates as the seeded demo user first, since the To-Do endpoints
/// now require a bearer token.
/// </summary>
public class TodoApiTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    // Matches ToDo.Infrastructure.Persistence.IdentitySeeder defaults.
    private const string SeedEmail = "demo@ballastlane.com";
    private const string SeedPassword = "Passw0rd!";

    // Mirrors the API's JSON contract so TodoStatus deserialises from its name ("Done").
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public TodoApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = SeedEmail, password = SeedPassword });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Todos_endpoint_requires_authentication()
    {
        using var anonymous = new WebApplicationFactory<Program>().CreateClient();

        var response = await anonymous.GetAsync("/api/todos");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_then_GetById_returns_full_detail_with_description()
    {
        var create = await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("Write tests", "Cover the CRUD happy path", null));

        create.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<TodoDetailDto>(JsonOptions);
        created.ShouldNotBeNull();

        var detail = await _client.GetFromJsonAsync<TodoDetailDto>($"/api/todos/{created!.Id}", JsonOptions);
        detail!.Title.ShouldBe("Write tests");
        detail.Description.ShouldBe("Cover the CRUD happy path");
    }

    [Fact]
    public async Task List_endpoint_omits_the_description()
    {
        await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("Hide my description", "secret details that should not be listed", null));

        var list = await _client.GetFromJsonAsync<List<TodoDetailDto>>("/api/todos", JsonOptions);

        list.ShouldNotBeNull();
        list!.ShouldNotBeEmpty();
        // Summary DTO never serialises Description, so it deserialises back as null for every row.
        list.ShouldAllBe(x => x.Description == null);
    }

    [Fact]
    public async Task Update_changes_the_stored_task()
    {
        var created = await (await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("Before", "before desc", null))).Content.ReadFromJsonAsync<TodoDetailDto>(JsonOptions);

        var update = await _client.PutAsJsonAsync($"/api/todos/{created!.Id}",
            new UpdateTodoRequest("After", "after desc", TodoStatus.Done, null));

        update.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updated = await update.Content.ReadFromJsonAsync<TodoDetailDto>(JsonOptions);
        updated!.Title.ShouldBe("After");
        updated.Status.ShouldBe(TodoStatus.Done);
    }

    [Fact]
    public async Task Delete_removes_the_task_and_then_returns_404()
    {
        var created = await (await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("To be deleted", null, null))).Content.ReadFromJsonAsync<TodoDetailDto>(JsonOptions);

        var delete = await _client.DeleteAsync($"/api/todos/{created!.Id}");
        delete.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var afterDelete = await _client.GetAsync($"/api/todos/{created.Id}");
        afterDelete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_returns_404_for_unknown_id()
    {
        var response = await _client.GetAsync($"/api/todos/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_returns_400_when_title_is_missing()
    {
        var response = await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("", "no title provided", null));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private sealed record AccessTokenResponse(string TokenType, string AccessToken, long ExpiresIn, string RefreshToken);
}
