using FGames.SharedKernel;

namespace FGames.Modules.Promotions.Domain.Entities;

public sealed class Promotion : AggregateRoot<int>
{
    private readonly List<GamePromotion> _gamePromotions = [];

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public bool Active { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<GamePromotion> GamePromotions => _gamePromotions.AsReadOnly();

    private Promotion()
        : base(0)
    {
    }

    public static Result<Promotion> Create(
        DateTime startDate,
        DateTime endDate,
        decimal discountPercentage,
        Guid createdByUserId)
    {
        var errors = new List<Error>();

        if (endDate <= startDate)
            errors.Add(new Error("Promotion.InvalidPeriod", "A data final deve ser posterior à data inicial."));

        if (discountPercentage <= 0 || discountPercentage > 100)
            errors.Add(new Error("Promotion.InvalidDiscount", "O desconto deve ser maior que 0 e no máximo 100."));

        if (errors.Count > 0)
            return Result.Failure<Promotion>(errors);

        var promotion = new Promotion
        {
            StartDate = startDate,
            EndDate = endDate,
            DiscountPercentage = discountPercentage,
            Active = true,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(promotion);
    }

    public Result AttachToGame(Guid gameId, Guid createdByUserId)
    {
        if (_gamePromotions.Any(gp => gp.GameId == gameId))
            return Result.Failure(new Error("Promotion.GameAlreadyAttached", "Este jogo já está vinculado a esta promoção."));

        _gamePromotions.Add(new GamePromotion(gameId, Id, createdByUserId));
        return Result.Success();
    }

    public bool IsActiveOn(DateTime moment) => Active && moment >= StartDate && moment <= EndDate;

    public void Deactivate() => Active = false;
}
