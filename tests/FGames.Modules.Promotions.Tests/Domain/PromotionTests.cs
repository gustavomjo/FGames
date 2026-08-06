using FGames.Modules.Promotions.Domain.Entities;

namespace FGames.Modules.Promotions.Tests.Domain;

public class PromotionTests
{
    private static Guid AdminId => Guid.NewGuid();
    private static DateTime Start => new(2026, 1, 1);
    private static DateTime End => new(2026, 1, 31);

    [Fact]
    public void Create_WithValidPeriodAndDiscount_ReturnsSuccess()
    {
        var result = Promotion.Create(Start, End, 20m, AdminId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Active);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ReturnsFailure()
    {
        var result = Promotion.Create(End, Start, 20m, AdminId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithEndDateEqualToStartDate_ReturnsFailure()
    {
        var result = Promotion.Create(Start, Start, 20m, AdminId);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(100.01)]
    [InlineData(150)]
    public void Create_WithInvalidDiscount_ReturnsFailure(decimal discount)
    {
        var result = Promotion.Create(Start, End, discount, AdminId);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithDiscountAtBoundaries_ReturnsSuccess()
    {
        Assert.True(Promotion.Create(Start, End, 0.01m, AdminId).IsSuccess);
        Assert.True(Promotion.Create(Start, End, 100m, AdminId).IsSuccess);
    }

    [Fact]
    public void AttachToGame_SameGameTwice_ReturnsFailureOnSecondAttempt()
    {
        var promotion = Promotion.Create(Start, End, 20m, AdminId).Value;
        var gameId = Guid.NewGuid();

        var first = promotion.AttachToGame(gameId, AdminId);
        var second = promotion.AttachToGame(gameId, AdminId);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsFailure);
        Assert.Single(promotion.GamePromotions);
    }

    [Fact]
    public void IsActiveOn_WithinPeriodAndActive_ReturnsTrue()
    {
        var promotion = Promotion.Create(Start, End, 20m, AdminId).Value;

        Assert.True(promotion.IsActiveOn(new DateTime(2026, 1, 15)));
        Assert.False(promotion.IsActiveOn(new DateTime(2026, 2, 1)));
    }

    [Fact]
    public void IsActiveOn_AfterDeactivate_ReturnsFalse()
    {
        var promotion = Promotion.Create(Start, End, 20m, AdminId).Value;
        promotion.Deactivate();

        Assert.False(promotion.IsActiveOn(new DateTime(2026, 1, 15)));
    }
}
