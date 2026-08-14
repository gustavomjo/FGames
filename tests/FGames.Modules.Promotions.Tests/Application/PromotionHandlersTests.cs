using FGames.Modules.Promotions.Application;
using FGames.Modules.Promotions.Application.Commands;
using FGames.Modules.Promotions.Application.Interfaces;
using FGames.Modules.Promotions.Application.Queries;
using FGames.Modules.Promotions.Domain.Entities;
using FGames.Modules.Promotions.Domain.Interfaces;

namespace FGames.Modules.Promotions.Tests.Application;

public class PromotionHandlersTests
{
    private sealed class FakePromotionRepository : IPromotionRepository
    {
        public Promotion? Promotion { get; init; }
        public bool HasOverlap { get; init; }
        public DateTime? ListMoment { get; private set; }

        public Task<Promotion?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Promotion);

        public Task<IReadOnlyList<Promotion>> ListActiveAsync(
            DateTime moment,
            CancellationToken cancellationToken = default)
        {
            ListMoment = moment;
            var promotion = Promotion;
            IReadOnlyList<Promotion> result = promotion is null ? [] : [promotion];
            return Task.FromResult(result);
        }

        public Task<Promotion?> GetActiveForGameAsync(
            Guid gameId,
            DateTime moment,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Promotion);

        public Task<IReadOnlyDictionary<Guid, Promotion>> GetActiveForGamesAsync(
            IReadOnlyCollection<Guid> gameIds,
            DateTime moment,
            CancellationToken cancellationToken = default)
        {
            var promotion = Promotion;
            IReadOnlyDictionary<Guid, Promotion> result = promotion is null || gameIds.Count == 0
                ? new Dictionary<Guid, Promotion>()
                : new Dictionary<Guid, Promotion> { [gameIds.First()] = promotion };

            return Task.FromResult(result);
        }

        public Task<bool> HasOverlappingActivePromotionAsync(
            Guid gameId,
            DateTime startDate,
            DateTime endDate,
            int excludedPromotionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(HasOverlap);

        public Task AcquireGamePromotionLockAsync(
            Guid gameId,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Add(Promotion promotion) => throw new NotSupportedException();
    }

    private sealed class FakeGameLookupService : IGameLookupService
    {
        public Task<GameLookupResult?> GetPublishedGameAsync(
            Guid gameId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<GameLookupResult?>(new GameLookupResult(gameId, "Game", 100m, true));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(1);
        }

        public Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default) =>
            operation(cancellationToken);
    }

    [Fact]
    public async Task AttachGame_WhenAnotherPromotionOverlaps_ReturnsFailure()
    {
        var promotion = CreatePromotion();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AttachGamePromotionCommandHandler(
            new FakePromotionRepository { Promotion = promotion, HasOverlap = true },
            new FakeGameLookupService(),
            unitOfWork);

        var result = await handler.Handle(
            new AttachGamePromotionCommand(1, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Promotion.OverlappingGamePromotion", result.FirstError.Code);
        Assert.Equal(0, unitOfWork.SaveCalls);
        Assert.Empty(promotion.GamePromotions);
    }

    [Fact]
    public async Task AttachGame_WhenPeriodDoesNotOverlap_AttachesAndSaves()
    {
        var promotion = CreatePromotion();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AttachGamePromotionCommandHandler(
            new FakePromotionRepository { Promotion = promotion, HasOverlap = false },
            new FakeGameLookupService(),
            unitOfWork);

        var result = await handler.Handle(
            new AttachGamePromotionCommand(1, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveCalls);
        Assert.Single(promotion.GamePromotions);
    }

    [Fact]
    public async Task ListActivePromotions_UsesCurrentUtcMoment()
    {
        var repository = new FakePromotionRepository { Promotion = CreatePromotion() };
        var handler = new ListActivePromotionsQueryHandler(repository);
        var before = DateTime.UtcNow;

        var result = await handler.Handle(new ListActivePromotionsQuery(), CancellationToken.None);

        var after = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.NotNull(repository.ListMoment);
        Assert.Equal(DateTimeKind.Utc, repository.ListMoment.Value.Kind);
        Assert.InRange(repository.ListMoment.Value, before, after);
    }

    private static Promotion CreatePromotion() =>
        Promotion.Create(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            20m,
            Guid.NewGuid()).Value;
}
