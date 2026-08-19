using FGames.Modules.Games.Application.Queries;
using FGames.Modules.Games.Domain.Entities;
using FGames.Modules.Games.Domain.Enums;
using FGames.Modules.Games.Domain.Interfaces;

namespace FGames.Modules.Games.Tests.Application;

public class GetGameByIdQueryHandlerTests
{
    private sealed class FakeGameRepository(Game game) : IGameRepository
    {
        public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == game.Id ? game : null);

        public Task<IReadOnlyList<Game>> ListAsync(
            GameStatus? status = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Game>>([]);

        public Task<bool> ExistsByNameAsync(
            string name,
            Guid? excludedGameId = null,
            CancellationToken cancellationToken = default) => Task.FromResult(false);

        public void Add(Game newGame) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Handle_DraftWithoutAdministrativeAccess_ReturnsNotFound()
    {
        var game = CreateDraft();
        var handler = new GetGameByIdQueryHandler(new FakeGameRepository(game));

        var result = await handler.Handle(new GetGameByIdQuery(game.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Game.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_DraftWithAdministrativeAccess_ReturnsGame()
    {
        var game = CreateDraft();
        var handler = new GetGameByIdQueryHandler(new FakeGameRepository(game));

        var result = await handler.Handle(
            new GetGameByIdQuery(game.Id, IncludeUnpublished: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Draft.ToString(), result.Value.Status);
    }

    private static Game CreateDraft() => Game.Create(
        "Draft Game",
        null,
        GameCategory.Action,
        AgeRating.Everyone,
        10m,
        Guid.NewGuid()).Value;
}
