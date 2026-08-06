using FGames.Modules.Users.Domain.Enums;

namespace FGames.Modules.Users.Application.Interfaces;

public interface ITokenGenerator
{
    TokenResult GenerateToken(Guid userId, string email, Role role);
}

public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);
