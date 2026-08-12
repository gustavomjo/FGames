using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Domain.Enums;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Commands;

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    DateOnly? BirthDate,
    Guid CreatedByUserId,
    Role Role) : IRequest<Result<UserDto>>;
