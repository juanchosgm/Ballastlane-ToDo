using Microsoft.EntityFrameworkCore;
using Shouldly;
using ToDo.Domain.Entities;
using ToDo.Domain.Enums;
using ToDo.Infrastructure.Persistence;
using ToDo.Infrastructure.Repositories;
using Xunit;

namespace ToDo.Application.Tests.Todos;

/// <summary>
/// Exercises the repository against a real EF Core in-memory context to verify
/// the persistence mapping and query behaviour (uniquely named DB per test).
/// </summary>
public class TodoRepositoryTests
{
    private const string UserId = "user-1";

    private static TodoDbContext NewContext() =>
        new(new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase($"repo-tests-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task AddAsync_then_GetById_round_trips_the_entity()
    {
        await using var ctx = NewContext();
        var repo = new TodoRepository(ctx);
        var item = new TodoItem("Task", "Details", UserId);

        await repo.AddAsync(item);
        var fetched = await repo.GetByIdAsync(item.Id, UserId);

        fetched.ShouldNotBeNull();
        fetched!.Title.ShouldBe("Task");
        fetched.Description.ShouldBe("Details");
    }

    [Fact]
    public async Task GetById_returns_null_for_another_users_task()
    {
        await using var ctx = NewContext();
        var repo = new TodoRepository(ctx);
        var item = new TodoItem("Private", "not yours", UserId);
        await repo.AddAsync(item);

        (await repo.GetByIdAsync(item.Id, "someone-else")).ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsync_returns_only_the_owners_items_newest_first()
    {
        await using var ctx = NewContext();
        var repo = new TodoRepository(ctx);
        await repo.AddAsync(new TodoItem("first", null, UserId));
        await Task.Delay(5);
        await repo.AddAsync(new TodoItem("second", null, UserId));
        await repo.AddAsync(new TodoItem("other user", null, "user-2"));

        var all = await repo.GetAllAsync(UserId);

        all.Count.ShouldBe(2);
        all[0].Title.ShouldBe("second");
    }

    [Fact]
    public async Task DeleteAsync_removes_the_entity()
    {
        await using var ctx = NewContext();
        var repo = new TodoRepository(ctx);
        var item = new TodoItem("temp", null, UserId);
        await repo.AddAsync(item);

        await repo.DeleteAsync(item);

        (await repo.GetByIdAsync(item.Id, UserId)).ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_persists_changes()
    {
        await using var ctx = NewContext();
        var repo = new TodoRepository(ctx);
        var item = new TodoItem("before", null, UserId);
        await repo.AddAsync(item);

        item.Update("after", "now with description", TodoStatus.Done, null);
        await repo.UpdateAsync(item);

        var reloaded = await repo.GetByIdAsync(item.Id, UserId);
        reloaded!.Title.ShouldBe("after");
        reloaded.Status.ShouldBe(TodoStatus.Done);
        reloaded.Description.ShouldBe("now with description");
    }
}
