using FGames.Modules.Games.Application;
using FGames.Modules.Games.Application.Commands;
using FGames.Modules.Games.Domain.Entities;
using FGames.Modules.Games.Domain.Enums;
using FGames.Modules.Games.Domain.Interfaces;

namespace FGames.Modules.Games.Tests.Application;

public class GameNameUniquenessTests
{
    private sealed class FakeGameRepository : IGameRepository
    {
        public Game? ExistingGame { get; init; }
        public bool NameExists { get; init; }
        public Game? AddedGame { get; private set; }

        public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(ExistingGame?.Id == id ? ExistingGame : null);

        public Task<IReadOnlyList<Game>> ListAsync(
            GameStatus? status = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Game>>([]);

        public Task<bool> ExistsByNameAsync(
            string name,
            Guid? excludedGameId = null,
            CancellationToken cancellationToken = default) => Task.FromResult(NameExists);

        public void Add(Game game) => AddedGame = game;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(1);
        }
    }

    [Fact]
    public async Task Create_WhenNormalizedNameExists_ReturnsConflictErrorWithoutSaving()
    {
        var repository = new FakeGameRepository { NameExists = true };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateGameCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateGameCommand(" Halo ", null, GameCategory.Action, AgeRating.Everyone, 10m, Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Game.NameAlreadyExists", result.FirstError.Code);
        Assert.Null(repository.AddedGame);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task Update_WhenNameBelongsToAnotherGame_ReturnsConflictErrorWithoutSaving()
    {
        var existingGame = Game.Create(
            "Original",
            null,
            GameCategory.Action,
            AgeRating.Everyone,
            10m,
            Guid.NewGuid()).Value;
        var repository = new FakeGameRepository { ExistingGame = existingGame, NameExists = true };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateGameCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new UpdateGameCommand(existingGame.Id, "Duplicado", null, GameCategory.Action, AgeRating.Everyone, 10m),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Game.NameAlreadyExists", result.FirstError.Code);
        Assert.Equal("Original", existingGame.Name);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }
}
