using FGames.Modules.Games.Application.Interfaces;
using FGames.Modules.Games.Application.Queries;
using FGames.Modules.Games.Domain.Entities;
using FGames.Modules.Games.Domain.Enums;
using FGames.Modules.Games.Domain.Interfaces;

namespace FGames.Modules.Games.Tests.Application;

public class ListGamesQueryHandlerTests
{
    private sealed class FakeGameRepository : IGameRepository
    {
        public IReadOnlyList<Game> Games { get; init; } = [];
        public GameStatus? ReceivedStatus { get; private set; }

        public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Games.FirstOrDefault(game => game.Id == id));

        public Task<IReadOnlyList<Game>> ListAsync(
            GameStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            ReceivedStatus = status;
            return Task.FromResult(Games);
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            Guid? excludedGameId = null,
            CancellationToken cancellationToken = default) => Task.FromResult(false);

        public void Add(Game game) => throw new NotSupportedException();
    }

    private sealed class FakeActivePromotionLookupService : IActivePromotionLookupService
    {
        public int Calls { get; private set; }
        public IReadOnlyCollection<Guid> ReceivedGameIds { get; private set; } = [];
        public IReadOnlyDictionary<Guid, ActivePromotionLookupResult> Promotions { get; init; } =
            new Dictionary<Guid, ActivePromotionLookupResult>();

        public Task<IReadOnlyDictionary<Guid, ActivePromotionLookupResult>> GetActivePromotionsForGamesAsync(
            IReadOnlyCollection<Guid> gameIds,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            ReceivedGameIds = gameIds;
            return Task.FromResult(Promotions);
        }
    }

    [Fact]
    public async Task Handle_ForwardsStatusAndLoadsPromotionsInSingleBatch()
    {
        var firstGame = CreateGame("First Game", 100m);
        var secondGame = CreateGame("Second Game", 50m);
        var repository = new FakeGameRepository { Games = [firstGame, secondGame] };
        var promotionLookup = new FakeActivePromotionLookupService
        {
            Promotions = new Dictionary<Guid, ActivePromotionLookupResult>
            {
                [firstGame.Id] = new(1, 20m)
            }
        };
        var handler = new ListGamesQueryHandler(repository, promotionLookup);

        var result = await handler.Handle(new ListGamesQuery(GameStatus.Published), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Published, repository.ReceivedStatus);
        Assert.Equal(1, promotionLookup.Calls);
        Assert.Equal(2, promotionLookup.ReceivedGameIds.Count);
        Assert.Equal(80m, result.Value.Single(game => game.Id == firstGame.Id).FinalPrice);
        Assert.Equal(50m, result.Value.Single(game => game.Id == secondGame.Id).FinalPrice);
    }

    private static Game CreateGame(string name, decimal price)
    {
        var game = Game.Create(
            name,
            description: null,
            GameCategory.Action,
            AgeRating.Everyone,
            price,
            Guid.NewGuid()).Value;

        game.Publish();
        return game;
    }
}
