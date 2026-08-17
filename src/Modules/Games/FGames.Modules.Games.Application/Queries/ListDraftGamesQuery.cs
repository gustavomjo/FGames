using FGames.Modules.Games.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Queries;

public sealed record ListDraftGamesQuery : IRequest<Result<IReadOnlyList<GameDto>>>;
