using FGames.Modules.Users.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResultDto>>;
