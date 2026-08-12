using FGames.Modules.Library.Domain.Entities;

namespace FGames.Modules.Library.Tests.Domain;

public class UserGameTests
{
    [Fact]
    public void Purchase_WithValidPrice_ReturnsSuccess()
    {
        var result = UserGame.Purchase(Guid.NewGuid(), Guid.NewGuid(), 49.90m);

        Assert.True(result.IsSuccess);
        Assert.Equal(49.90m, result.Value.PricePaid);
    }

    [Fact]
    public void Purchase_WithZeroPrice_ReturnsSuccess()
    {
        var result = UserGame.Purchase(Guid.NewGuid(), Guid.NewGuid(), 0m);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Purchase_WithNegativePrice_ReturnsFailure()
    {
        var result = UserGame.Purchase(Guid.NewGuid(), Guid.NewGuid(), -0.01m);

        Assert.True(result.IsFailure);
    }
}
