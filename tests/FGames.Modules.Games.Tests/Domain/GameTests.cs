using FGames.Modules.Games.Domain.Entities;
using FGames.Modules.Games.Domain.Enums;

namespace FGames.Modules.Games.Tests.Domain;

public class GameTests
{
    private static Guid AdminId => Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_StartsAsDraft()
    {
        var result = Game.Create("Space Quest", "Um jogo de aventura", GameCategory.Adventure, AgeRating.Everyone, 49.90m, AdminId);

        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Draft, result.Value.Status);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var result = Game.Create("  Space Quest  ", null, GameCategory.Adventure, AgeRating.Everyone, 49.90m, AdminId);

        Assert.True(result.IsSuccess);
        Assert.Equal("Space Quest", result.Value.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ReturnsFailure(string name)
    {
        var result = Game.Create(name, null, GameCategory.Action, AgeRating.Everyone, 10m, AdminId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithNegativePrice_ReturnsFailure()
    {
        var result = Game.Create("Space Quest", null, GameCategory.Adventure, AgeRating.Everyone, -1m, AdminId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithUndefinedCategory_ReturnsFailure()
    {
        var result = Game.Create("Space Quest", null, (GameCategory)999, AgeRating.Everyone, 10m, AdminId);

        Assert.True(result.IsFailure);
        Assert.Equal("Game.InvalidCategory", result.FirstError.Code);
    }

    [Fact]
    public void Create_WithUndefinedAgeRating_ReturnsFailure()
    {
        var result = Game.Create("Space Quest", null, GameCategory.Action, (AgeRating)999, 10m, AdminId);

        Assert.True(result.IsFailure);
        Assert.Equal("Game.InvalidAgeRating", result.FirstError.Code);
    }

    [Fact]
    public void Publish_FromDraft_SetsStatusToPublished()
    {
        var game = Game.Create("Space Quest", null, GameCategory.Adventure, AgeRating.Everyone, 49.90m, AdminId).Value;

        var result = game.Publish();

        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Published, game.Status);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ReturnsFailure()
    {
        var game = Game.Create("Space Quest", null, GameCategory.Adventure, AgeRating.Everyone, 49.90m, AdminId).Value;
        game.Publish();

        var result = game.Publish();

        Assert.True(result.IsFailure);
    }
}
