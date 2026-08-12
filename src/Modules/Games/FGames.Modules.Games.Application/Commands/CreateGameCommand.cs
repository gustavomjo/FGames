using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Domain.Enums;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Commands;

public sealed record CreateGameCommand(
    string Name,
    string? Description,
    GameCategory Category,
    AgeRating Rating,
    decimal Price,
    Guid CreatedByUserId) : IRequest<Result<GameDto>>;
