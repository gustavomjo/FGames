using FGames.Modules.Users.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Queries;

public sealed record ListUsersQuery : IRequest<Result<IReadOnlyList<UserDto>>>;
