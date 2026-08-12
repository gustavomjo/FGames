using FGames.Modules.Users.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FGames.Modules.Users.Infrastructure.Auth;

public sealed class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password) => _passwordHasher.HashPassword(new object(), password);

    public bool Verify(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
