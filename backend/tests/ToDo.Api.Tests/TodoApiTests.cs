using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using ToDo.Application.Todos.Dtos;
using Xunit;

namespace ToDo.Api.Tests;

/// <summary>
/// End-to-end tests that spin up the real Minimal API in-process (with the
/// in-memory database) and drive it over HTTP, exactly like the Angular client would.
/// </summary>
public class TodoApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TodoApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Create_then_GetById_returns_full_detail_with_description()
    {
        var create = await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("Write tests", "Cover the CRUD happy path"));

        create.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<TodoDetailDto>();
        created.ShouldNotBeNull();

        var detail = await _client.GetFromJsonAsync<TodoDetailDto>($"/api/todos/{created!.Id}");
        detail!.Title.ShouldBe("Write tests");
        detail.Description.ShouldBe("Cover the CRUD happy path");
    }

    [Fact]
    public async Task List_endpoint_omits_the_description()
    {
        await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("Hide my description", "secret details that should not be listed"));

        var list = await _client.GetFromJsonAsync<List<TodoDetailDto>>("/api/todos");

        list.ShouldNotBeNull();
        list!.ShouldNotBeEmpty();
        // Summary DTO never serialises Description, so it deserialises back as null for every row.
        list.ShouldAllBe(x => x.Description == null);
    }

    [Fact]
    public async Task Update_changes_the_stored_task()
    {
        var created = await (await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("Before", "before desc"))).Content.ReadFromJsonAsync<TodoDetailDto>();

        var update = await _client.PutAsJsonAsync($"/api/todos/{created!.Id}",
            new UpdateTodoRequest("After", "after desc", true));

        update.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updated = await update.Content.ReadFromJsonAsync<TodoDetailDto>();
        updated!.Title.ShouldBe("After");
        updated.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task Delete_removes_the_task_and_then_returns_404()
    {
        var created = await (await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest("To be deleted", null))).Content.ReadFromJsonAsync<TodoDetailDto>();

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
            new CreateTodoRequest("", "no title provided"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
