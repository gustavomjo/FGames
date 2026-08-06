using FGames.Modules.Games.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Games.Application.Commands;

public sealed record PublishGameCommand(Guid GameId) : IRequest<Result<GameDto>>;
