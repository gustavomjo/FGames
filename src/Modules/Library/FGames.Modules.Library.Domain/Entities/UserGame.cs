using FGames.SharedKernel;

namespace FGames.Modules.Library.Domain.Entities;

public sealed class UserGame : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal PricePaid { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserGame(Guid id)
        : base(id)
    {
    }

    public static Result<UserGame> Purchase(Guid userId, Guid gameId, decimal pricePaid)
    {
        if (pricePaid < 0)
            return Result.Failure<UserGame>(new Error("UserGame.InvalidPrice", "O preço pago não pode ser negativo."));

        var userGame = new UserGame(Guid.NewGuid())
        {
            UserId = userId,
            GameId = gameId,
            PricePaid = pricePaid,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(userGame);
    }
}
