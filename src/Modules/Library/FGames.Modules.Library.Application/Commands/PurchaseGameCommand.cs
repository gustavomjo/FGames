using FGames.Modules.Library.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Library.Application.Commands;

public sealed record PurchaseGameCommand(Guid UserId, Guid GameId) : IRequest<Result<UserGameDto>>;
