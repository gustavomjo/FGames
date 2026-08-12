using FGames.SharedKernel;

namespace FGames.Modules.Promotions.Domain.Entities;

public sealed class GamePromotion : Entity<int>
{
    public Guid GameId { get; private set; }
    public int PromotionId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private GamePromotion()
        : base(0)
    {
    }

    internal GamePromotion(Guid gameId, int promotionId, Guid createdByUserId)
        : base(0)
    {
        GameId = gameId;
        PromotionId = promotionId;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }
}
