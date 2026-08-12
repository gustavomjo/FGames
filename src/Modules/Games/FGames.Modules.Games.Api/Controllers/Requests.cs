using FGames.Modules.Games.Domain.Enums;

namespace FGames.Modules.Games.Api.Controllers;

public sealed record CreateGameRequest(string Name, string? Description, GameCategory Category, AgeRating Rating, decimal Price);

public sealed record UpdateGameRequest(string Name, string? Description, GameCategory Category, AgeRating Rating, decimal Price);
