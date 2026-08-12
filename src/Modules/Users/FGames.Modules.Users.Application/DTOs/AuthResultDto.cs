namespace FGames.Modules.Users.Application.DTOs;

public sealed record AuthResultDto(string AccessToken, DateTime ExpiresAtUtc, UserDto User);
