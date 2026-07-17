using Shouldly;
using ToDo.Application.Todos.Dtos;
using ToDo.Application.Todos.Validators;
using Xunit;

namespace ToDo.Application.Tests.Todos;

public class TodoValidatorTests
{
    private readonly CreateTodoRequestValidator _createValidator = new();
    private readonly UpdateTodoRequestValidator _updateValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_is_invalid_when_title_is_blank(string title)
    {
        var result = _createValidator.Validate(new CreateTodoRequest(title, null));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateTodoRequest.Title));
    }

    [Fact]
    public void Create_is_invalid_when_title_exceeds_max_length()
    {
        var result = _createValidator.Validate(new CreateTodoRequest(new string('x', 201), null));

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Create_is_valid_with_title_and_optional_description()
    {
        var result = _createValidator.Validate(new CreateTodoRequest("Valid", null));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Update_is_invalid_when_title_is_blank()
    {
        var result = _updateValidator.Validate(new UpdateTodoRequest("", null, false));

        result.IsValid.ShouldBeFalse();
    }
}
