using FGames.Modules.Users.Application.Commands;
using FGames.Modules.Users.Application.Validators;
using FGames.Modules.Users.Domain.Enums;

namespace FGames.Modules.Users.Tests.Application;

public class UserCommandValidatorTests
{
    [Fact]
    public void CreateUser_WithUndefinedRole_IsInvalid()
    {
        var command = new CreateUserCommand(
            "User",
            "user@example.com",
            "Password123!",
            null,
            Guid.NewGuid(),
            (Role)999);

        var result = new CreateUserCommandValidator().Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateUserCommand.Role));
    }

    [Fact]
    public void SetUserStatus_WithUndefinedStatus_IsInvalid()
    {
        var command = new SetUserStatusCommand(Guid.NewGuid(), (UserStatus)999);

        var result = new SetUserStatusCommandValidator().Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SetUserStatusCommand.Status));
    }
}
