using FGames.Modules.Games.Domain.Enums;
using FGames.SharedKernel;

namespace FGames.Modules.Games.Domain.Entities;

public sealed class Game : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public GameCategory Category { get; private set; }
    public AgeRating Rating { get; private set; }
    public GameStatus Status { get; private set; }
    public decimal Price { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Game(Guid id)
        : base(id)
    {
    }

    public static Result<Game> Create(
        string name,
        string? description,
        GameCategory category,
        AgeRating rating,
        decimal price,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Game>(new Error("Game.NameRequired", "O nome do jogo é obrigatório."));

        if (price < 0)
            return Result.Failure<Game>(new Error("Game.InvalidPrice", "O preço do jogo não pode ser negativo."));

        var game = new Game(Guid.NewGuid())
        {
            Name = name,
            Description = description,
            Category = category,
            Rating = rating,
            Status = GameStatus.Draft,
            Price = price,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(game);
    }

    public Result Update(string name, string? description, GameCategory category, AgeRating rating, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Game.NameRequired", "O nome do jogo é obrigatório."));

        if (price < 0)
            return Result.Failure(new Error("Game.InvalidPrice", "O preço do jogo não pode ser negativo."));

        Name = name;
        Description = description;
        Category = category;
        Rating = rating;
        Price = price;

        return Result.Success();
    }

    public Result Publish()
    {
        if (Status == GameStatus.Published)
            return Result.Failure(new Error("Game.AlreadyPublished", "O jogo já está publicado."));

        Status = GameStatus.Published;
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == GameStatus.Inactive)
            return Result.Failure(new Error("Game.AlreadyInactive", "O jogo já está inativo."));

        Status = GameStatus.Inactive;
        return Result.Success();
    }
}
