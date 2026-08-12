using FGames.SharedKernel;
using FGames.Modules.Users.Application.DTOs;
using MediatR;

namespace FGames.Modules.Users.Application.Commands;

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Password,
    DateOnly? BirthDate,
    string? CreationIp) : IRequest<Result<UserDto>>;
