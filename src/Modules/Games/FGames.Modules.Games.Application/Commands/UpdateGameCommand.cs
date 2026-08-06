using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Domain.Enums;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Commands;

public sealed record UpdateGameCommand(
    Guid GameId,
    string Name,
    string? Description,
    GameCategory Category,
    AgeRating Rating,
    decimal Price) : IRequest<Result<GameDto>>;
