using NSubstitute;
using Shouldly;
using ToDo.Application.Common.Exceptions;
using ToDo.Application.Common.Interfaces;
using ToDo.Application.Todos.Dtos;
using ToDo.Application.Todos.Services;
using ToDo.Domain.Entities;
using Xunit;

namespace ToDo.Application.Tests.Todos;

public class TodoServiceTests
{
    private readonly ITodoRepository _repository = Substitute.For<ITodoRepository>();
    private readonly TodoService _sut;

    public TodoServiceTests() => _sut = new TodoService(_repository);

    [Fact]
    public async Task CreateAsync_persists_item_and_returns_detail()
    {
        var request = new CreateTodoRequest("Buy milk", "2 liters, semi-skimmed");

        var result = await _sut.CreateAsync(request);

        result.Title.ShouldBe("Buy milk");
        result.Description.ShouldBe("2 liters, semi-skimmed");
        result.IsCompleted.ShouldBeFalse();
        await _repository.Received(1).AddAsync(Arg.Is<TodoItem>(t => t.Title == "Buy milk"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_returns_detail_when_found()
    {
        var item = new TodoItem("Read a book", "Chapter 1-3");
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _sut.GetByIdAsync(item.Id);

        result.Id.ShouldBe(item.Id);
        result.Description.ShouldBe("Chapter 1-3");
    }

    [Fact]
    public async Task GetByIdAsync_throws_NotFound_when_missing()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TodoItem?)null);

        await Should.ThrowAsync<NotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_mutates_and_saves_existing_item()
    {
        var item = new TodoItem("Old title", "Old description");
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _sut.UpdateAsync(item.Id, new UpdateTodoRequest("New title", "New description", true));

        result.Title.ShouldBe("New title");
        result.Description.ShouldBe("New description");
        result.IsCompleted.ShouldBeTrue();
        result.UpdatedAt.ShouldNotBeNull();
        await _repository.Received(1).UpdateAsync(item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_throws_NotFound_when_missing()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TodoItem?)null);

        await Should.ThrowAsync<NotFoundException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), new UpdateTodoRequest("t", null, false)));
    }

    [Fact]
    public async Task DeleteAsync_removes_existing_item()
    {
        var item = new TodoItem("Delete me", null);
        _repository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);

        await _sut.DeleteAsync(item.Id);

        await _repository.Received(1).DeleteAsync(item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_throws_NotFound_when_missing()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TodoItem?)null);

        await Should.ThrowAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_maps_every_item_to_summary()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<TodoItem>
        {
            new("A", "desc a"),
            new("B", "desc b")
        });

        var result = await _sut.GetAllAsync();

        result.Count.ShouldBe(2);
        result.ShouldAllBe(x => x.Title != null);
    }
}
