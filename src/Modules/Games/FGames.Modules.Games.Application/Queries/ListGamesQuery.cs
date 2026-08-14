using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Domain.Enums;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Queries;

public sealed record ListGamesQuery(GameStatus? Status) : IRequest<Result<IReadOnlyList<GameDto>>>;
