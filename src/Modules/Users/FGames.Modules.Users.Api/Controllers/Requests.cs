using FGames.Modules.Users.Domain.Enums;

namespace FGames.Modules.Users.Api.Controllers;

public sealed record RegisterUserRequest(string Name, string Email, string Password, DateOnly? BirthDate);

public sealed record LoginRequest(string Email, string Password);

public sealed record CreateUserRequest(string Name, string Email, string Password, DateOnly? BirthDate, Role Role);

public sealed record SetUserStatusRequest(UserStatus Status);
