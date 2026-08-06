using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Domain.Enums;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Commands;

public sealed record SetUserStatusCommand(Guid UserId, UserStatus Status) : IRequest<Result<UserDto>>;
