using FGames.Modules.Library.Application;
using FGames.Modules.Library.Application.Commands;
using FGames.Modules.Library.Application.Interfaces;
using FGames.Modules.Library.Domain.Entities;
using FGames.Modules.Library.Domain.Interfaces;

namespace FGames.Modules.Library.Tests.Application;

public class PurchaseGameCommandHandlerTests
{
    private sealed class FakeUserGameRepository : IUserGameRepository
    {
        private readonly List<UserGame> _items = [];
        public bool ExistingPurchase { get; set; }

        public Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingPurchase);

        public Task<IReadOnlyList<UserGame>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<UserGame>>(_items.Where(i => i.UserId == userId).ToList());

        public void Add(UserGame userGame) => _items.Add(userGame);
    }

    private sealed class FakeGameLookupService : IGameLookupService
    {
        public GameLookupResult? Game { get; set; }

        public Task<GameLookupResult?> GetPublishedGameAsync(Guid gameId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Game);
    }

    private sealed class FakeActivePromotionLookupService : IActivePromotionLookupService
    {
        public ActivePromotionLookupResult? Promotion { get; set; }

        public Task<ActivePromotionLookupResult?> GetActivePromotionForGameAsync(Guid gameId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Promotion);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    [Fact]
    public async Task Handle_WhenAlreadyPurchased_ReturnsFailure()
    {
        var repository = new FakeUserGameRepository { ExistingPurchase = true };
        var handler = new PurchaseGameCommandHandler(repository, new FakeGameLookupService(), new FakeActivePromotionLookupService(), new FakeUnitOfWork());

        var result = await handler.Handle(new PurchaseGameCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("UserGame.AlreadyPurchased", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WhenGameNotPublished_ReturnsFailure()
    {
        var repository = new FakeUserGameRepository();
        var gameLookup = new FakeGameLookupService { Game = null };
        var handler = new PurchaseGameCommandHandler(repository, gameLookup, new FakeActivePromotionLookupService(), new FakeUnitOfWork());

        var result = await handler.Handle(new PurchaseGameCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("UserGame.GameNotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_WithActivePromotion_AppliesDiscountToPricePaid()
    {
        var gameId = Guid.NewGuid();
        var repository = new FakeUserGameRepository();
        var gameLookup = new FakeGameLookupService { Game = new GameLookupResult(gameId, "Space Quest", 100m, true) };
        var promotionLookup = new FakeActivePromotionLookupService { Promotion = new ActivePromotionLookupResult(1, 25m) };
        var handler = new PurchaseGameCommandHandler(repository, gameLookup, promotionLookup, new FakeUnitOfWork());

        var result = await handler.Handle(new PurchaseGameCommand(Guid.NewGuid(), gameId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(75m, result.Value.PricePaid);
    }

    [Fact]
    public async Task Handle_WithoutActivePromotion_ChargesFullPrice()
    {
        var gameId = Guid.NewGuid();
        var repository = new FakeUserGameRepository();
        var gameLookup = new FakeGameLookupService { Game = new GameLookupResult(gameId, "Space Quest", 100m, true) };
        var handler = new PurchaseGameCommandHandler(repository, gameLookup, new FakeActivePromotionLookupService(), new FakeUnitOfWork());

        var result = await handler.Handle(new PurchaseGameCommand(Guid.NewGuid(), gameId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value.PricePaid);
    }
}
