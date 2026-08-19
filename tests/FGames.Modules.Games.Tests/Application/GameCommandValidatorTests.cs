using FGames.Modules.Games.Application.Commands;
using FGames.Modules.Games.Application.Validators;
using FGames.Modules.Games.Domain.Enums;

namespace FGames.Modules.Games.Tests.Application;

public class GameCommandValidatorTests
{
    [Fact]
    public void CreateGame_WithUndefinedEnums_IsInvalid()
    {
        var command = new CreateGameCommand(
            "Game",
            null,
            (GameCategory)999,
            (AgeRating)999,
            10m,
            Guid.NewGuid());

        var result = new CreateGameCommandValidator().Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGameCommand.Category));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGameCommand.Rating));
    }
}
